// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Runtime.Data
open Clustering_Iris.DataStructures
open DataStructures

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data"
let dataPath = sprintf @"%s/iris-full.txt" baseDatasetsLocation
let baseModelsPath = @"../../../../MLModels"
let modelPath = sprintf @"%s/IrisModel.zip" baseModelsPath

let dataLoader (mlContext : MLContext) =
    mlContext.Data.TextReader(
        TextLoader.Arguments(
            Separator = "\t",
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


[<EntryPoint>]
let main argv =

    let mlContext = MLContext(seed = Nullable 1)

    //STEP 1: Common data loading
    let fullData = 
        dataLoader mlContext
        |> read dataPath

    let struct (trainingDataView, testingDataView) = mlContext.Clustering.TrainTestSplit(fullData, testFraction = 0.2)

    //STEP 2: Process data transformations in pipeline
    let dataProcessPipeline =
        mlContext.Transforms.Concatenate("Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth")

    // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
    Common.ConsoleHelper.peekDataViewInConsole<IrisData> mlContext trainingDataView dataProcessPipeline 10 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 10 |> ignore

    let trainer = mlContext.Clustering.Trainers.KMeans(features = "Features", clustersCount = 3)

    // STEP 3: Create and train the model                
    let modelBuilder = 
        Common.ModelBuilder.create mlContext dataProcessPipeline
        |> Common.ModelBuilder.addTrainer trainer

    let trainedModel = 
        modelBuilder
        |> Common.ModelBuilder.train trainingDataView

    // STEP4: Evaluate accuracy of the model
    let metrics = 
        (trainedModel, modelBuilder)
        |> Common.ModelBuilder.evaluateClusteringModel testingDataView
    Common.ConsoleHelper.printClusteringMetrics (trainer.ToString()) metrics

    // STEP5: Save/persist the model as a .ZIP file
    (trainedModel, modelBuilder)
    |> Common.ModelBuilder.saveModelAsFile modelPath

    printfn "=============== End of training process ==============="

    printfn "=============== Predict a cluster for a single case (Single Iris data sample) ==============="

    // Test with one sample text 
    let sampleIrisData = 
        {
            SepalLength = 3.3f
            SepalWidth = 1.6f
            PetalLength = 0.2f
            PetalWidth = 5.1f
        }

    //Create the clusters: Create data files and plot a chart
    let prediction = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.loadModelFromZipFile modelPath
        |> Common.ModelScorer.predictSingle sampleIrisData

    printfn "Cluster assigned for setosa flowers: %d" prediction.SelectedClusterId

    Common.ConsoleHelper.consolePressAnyKey ()

    0 // return an integer exit code
