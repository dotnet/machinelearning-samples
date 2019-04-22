open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data


[<CLIMutable>]
type HeartData = 
    {
        Age : float32 
        Sex : float32 
        Cp : float32 
        TrestBps : float32 
        Chol : float32 
        Fbs : float32 
        RestEcg : float32 
        Thalac : float32 
        Exang : float32 
        OldPeak : float32 
        Slope : float32 
        Ca : float32 
        Thal : float32 
    }

[<CLIMutable>]
type HeartDataImport = 
    {
        [<LoadColumn(0)>] Age : float32 
        [<LoadColumn(1)>] Sex : float32 
        [<LoadColumn(2)>] Cp : float32 
        [<LoadColumn(3)>] TrestBps : float32 
        [<LoadColumn(4)>] Chol : float32 
        [<LoadColumn(5)>] Fbs : float32 
        [<LoadColumn(6)>] RestEcg : float32 
        [<LoadColumn(7)>] Thalac : float32 
        [<LoadColumn(8)>] Exang : float32 
        [<LoadColumn(9)>] OldPeak : float32 
        [<LoadColumn(10)>] Slope : float32 
        [<LoadColumn(11)>] Ca : float32 
        [<LoadColumn(12)>] Thal : float32 
        [<LoadColumn(13)>] Label : bool 
    }

[<CLIMutable>]
type HeartPrediction = { PredictedLabel : bool; Probability : float32; Score : float32 }

let heartSampleData = 
    [
        {Age=36.f; Sex=1.f; Cp=4.f; TrestBps=145.0f; Chol=210.0f; Fbs=0.f; RestEcg=2.f; Thalac=148.f; Exang=1.f; OldPeak=1.9f; Slope=2.f; Ca=1.f; Thal=7.f} 
        {Age=95.f; Sex=1.f; Cp=4.f; TrestBps=145.0f; Chol=210.0f; Fbs=0.f; RestEcg=2.f; Thalac=148.f; Exang=1.f; OldPeak=1.9f; Slope=2.f; Ca=1.f; Thal=7.f} 
        {Age=46.f; Sex=1.f; Cp=4.f; TrestBps=135.f; Chol=192.f; Fbs=0.f; RestEcg=0.f; Thalac=148.f; Exang=0.f; OldPeak=0.3f; Slope=2.f; Ca=0.f; Thal=6.f} 
        {Age=45.f; Sex=0.f; Cp=1.f; TrestBps=140.f; Chol=221.f; Fbs=1.f; RestEcg=1.f; Thalac=150.f; Exang=0.f; OldPeak=2.3f; Slope=3.f; Ca=0.f; Thal=6.f} 
        {Age=88.f; Sex=0.f; Cp=1.f; TrestBps=140.f; Chol=221.f; Fbs=1.f; RestEcg=1.f; Thalac=150.f; Exang=0.f; OldPeak=2.3f; Slope=3.f; Ca=0.f; Thal=6.f}        
    ]

let assemblyFolderPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

let baseDatasetsRelativePath = @"../../../Data"

let trainDataRelativePath = Path.Combine(baseDatasetsRelativePath, "HeartTraining.csv")
let testDataRelativePath = Path.Combine(baseDatasetsRelativePath, "HeartTest.csv")

let trainDataPath = Path.Combine(assemblyFolderPath, trainDataRelativePath)
let testDataPath = Path.Combine(assemblyFolderPath, testDataRelativePath)

let baseModelsRelativePath = @"../../../MLModels"
let modelRelativePath = Path.Combine(baseModelsRelativePath, "HeartClassification.zip")

let modelPath = Path.Combine(assemblyFolderPath, modelRelativePath)
            
let mlContext = new MLContext()

let trainingDataView = mlContext.Data.LoadFromTextFile<HeartDataImport>(trainDataPath, hasHeader = true, separatorChar = ';')
let testDataView = mlContext.Data.LoadFromTextFile<HeartDataImport>(testDataPath, hasHeader = true, separatorChar = ';')
let pipeline = 
    EstimatorChain()
        .Append(mlContext.Transforms.Concatenate("Features", "Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca", "Thal"))
        .Append(mlContext.BinaryClassification.Trainers.FastTree("Label", "Features"))
            
printfn "=============== Training the model ==============="
let trainedModel = pipeline.Fit(trainingDataView)
            
printfn ""
printfn ""
printfn "=============== Finish the train model ==============="
printfn ""
printfn ""
printfn "===== Evaluating Model's accuracy with Test data ====="
let predictions = trainedModel.Transform(testDataView)
let metrics = mlContext.BinaryClassification.Evaluate(data = predictions, labelColumnName = "Label", scoreColumnName = "Score")
printfn ""
printfn ""
printfn "************************************************************"
printfn "*       Metrics for %s binary classification model      " (trainedModel.ToString())
printfn "*-----------------------------------------------------------"
printfn "*       Accuracy: %.2f%%" (metrics.Accuracy * 100.0)
printfn "*       Area Under Roc Curve:      %.2f%%" (metrics.AreaUnderRocCurve * 100.0)
printfn "*       Area Under PrecisionRecall Curve:  %.2f%%" (metrics.AreaUnderPrecisionRecallCurve * 100.0)
printfn "*       F1Score:  %.2f%%" (metrics.F1Score * 100.0)
printfn "*       LogLoss:  %.2f" metrics.LogLoss
printfn "*       LogLossReduction:  %.2f" metrics.LogLossReduction
printfn "*       PositivePrecision:  %.2f" metrics.PositivePrecision
printfn "*       PositiveRecall:  %.2f" metrics.PositiveRecall
printfn "*       NegativePrecision:  %.2f" metrics.NegativePrecision
printfn "*       NegativeRecall:  %.2f%%" (metrics.NegativeRecall * 100.0)
printfn "************************************************************"
printfn ""
printfn ""
printfn "=============== Saving the model to a file ==============="
mlContext.Model.Save(trainedModel, trainingDataView.Schema, modelPath)
printfn ""
printfn ""
printfn "=============== Model Saved ============= "

// test
let loadedModel, _ = mlContext.Model.Load(modelPath)
let predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(loadedModel)          

do 
    let recordNames = FSharp.Reflection.FSharpType.GetRecordFields(typeof<HeartData>) |> Array.map (fun x -> x.Name)
    let reader = FSharp.Reflection.FSharpValue.PreComputeRecordReader(typeof<HeartData>)
    heartSampleData
    |> Seq.iter 
        (fun x ->
            let prediction = predictionEngine.Predict x
            printfn "=============== Single Prediction  ==============="
            (recordNames, reader x) ||> Array.iter2 (printfn "%s: %O")
            printfn "Prediction Value: %b " prediction.PredictedLabel
            printfn "Prediction: %s " (if prediction.PredictedLabel then "A disease could be present" else "Not present disease" )
            printfn "Probability: %0.8f" prediction.Probability
            printfn "=================================================="
            printfn ""
            printfn "")


printfn "=============== End of process, hit any key to finish ==============="
Console.ReadLine() |> ignore
