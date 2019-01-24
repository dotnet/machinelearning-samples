// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open MulticlassClassification_Iris.DataStructures
open Common
open MulticlassClassification_Iris

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data";
let trainDataPath = sprintf @"%s/iris-train.txt" baseDatasetsLocation
let testDataPath = sprintf @"%s/iris-test.txt" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels"
let modelPath = sprintf @"%s/IrisClassificationModel.zip" baseModelsPath


let buildTrainEvaluateAndSaveModel (mlContext : MLContext) =
    
    // STEP 1: Common data loading configuration
    let textLoader = 
        mlContext.Data.CreateTextReader(
            separatorChar = '\t',
            hasHeader = true,
            columns = 
                [|
                    TextLoader.Column("Label", Nullable DataKind.R4, 0)
                    TextLoader.Column("SepalLength", Nullable DataKind.R4, 1)
                    TextLoader.Column("SepalWidth", Nullable DataKind.R4, 2)
                    TextLoader.Column("PetalLength", Nullable DataKind.R4, 3)
                    TextLoader.Column("PetalWidth", Nullable DataKind.R4, 4)
                |]
        )

    let trainingDataView = textLoader.Read trainDataPath
    let testDataView = textLoader.Read testDataPath
    
    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline =
        mlContext.Transforms.Concatenate("Features", [| "SepalLength"; "SepalWidth"; "PetalLength"; "PetalWidth"|])
        |> Common.ModelBuilder.appendCacheCheckpoint mlContext

    // STEP 3: Set the training algorithm, then create and config the modelBuilder
    let trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn = "Label", featureColumn = "Features")
    let modelBuilder = 
        Common.ModelBuilder.create mlContext dataProcessPipeline
        |> Common.ModelBuilder.addTrainer trainer

    // STEP 4: Train the model fitting to the DataSet
    //Measure training time
    let watch = System.Diagnostics.Stopwatch.StartNew()

    printfn "=============== Training the model ==============="
    let trainedModel = 
        modelBuilder
        |> Common.ModelBuilder.train trainingDataView

    //Stop measuring time
    watch.Stop()
    let elapsedMs = watch.ElapsedMilliseconds
    printfn "***** Training time: %f seconds *****" ((float)elapsedMs/1000.0) 

    // STEP 5: Evaluate the model and show accuracy stats
    printfn "===== Evaluating Model's accuracy with Test data ====="
    let metrics = 
        (trainedModel, modelBuilder)
        |> Common.ModelBuilder.evaluateMultiClassClassificationModel testDataView "Label" "Score"

    Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metrics

    // STEP 6: Save/persist the trained model to a .ZIP file
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
