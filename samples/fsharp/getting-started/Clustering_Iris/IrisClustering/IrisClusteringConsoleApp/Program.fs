﻿// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open Clustering_Iris.DataStructures
open DataStructures
open Microsoft.Data.DataView

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data"
let dataPath = sprintf @"%s/iris-full.txt" baseDatasetsLocation
let baseModelsPath = @"../../../../MLModels"
let modelPath = sprintf @"%s/IrisModel.zip" baseModelsPath


[<EntryPoint>]
let main argv =

    //Create the MLContext to share across components for deterministic results
    let mlContext = MLContext(seed = Nullable 1)    //Seed set to any number so you have a deterministic environment

    // STEP 1: Common data loading configuration
    let fullData = 
        mlContext.Data.ReadFromTextFile(dataPath,
            hasHeader = true,
            separatorChar = '\t',
            columns =
                [|
                    TextLoader.Column("Label", Nullable DataKind.R4, 0)
                    TextLoader.Column("SepalLength", Nullable DataKind.R4, 1)
                    TextLoader.Column("SepalWidth", Nullable DataKind.R4, 2)
                    TextLoader.Column("PetalLength", Nullable DataKind.R4, 3)
                    TextLoader.Column("PetalWidth", Nullable DataKind.R4, 4)
                |]
        )
    
    //Split dataset in two parts: TrainingDataset (80%) and TestDataset (20%)
    let struct(trainingDataView, testingDataView) = mlContext.Clustering.TrainTestSplit(fullData, testFraction = 0.2)

    //STEP 2: Process data transformations in pipeline
    let dataProcessPipeline = 
        mlContext.Transforms.Concatenate("Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth")
        
    // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
    Common.ConsoleHelper.peekDataViewInConsole<IrisData> mlContext trainingDataView dataProcessPipeline 10 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 10 |> ignore

    // STEP 3: Create and train the model     
    let trainer = mlContext.Clustering.Trainers.KMeans(featureColumn = "Features", clustersCount = 3)
    let trainingPipeline = dataProcessPipeline.Append(trainer)
    let trainedModel = trainingPipeline.Fit(trainingDataView)

    // STEP4: Evaluate accuracy of the model
    let (predictions : IDataView) = trainedModel.Transform(testingDataView)
    let metrics = mlContext.Clustering.Evaluate(predictions, score = "Score", features = "Features")

    Common.ConsoleHelper.printClusteringMetrics (trainer.ToString()) metrics


    // STEP5: Save/persist the model as a .ZIP file
    use fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
    mlContext.Model.Save(trainedModel, fs)
    fs.Close()

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

    use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
    let model = mlContext.Model.Load(stream)
    // Create prediction engine related to the loaded trained model
    let predEngine = model.CreatePredictionEngine<IrisData, IrisPrediction>(mlContext)

    //Score
    let resultprediction = predEngine.Predict(sampleIrisData)

    printfn "Cluster assigned for setosa flowers: %d" resultprediction.SelectedClusterId

    printfn "=============== End of process, hit any key to finish ==============="

    Console.ReadKey() |> ignore

    0 // return an integer exit code
