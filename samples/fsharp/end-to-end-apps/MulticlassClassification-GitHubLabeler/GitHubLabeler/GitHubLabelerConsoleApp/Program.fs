// Learn more about F# at http://fsharp.org

open Common
open System
open System.IO
open Microsoft.Extensions.Configuration
open Microsoft.ML
open DataStructures
open Microsoft.ML.Data
open System.Security.Cryptography
open Microsoft.ML.Trainers


let repoOwner = "a"
let repoName = "a"
let accessToken = "a"


let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data"
let dataSetLocation = sprintf @"%s/corefx-issues-train.tsv" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels"
let modelFilePathName = sprintf @"%s/GitHubLabelerModel.zip" baseModelsPath

type MyTrainerStrategy = | SdcaMultiClassTrainer | OVAAveragedPerceptronTrainer 


let setupAppConfiguration () =
    let builder = ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json")
    builder.Build()

let downcastPipeline (x : IEstimator<_>) = 
    match x with 
    | :? IEstimator<ITransformer> as y -> y
    | _ -> failwith "downcastPipeline: expecting a IEstimator<ITransformer>"

let buildAndTrainModel dataSetLocation modelPath selectedStrategy =

    // Create MLContext to be shared across the model creation workflow objects 
    // Set a random seed for repeatable/deterministic results across multiple trainings.
    let mlContext = MLContext(seed = Nullable 0)

    // STEP 1: Common data loading configuration
    let textLoader =
        mlContext.Data.CreateTextLoader(
            separatorChar = '\t',
            hasHeader = true,
            columns = 
                [|
                    TextLoader.Column("ID", DataKind.String, 0)
                    TextLoader.Column("Area", DataKind.String, 1)
                    TextLoader.Column("Title", DataKind.String, 2)
                    TextLoader.Column("Description", DataKind.String, 3)
                |]
        )

    let trainingDataView = textLoader.Load([| dataSetLocation |])
       
    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline = 
        EstimatorChain()
            .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", "Area"))
            .Append(mlContext.Transforms.Text.FeaturizeText("TitleFeaturized", "Title"))
            .Append(mlContext.Transforms.Text.FeaturizeText("DescriptionFeaturized", "Description"))
            .Append(mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"))
            .AppendCacheCheckpoint(mlContext)
        |> downcastPipeline

    // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<DataStructures.GitHubIssue> mlContext trainingDataView dataProcessPipeline 2 |> ignore
    //Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 2 |> ignore
    
    // STEP 3: Create the selected training algorithm/trainer
    let trainer =
        match selectedStrategy with
        | MyTrainerStrategy.SdcaMultiClassTrainer ->
            mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy( "Label", "Features") |> downcastPipeline

        | MyTrainerStrategy.OVAAveragedPerceptronTrainer ->
            let averagedPerceptronBinaryTrainer = 
                mlContext.BinaryClassification.Trainers.AveragedPerceptron( "Label", "Features", numberOfIterations = 10)
                
            let downcastTrainer (x : ITrainerEstimator<_,_>) = 
                match x with 
                | :? ITrainerEstimator<_,_> as y -> y
                | _ -> failwith "downcastPipeline: expecting a ITrainerEstimator"
            
            let averagedPerceptronBinaryTrainer' = downcastTrainer averagedPerceptronBinaryTrainer

            // Compose an OVA (One-Versus-All) trainer with the BinaryTrainer.
            // In this strategy, a binary classification algorithm is used to train one classifier for each class, "
            // which distinguishes that class from all other classes. Prediction is then performed by running these binary classifiers, "
            // and choosing the prediction with the highest confidence score.
            mlContext.MulticlassClassification.Trainers.OneVersusAll(averagedPerceptronBinaryTrainer')
            |> downcastPipeline

    //Set the trainer/algorithm
    let modelBuilder = 
        dataProcessPipeline
            .Append(trainer)
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
  
    // STEP 4: Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
    // in order to evaluate and get the model's accuracy metrics
    printfn "=============== Cross-validating to get model's accuracy metrics ==============="

    let crossValidationResults = 
        mlContext.MulticlassClassification.CrossValidate(data = trainingDataView, estimator = downcastPipeline modelBuilder, numberOfFolds = 6, labelColumnName = "Label")
                   
    crossValidationResults
    |> Seq.toArray
    |> Common.ConsoleHelper.printMulticlassClassificationFoldsAverageMetrics (trainer.ToString()) 

    // STEP 5: Train the model fitting to the DataSet
    printfn "=============== Training the model ==============="
    let trainedModel = modelBuilder.Fit(trainingDataView)


    // (OPTIONAL) Try/test a single prediction with the "just-trained model" (Before saving the model)
    let issue = { 
        ID = "Any-ID"
        Area = ""
        Title = "WebSockets communication is slow in my machine"
        Description = "The WebSockets communication used under the covers by SignalR looks like is going slow in my development machine.." 
    }
    let predEngine = mlContext.Model.CreatePredictionEngine<GitHubIssue, GitHubIssuePrediction>(trainedModel)
    let prediction =  predEngine.Predict(issue)

    printfn "=============== Single Prediction just-trained-model - Result: %s ===============" prediction.Area

    // STEP 6: Save/persist the trained model to a .ZIP file
    printfn "=============== Saving the model to a file ==============="
    do 
        use f = File.Open(modelPath,FileMode.Create)
        mlContext.Model.Save(trainedModel, trainingDataView.Schema, f)

    Common.ConsoleHelper.consoleWriteHeader "Training process finalized"

let testSingleLabelPrediction (configuration : IConfiguration) modelFilePathName =
    let token =     configuration.["GitHubToken"];
    let repoOwner = configuration.["GitHubRepoOwner"]; //IMPORTANT: This can be a GitHub User or a GitHub Organization
    let repoName =  configuration.["GitHubRepoName"];

    
    Labeler.initialise modelFilePathName repoOwner repoName token
    |> Labeler.testPredictionForSingleIssue

let predictLabelsAndUpdateGitHub (configuration : IConfiguration) modelPath =
    printfn ".............Retrieving Issues from GITHUB repo, predicting label/s and assigning predicted label/s......"
    
    let token =     configuration.["GitHubToken"];
    let repoOwner = configuration.["GitHubRepoOwner"]; //IMPORTANT: This can be a GitHub User or a GitHub Organization
    let repoName =  configuration.["GitHubRepoName"];

    if String.IsNullOrEmpty(token) || token = "YOUR - GUID - GITHUB - TOKEN" ||
            String.IsNullOrEmpty(repoOwner) || repoOwner = "YOUR-REPO-USER-OWNER-OR-ORGANIZATION" ||
            String.IsNullOrEmpty(repoName) || repoName = "YOUR-REPO-SINGLE-NAME" then
        Console.Error.WriteLine()
        Console.Error.WriteLine("Error: please configure the credentials in the appsettings.json file")
        
    else
        
        Labeler.initialise modelPath repoOwner repoName token
        |> Labeler.LabelAllNewIssuesInGitHubRepo
        printfn "Labeling completed"


[<EntryPoint>]
let main argv =
    let configuration = setupAppConfiguration()

    //1. ChainedBuilderExtensions and Train the model
    buildAndTrainModel dataSetLocation modelFilePathName MyTrainerStrategy.SdcaMultiClassTrainer
    
    //2. Try/test to predict a label for a single hard-coded Issue
    testSingleLabelPrediction configuration modelFilePathName


    //3. Predict Issue Labels and apply into a real GitHub repo
    // (Comment the next line if no real access to GitHub repo) 
    predictLabelsAndUpdateGitHub configuration modelFilePathName

    Common.ConsoleHelper.consolePressAnyKey()

    0 // return an integer exit code
