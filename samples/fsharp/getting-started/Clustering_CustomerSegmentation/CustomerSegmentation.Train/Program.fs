// Learn more about F# at http://fsharp.org

open System
open Common.ConsoleHelper
open CustomerSegmentation
open CustomerSegmentation.DataStructures
open System.IO
open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Transforms
open Microsoft.ML.Transforms.Categorical
open Microsoft.ML.Transforms.Projections

[<EntryPoint>]
let main argv =

    let assetsPath = ModelHelpers.GetAssetsPath([|"../../../assets"|])

    let transactionsCsv = Path.Combine(assetsPath, "inputs", "transactions.csv")
    let offersCsv = Path.Combine(assetsPath, "inputs", "offers.csv")
    let pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv")
    let modelZip = Path.Combine(assetsPath, "outputs", "retailClustering.zip")

    try 
        //STEP 0: Special data pre-process in this sample creating PivotTable csv file
        DataHelper.PreProcessAndSave offersCsv transactionsCsv pivotCsv |> ignore

        //Create the MLContext to share across components for deterministic results
        let mlContext = MLContext(seed = Nullable(1))

        //STEP 1: Common data loading configuration
        let textLoader = CustomerSegmentationTextLoaderFactory.CreateTextLoader mlContext
        let pivotDataView = textLoader.Read pivotCsv

        //STEP 2: Configure data tranformations in pipeline
        let dataProcessPipeline = 
            PrincipalComponentAnalysisEstimator(mlContext, "Features", "PCAFeatures", null, 2, 0, false, Nullable())
            |> Common.ModelBuilder.append(OneHotEncodingEstimator(mlContext, [|OneHotEncodingEstimator.ColumnInfo("LastName","LastNameKey", CategoricalTransform.OutputKind.Ind)|]))
            |> Common.ConsoleHelper.downcastPipeline


        // (Optional) Peek data in training Data View after applying the ProcessPipeline's transformations
        //TODO: getting error here
        //peekDataViewInConsole<PivotObservation> mlContext pivotDataView dataProcessPipeline 10 |> ignore 
        peekVectorColumnDataInConsole mlContext "Features" pivotDataView dataProcessPipeline 10 |> ignore

        //STEP 3: Create and train the model
        let trainer = mlContext.Clustering.Trainers.KMeans("Features", clustersCount = 3)
        let modelBuilder = 
            Common.ModelBuilder.create mlContext dataProcessPipeline
            |> Common.ModelBuilder.addTrainer trainer

        printfn "=============== Training the model ==============="
        let trainedModel = 
            modelBuilder |> Common.ModelBuilder.train pivotDataView

        //STEP 4: Evaluate accuracy of the model
        printfn "===== Evaluating Model's accuracy with Test data ====="
        let metrics = (trainedModel, modelBuilder) |> Common.ModelBuilder.evaluateClusteringModel pivotDataView
        printClusteringMetrics (trainer.ToString()) metrics

        //STEP 5: Save / persist the model as .zip file
        printfn "=============== Saving the model to a file ==============="
        (trainedModel, modelBuilder) |> Common.ModelBuilder.saveModelAsFile modelZip

    with exn -> 
        consoleWriteException [|exn.Message|]

    consolePressAnyKey()
    0
