// Learn more about F# at http://fsharp.org

open Common
open System
open System.IO
open Microsoft.Extensions.Configuration
open Microsoft.ML
open Microsoft.ML.Runtime.Data
open Microsoft.ML.Runtime.Learners
open Microsoft.ML.Runtime.Training
open Microsoft.ML.Runtime
open DataStructures


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


let buildAndTrainModel dataSetLocation modelPath selectedStrategy =

    // Create MLContext to be shared across the model creation workflow objects 
    // Set a random seed for repeatable/deterministic results across multiple trainings.
    let mlContext = MLContext(seed = Nullable 0)

    // STEP 1: Common data loading configuration
    let textLoader =
        mlContext.Data.TextReader(
            TextLoader.Arguments(
                Separator = "tab",
                HasHeader = true,
                Column = 
                    [|

                        TextLoader.Column("ID", Nullable DataKind.Text, 0)
                        TextLoader.Column("Area", Nullable DataKind.Text, 1)
                        TextLoader.Column("Title", Nullable DataKind.Text, 2)
                        TextLoader.Column("Description", Nullable DataKind.Text, 3)
                    |]
            )
        )

    let trainingDataView = textLoader.Read([| dataSetLocation |])
       
    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline = 
        mlContext.Transforms.Categorical.MapValueToKey("Area", "Label")
            |> Common.ModelBuilder.append (mlContext.Transforms.Text.FeaturizeText("Title", "TitleFeaturized"))
            |> Common.ModelBuilder.append (mlContext.Transforms.Text.FeaturizeText("Description", "DescriptionFeaturized"))
            |> Common.ModelBuilder.append (mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"))
            |> Common.ModelBuilder.downcastPipeline

    // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<DataStructures.GitHubIssue> mlContext trainingDataView dataProcessPipeline 2 |> ignore
    //Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 2 |> ignore
    
    // STEP 3: Create the selected training algorithm/trainer
    let trainer =
        match selectedStrategy with
        | MyTrainerStrategy.SdcaMultiClassTrainer ->
            mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(
                DefaultColumnNames.Label, 
                DefaultColumnNames.Features)
            |> ModelBuilder.downcastPipeline

        | MyTrainerStrategy.OVAAveragedPerceptronTrainer ->
            let averagedPerceptronBinaryTrainer = 
                mlContext.BinaryClassification.Trainers.AveragedPerceptron(
                    DefaultColumnNames.Label,
                    DefaultColumnNames.Features,
                    numIterations = 10)
                
            // Because of variant generics used in the C# model the trainer has to be downcasted
            // to ITrainerEstimator<ISingleFeaturePredictionTransformer<IPredictorProducing<float32>>, IPredictorProducing<float32>>
            // type, otherwise F# won't allow it.
            let downcastTrainer (a: ITrainerEstimator<'a, 'b>) =
                match a with
                | :? ITrainerEstimator<ISingleFeaturePredictionTransformer<IPredictorProducing<float32>>, IPredictorProducing<float32>> as p -> p
                | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

            let averagedPerceptronBinaryTrainer' = downcastTrainer averagedPerceptronBinaryTrainer

            // Compose an OVA (One-Versus-All) trainer with the BinaryTrainer.
            // In this strategy, a binary classification algorithm is used to train one classifier for each class, "
            // which distinguishes that class from all other classes. Prediction is then performed by running these binary classifiers, "
            // and choosing the prediction with the highest confidence score.
            new Ova(mlContext, averagedPerceptronBinaryTrainer')
            |> ModelBuilder.downcastPipeline

    //Set the trainer/algorithm
    let modelBuilder = 
        Common.ModelBuilder.create mlContext dataProcessPipeline
        |> Common.ModelBuilder.addTrainer trainer
        |> Common.ModelBuilder.addEstimator (mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))

    
    // STEP 4: Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
    // in order to evaluate and get the model's accuracy metrics
    printfn "=============== Cross-validating to get model's accuracy metrics ==============="
    let crossValResults = 
        modelBuilder
        |> ModelBuilder.crossValidateAndEvaluateMulticlassClassificationModel trainingDataView 6 "Label"
        |> Array.map(fun struct (a,b,c) -> (a,b,c))
        
    Common.ConsoleHelper.printMulticlassClassificationFoldsAverageMetrics (trainer.ToString()) crossValResults

    // STEP 5: Train the model fitting to the DataSet
    printfn "=============== Training the model ==============="
    let trainedModel = 
        modelBuilder
        |> Common.ModelBuilder.train trainingDataView


    // (OPTIONAL) Try/test a single prediction with the "just-trained model" (Before saving the model)
    let issue = { 
        ID = "Any-ID"
        Area = ""
        Title = "WebSockets communication is slow in my machine"
        Description = "The WebSockets communication used under the covers by SignalR looks like is going slow in my development machine.." 
    }
    let prediction = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.setTrainedModel trainedModel
        |> Common.ModelScorer.predictSingle issue

    printfn "=============== Single Prediction just-trained-model - Result: %s ===============" prediction.Area

    // STEP 6: Save/persist the trained model to a .ZIP file
    printfn "=============== Saving the model to a file ==============="
    (trainedModel, modelBuilder)
    |> Common.ModelBuilder.saveModelAsFile modelPath

    Common.ConsoleHelper.consoleWriteHeader "Training process finalized"

let testSingleLabelPrediction (configuration : IConfiguration) modelFilePathName =
    let token =     configuration.["GitHubToken"];
    let repoOwner = configuration.["GitHubRepoOwner"]; //IMPORTANT: This can be a GitHub User or a GitHub Organization
    let repoName =  configuration.["GitHubRepoName"];

    
    Labeler.initialise modelFilePathName repoOwner repoName token
    |> Labeler.testPredictionForSingleIssue

let predictLabelsAndUpdateGitHub (configuration : IConfiguration) modelPath =
    let token =     configuration.["GitHubToken"];
    let repoOwner = configuration.["GitHubRepoOwner"]; //IMPORTANT: This can be a GitHub User or a GitHub Organization
    let repoName =  configuration.["GitHubRepoName"];

    if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(repoOwner) || String.IsNullOrEmpty(repoName)) then
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
