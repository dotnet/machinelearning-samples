// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Runtime.Data
open SentimentAnalysis.DataStructures.Model


let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data"
let trainDataPath = sprintf @"%s/wikipedia-detox-250-line-data.tsv" baseDatasetsLocation
let testDataPath = sprintf @"%s/wikipedia-detox-250-line-test.tsv" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels";
let modelPath = sprintf @"%s/SentimentModel.zip" baseModelsPath

let dataLoader (mlContext : MLContext) =
    mlContext.Data.TextReader(
        TextLoader.Arguments(
            Separator = "tab",
            HasHeader = true,
            Column = 
                [|
                    TextLoader.Column("Label", Nullable DataKind.Bool, 0)
                    TextLoader.Column("Text", Nullable DataKind.Text, 1)
                |]
        )
    )

let read (dataPath : string) (dataLoader : TextLoader) =
    dataLoader.Read dataPath

let buildTrainEvaluateAndSaveModel (mlContext : MLContext) =
    // STEP 1: Common data loading configuration
    let trainingDataView = 
        dataLoader mlContext
        |> read trainDataPath

    let testDataView = 
        dataLoader mlContext
        |> read testDataPath

    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline =
        mlContext.Transforms.Text.FeaturizeText("Text", "Features")

    // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<SentimentIssue> mlContext trainingDataView dataProcessPipeline 2 |> ignore

    // STEP 3: Set the training algorithm, then create and config the modelBuilder
    let trainer = mlContext.BinaryClassification.Trainers.FastTree(label = "Label", features = "Features")
    let modelBuilder = 
        Common.ModelBuilder.create mlContext dataProcessPipeline
        |> Common.ModelBuilder.addTrainer trainer

    // STEP 4: Train the model fitting to the DataSet
    printfn "=============== Training the model ==============="
    let trainedModel = 
        modelBuilder
        |> Common.ModelBuilder.train trainingDataView

    // STEP 5: Evaluate the model and show accuracy stats
    printfn "===== Evaluating Model's accuracy with Test data ====="
    let metrics = 
        (trainedModel, modelBuilder)
        |> Common.ModelBuilder.evaluateBinaryClassificationModel testDataView "Label" "Score"

    Common.ConsoleHelper.printBinaryClassificationMetrics (trainer.ToString()) metrics

    // STEP 6: Save/persist the trained model to a .ZIP file
    printfn "=============== Saving the model to a file ==============="
    (trainedModel, modelBuilder)
    |> Common.ModelBuilder.saveModelAsFile modelPath

let testSinglePrediction (mlContext : MLContext) =
    // (OPTIONAL) Try/test a single prediction by loding the model from the file, first.
    let sampleStatement = { Text = "This is a very rude movie" }
    
    let resultprediction = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.loadModelFromZipFile modelPath
        |> Common.ModelScorer.predictSingle sampleStatement

    printfn "=============== Single Prediction  ==============="
    printfn "Text: %s | Prediction: %s sentiment | Probability: %f "
        sampleStatement.Text
        (if Convert.ToBoolean(resultprediction.Prediction) then "Toxic" else "Nice")
        resultprediction.Probability
    printfn "=================================================="
    

[<EntryPoint>]
let main argv =
    
    //Create MLContext to be shared across the model creation workflow objects 
    //Set a random seed for repeatable/deterministic results across multiple trainings.
    let mlContext = MLContext(seed = Nullable 1)

    // Create, Train, Evaluate and Save a model
    buildTrainEvaluateAndSaveModel mlContext
    Common.ConsoleHelper.consoleWriteHeader "=============== End of training processh ==============="

    // Make a single test prediction loding the model from .ZIP file
    testSinglePrediction mlContext

    Common.ConsoleHelper.consoleWriteHeader "=============== End of process, hit any key to finish ==============="
    Common.ConsoleHelper.consolePressAnyKey()

    0 // return an integer exit code
