open System
open System.Collections.Generic
open System.IO
open System.Linq
open Microsoft.ML
open Microsoft.ML.Data

[<CLIMutable>]
type MeterData =
    {
        [<LoadColumn(0)>]
        name : string
        [<LoadColumn(1)>]
        time : DateTime
        [<LoadColumn(2)>]
        ConsumptionDiffNormalized : float32
    }

[<CLIMutable>]
type SpikePrediction = {Prediction : double []}

let assemblyFolderPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

let datasetsRelativePath = @"../../../Data"
let trainingDatarelativePath = Path.Combine(datasetsRelativePath, "power-export_min.csv")

let trainingDataPath = Path.Combine(datasetsRelativePath, trainingDatarelativePath)

let baseModelsRelativePath = @"../../../MLModels"
let modelRelativePath = Path.Combine(baseModelsRelativePath, "PowerAnomalyDetectionModel.zip")

let modelPath = Path.Combine(assemblyFolderPath, modelRelativePath)

let mlContext = MLContext(seed = Nullable 0)

// load data
let dataView = mlContext.Data.LoadFromTextFile<MeterData>(trainingDatarelativePath, separatorChar = ',', hasHeader = true)

let trainigPipeLine = mlContext.Transforms.DetectSpikeBySsa("Prediction", "ConsumptionDiffNormalized", confidence = 98, pvalueHistoryLength = 30, trainingWindowSize = 90, seasonalityWindowSize = 30)

let trainedModel = trainigPipeLine.Fit(dataView)

mlContext.Model.Save(trainedModel, dataView.Schema, modelPath)

printfn "The model is saved to %s" modelPath
printfn ""


let transformedData = 
    let trainedModel, _ = mlContext.Model.Load modelPath
    trainedModel.Transform(dataView)

// Getting the data of the newly created column as an IEnumerable
let predictions = mlContext.Data.CreateEnumerable<SpikePrediction>(transformedData, false)

let colCDN = dataView.GetColumn<float32>("ConsumptionDiffNormalized").ToArray();
let colTime = dataView.GetColumn<DateTime>("time").ToArray();

// Output the input data and predictions
printfn "======Displaying anomalies in the Power meter data========="
printfn "Date              \tReadingDiff\tAlert\tScore\tP-Value"

predictions
|> Seq.iteri
    (fun i p ->
        if p.Prediction.[0] = 1.0 then
            Console.BackgroundColor <- ConsoleColor.DarkYellow
            Console.ForegroundColor <- ConsoleColor.Black
        Console.WriteLine("{0}\t{1:0.0000}\t{2:0.00}\t{3:0.00}\t{4:0.00}", colTime.[i], colCDN.[i], p.Prediction.[0], p.Prediction.[1], p.Prediction.[2])
        Console.ResetColor();
    )

printfn ""
printfn "Press any key to exit"
Console.Read() |> ignore
