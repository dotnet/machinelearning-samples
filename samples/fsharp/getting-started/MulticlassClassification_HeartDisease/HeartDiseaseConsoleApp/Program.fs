// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Core.Data

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

let heartSampleData = 
    [
        {Age=36.f; Sex=1.f; Cp=4.f; TrestBps=135.f; Chol=321.f; Fbs=1.f; RestEcg=0.f; Thalac=158.f; Exang=0.f; OldPeak=1.3f; Slope=0.f; Ca=0.f; Thal=3.f} 
        {Age=95.f; Sex=1.f; Cp=4.f; TrestBps=135.f; Chol=321.f; Fbs=1.f; RestEcg=0.f; Thalac=158.f; Exang=0.f; OldPeak=1.3f; Slope=0.f; Ca=0.f; Thal=3.f} 
        {Age=45.f; Sex=0.f; Cp=1.f; TrestBps=140.f; Chol=221.f; Fbs=1.f; RestEcg=1.f; Thalac=150.f; Exang=0.f; OldPeak=2.3f; Slope=3.f; Ca=0.f; Thal=6.f} 
        {Age=45.f; Sex=0.f; Cp=1.f; TrestBps=140.f; Chol=221.f; Fbs=1.f; RestEcg=1.f; Thalac=150.f; Exang=0.f; OldPeak=2.3f; Slope=3.f; Ca=0.f; Thal=6.f} 
        {Age=88.f; Sex=0.f; Cp=1.f; TrestBps=140.f; Chol=221.f; Fbs=1.f; RestEcg=1.f; Thalac=150.f; Exang=0.f; OldPeak=2.3f; Slope=3.f; Ca=0.f; Thal=6.f}        
    ]

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
        [<LoadColumn(13)>] Label : float32 
    }

[<CLIMutable>]
type HeartPrediction = { Score : float32 [] }

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data"
let trainDataPath = sprintf @"%s/HeartTraining.csv" baseDatasetsLocation
let testDataPath = sprintf @"%s/HeartTest.csv" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels";
let modelPath = sprintf @"%s/HeartClassification.zip" baseModelsPath



let read (dataPath : string) (dataLoader : TextLoader) =
    dataLoader.Read dataPath

let downcastPipeline (x : IEstimator<_>) = 
    match x with 
    | :? IEstimator<ITransformer> as y -> y
    | _ -> failwith "downcastPipeline: expecting a IEstimator<ITransformer>"


let buildTrainEvaluateAndSaveModel (mlContext : MLContext) =
    // STEP 1: Common data loading configuration
    let trainingDataView = mlContext.Data.ReadFromTextFile<HeartDataImport>(trainDataPath, hasHeader = true, separatorChar = ',')
    let testDataView = mlContext.Data.ReadFromTextFile<HeartDataImport>(testDataPath, hasHeader = true, separatorChar = ',')

    // STEP 2: Common data process configuration with pipeline data transformations          
    let dataProcessPipeline = 
        EstimatorChain()
            .Append(mlContext.Transforms.Concatenate
                (DefaultColumnNames.Features, "Age", "Sex",
                    "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac",
                    "Exang", "OldPeak", "Slope", "Ca", "Thal"))
            .AppendCacheCheckpoint(mlContext)
        |> downcastPipeline            

    // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<HeartData> mlContext trainingDataView dataProcessPipeline 5 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext DefaultColumnNames.Features trainingDataView dataProcessPipeline 5 |> ignore

    let trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn = DefaultColumnNames.Label, featureColumn = DefaultColumnNames.Features)
    let trainingPipeline = dataProcessPipeline.Append(trainer)

    printfn "=============== Training the model ==============="
    let trainedModel = trainingPipeline.Fit(trainingDataView)
    printfn "=============== Finish the train model.==============="

    printfn "===== Evaluating Model's accuracy with Test data ====="
    let predictions = trainedModel.Transform testDataView
    let metrics = 
        mlContext.MulticlassClassification.Evaluate   
            (data = predictions, 
             label = DefaultColumnNames.Label, 
             score = DefaultColumnNames.Score, 
             predictedLabel = DefaultColumnNames.PredictedLabel, 
             topK = 0)

    Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metrics

    printfn "=============== Saving the model to a file ==============="
    use fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
    mlContext.Model.Save(trainedModel, fs)

    printfn "=============== Model Saved ============= "


let testPrediction (mlContext : MLContext) =
    let trainedModel = 
        use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        mlContext.Model.Load(stream)
    let predEngine = trainedModel.CreatePredictionEngine<HeartData, HeartPrediction>(mlContext)

    heartSampleData
    |> List.iter 
        (fun x ->
            predEngine.Predict(x).Score
            |> Seq.iteri (fun i s -> printfn " %d: %0.3f" i s)
            printfn ""
        )

[<EntryPoint>]
let main argv =
    let mlContext = MLContext()
    buildTrainEvaluateAndSaveModel mlContext

    testPrediction mlContext
    printfn "=============== End of process, hit any key to finish ==============="
    Console.ReadKey() |> ignore

    0 // return an integer exit code
