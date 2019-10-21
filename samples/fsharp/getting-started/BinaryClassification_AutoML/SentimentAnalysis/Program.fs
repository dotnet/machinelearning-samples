open System
open System.IO
open System.Linq
open Common
open Microsoft.ML
open Microsoft.ML.AutoML
open Microsoft.ML.Data

[<CLIMutable>]
type SentimentIssue = 
    {
        [<LoadColumn(0)>]
        Label : bool
        [<LoadColumn(1)>]
        Text : string
    }
[<CLIMutable>]
type SentimentPrediction = 
    {
        [<ColumnName("PredictedLabel")>]
        Prediction : bool
        Score : float32
    }    

let assemblyFolderPath = Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName
let absolutePath x = Path.Combine(assemblyFolderPath, x)

let baseDatasetsRelativePath = @"Data"

let trainDataRelativePath = Path.Combine(baseDatasetsRelativePath, "wikipedia-detox-250-line-data.tsv")
let trainDataPath = absolutePath trainDataRelativePath

let testDataRelativePath = Path.Combine(baseDatasetsRelativePath, "wikipedia-detox-250-line-test.tsv")
let testDataPath = absolutePath testDataRelativePath

let baseModelsRelativePath = @"../../../MLModels"
let modelRelativePath = Path.Combine(baseModelsRelativePath, "SentimentModel.zip")
let modelPath = absolutePath modelRelativePath

let experimentTimeInSeconds = 60u

let mlContext = MLContext()


// Create, train, evaluate and save a model
// STEP 1: Load data
let trainingDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(trainDataPath, hasHeader = true)
let testDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(testDataPath, hasHeader = true)

// STEP 2: Display first few rows of training data
ConsoleHelper.showDataViewInConsole mlContext trainingDataView 4

// STEP 3: Initialize our user-defined progress handler that AutoML will 
// invoke after each model it produces and evaluates.
let progressHandler = ConsoleHelper.binaryExperimentProgressHandler()

// STEP 4: Run AutoML binary classification experiment
ConsoleHelper.consoleWriteHeader("=============== Running AutoML experiment ===============")
printfn "Running AutoML binary classification experiment for %d seconds..." experimentTimeInSeconds
let experimentResult = mlContext.Auto().CreateBinaryClassificationExperiment(experimentTimeInSeconds).Execute(trainingDataView, progressHandler = progressHandler)

// Print top models found by AutoML
printfn ""
printfn "Top models ranked by accuracy --"
experimentResult.RunDetails
|> Seq.filter (fun r -> not (isNull r.ValidationMetrics) && not (Double.IsNaN r.ValidationMetrics.Accuracy))
|> Seq.sortBy (fun x -> x.ValidationMetrics.Accuracy)
|> Seq.truncate 3
|> Seq.iteri (fun i x -> ConsoleHelper.printBinaryIterationMetrics (i + 1) x.TrainerName x.ValidationMetrics x.RuntimeInSeconds) 

// STEP 5: Evaluate the model and print metrics
ConsoleHelper.consoleWriteHeader "=============== Evaluating model's accuracy with test data ==============="
let bestRun = experimentResult.BestRun
let trainedModel = bestRun.Model
let predictions = trainedModel.Transform(testDataView)
let metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(data = predictions, scoreColumnName = "Score")
ConsoleHelper.printBinaryClassificationMetrics bestRun.TrainerName metrics

// STEP 6: Save/persist the trained model to a .ZIP file
mlContext.Model.Save(trainedModel, trainingDataView.Schema, modelPath)

printfn "The model is saved to %s" modelPath

ConsoleHelper.consoleWriteHeader "=============== Testing prediction engine ==============="
let sampleStatement = { Text = "This is a very rude movie" ; Label = false}

let loadedTrainedModel, _ = mlContext.Model.Load(modelPath)
printfn "=============== Loaded Model OK  ==============="

// Create prediction engine related to the loaded trained model
let predEngine= mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(loadedTrainedModel)
printfn "=============== Created Prediction Engine OK  ==============="
// Score
let predictedResult = predEngine.Predict sampleStatement

printfn "=============== Single Prediction  ==============="
printfn "Text: %s | Prediction: %s sentiment" sampleStatement.Text (if predictedResult.Prediction then "Toxic" else "NonToxic")
printfn "=================================================="

ConsoleHelper.consoleWriteHeader "=============== End of process, hit any key to finish ==============="
Console.ReadKey() |> ignore

