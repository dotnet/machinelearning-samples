// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Runtime.Data
open MulticlassClassification_Iris.DataStructures
open Common
open MulticlassClassification_Iris

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data";
let trainDataPath = sprintf @"%s/iris-train.txt" baseDatasetsLocation
let testDataPath = sprintf @"%s/iris-test.txt" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels"
let modelPath = sprintf @"%s/IrisClassificationModel.zip" baseModelsPath


let dataLoader (mlContext : MLContext) =
    mlContext.Data.TextReader(
        TextLoader.Arguments(
            Separator = "tab",
            HasHeader = true,
            Column = 
                [|
                    TextLoader.Column("Label", Nullable DataKind.R4, 0)
                    TextLoader.Column("SepalLength", Nullable DataKind.R4, 1)
                    TextLoader.Column("SepalWidth", Nullable DataKind.R4, 2)
                    TextLoader.Column("PetalLength", Nullable DataKind.R4, 3)
                    TextLoader.Column("PetalWidth", Nullable DataKind.R4, 4)
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
        mlContext.Transforms.Concatenate("Features", [| "SepalLength"; "SepalWidth"; "PetalLength"; "PetalWidth"|])

    // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<IrisData> mlContext trainingDataView dataProcessPipeline 5 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 5 |> ignore

    // STEP 3: Set the training algorithm, then create and config the modelBuilder
    let trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(label = "Label", features = "Features")
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
        |> Common.ModelBuilder.evaluateMultiClassClassificationModel testDataView "Label" "Score"

    Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metrics

    // STEP 6: Save/persist the trained model to a .ZIP file
    printfn "=============== Saving the model to a file ==============="
    (trainedModel, modelBuilder)
    |> Common.ModelBuilder.saveModelAsFile modelPath



let testSomePredictions (mlContext : MLContext) =

    //Test Classification Predictions with some hard-coded samples 
    let modelScorer = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.loadModelFromZipFile modelPath
        
    let prediction = modelScorer |> Common.ModelScorer.predictSingle DataStructures.TestIrisData.Iris1
    printfn "Actual: setosa.     Predicted probability: setosa:      %.4f" prediction.Score.[0]
    printfn "                                           versicolor:  %.4f" prediction.Score.[1]
    printfn "                                           virginica:   %.4f" prediction.Score.[2]
    printfn ""

    let prediction = modelScorer |> Common.ModelScorer.predictSingle DataStructures.TestIrisData.Iris2
    printfn "Actual: virginica.  Predicted probability: setosa:      %.4f" prediction.Score.[0]
    printfn "                                           versicolor:  %.4f" prediction.Score.[1]
    printfn "                                           virginica:   %.4f" prediction.Score.[2]
    printfn ""

    let prediction = modelScorer |> Common.ModelScorer.predictSingle DataStructures.TestIrisData.Iris3
    printfn "Actual: versicolor. Predicted probability: setosa:      %.4f" prediction.Score.[0]
    printfn "                                           versicolor:  %.4f" prediction.Score.[1]
    printfn "                                           virginica:   %.4f" prediction.Score.[2]
    printfn ""


[<EntryPoint>]
let main argv =
    
    // Create MLContext to be shared across the model creation workflow objects 
    // Set a random seed for repeatable/deterministic results across multiple trainings.
    let mlContext = MLContext(seed = Nullable 0)

    //1.
    buildTrainEvaluateAndSaveModel mlContext

    //2.
    testSomePredictions mlContext

    printfn "=============== End of process, hit any key to finish ==============="
    ConsoleHelper.consolePressAnyKey()

    0 // return an integer exit code
