open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data

let dataRoot = FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)

let imageHeight = 224
let imageWidth = 224
let mean = 117
let scale = 1
let channelsLast = true

[<CLIMutable>]
type ImageNetData =
    {
        [<LoadColumn(0)>]
        ImagePath : string
        [<LoadColumn(1)>]
        Label : string
    }
[<CLIMutable>]
type ImageNetPipeline =
    {
        ImagePath : string
        Label : string
        PredictedLabelValue : string
        Score : float32 []
        softmax2_pre_activation : float32 []
    }

let printImagePrediction (x : ImageNetPipeline) =
    let defaultForeground = Console.ForegroundColor
    let labelColor = ConsoleColor.Magenta
    let probColor = ConsoleColor.Blue
    printf "ImagePath: "
    Console.ForegroundColor <- labelColor
    printf "%s" (Path.GetFileName(x.ImagePath))
    Console.ForegroundColor <- defaultForeground
    printf " predicted as "
    Console.ForegroundColor <- labelColor
    printf "%s" x.PredictedLabelValue
    Console.ForegroundColor <- defaultForeground
    Console.Write(" with score ")
    Console.ForegroundColor <- probColor
    printf "%f" (x.Score |> Seq.max)
    Console.ForegroundColor <- defaultForeground;
    printfn ""

let printHeader lines =
    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Yellow
    printfn " "
    lines |> Seq.iter (printfn "%s")
    let maxLength = lines |> Seq.map (fun x -> x.Length) |> Seq.max
    printfn "%s" (String('#', maxLength))
    Console.ForegroundColor <- defaultColor

let printExn lines =
    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Red
    printfn " "
    printfn "EXCEPTION"
    printfn "#########"
    Console.ForegroundColor <- defaultColor
    lines |> Seq.iter (printfn "%s")


let buildAndTrainModel dataLocation imagesFolder inputModelLocation imageClassifierZip =
    printfn "Read model"
    printfn "Model location: %s" inputModelLocation
    printfn "Images folder: %s" imagesFolder
    printfn "Training file: %s" dataLocation
    printfn "Default parameters: image size =(%d,%d), image mean: %d" imageHeight imageWidth mean
    let mlContext = MLContext(seed = Nullable 1)
    let data = mlContext.Data.LoadFromTextFile<ImageNetData>(dataLocation, hasHeader = false)
    let pipeline =
        EstimatorChain()
            .Append(mlContext.Transforms.Conversion.MapValueToKey("LabelTokey", "Label"))
            .Append(mlContext.Transforms.LoadImages("ImageReal", imagesFolder, "ImagePath"))
            .Append(mlContext.Transforms.ResizeImages("ImageReal", imageWidth, imageHeight, inputColumnName = "ImageReal"))
            .Append(mlContext.Transforms.ExtractPixels("input", "ImageReal", interleavePixelColors = channelsLast, offsetImage = float32 mean))
            .Append(mlContext.Model.LoadTensorFlowModel(inputModelLocation).
                                 ScoreTensorFlowModel(outputColumnNames = [|"softmax2_pre_activation"|], inputColumnNames = [|"input"|], addBatchDimensionInput = true))
            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("LabelTokey", "softmax2_pre_activation"))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue","PredictedLabel"))
            .AppendCacheCheckpoint(mlContext)

    printHeader ["Training classification model"]
    let model = pipeline.Fit(data)
    let trainData = model.Transform(data)
    mlContext.Data.CreateEnumerable<_>(trainData, false, true)
    |> Seq.iter printImagePrediction

    printHeader ["Classification metrics"]
    let metrics = mlContext.MulticlassClassification.Evaluate(trainData, labelColumnName = "LabelTokey", predictedLabelColumnName = "PredictedLabel")
    printfn "LogLoss is: %.15f" metrics.LogLoss
    metrics.PerClassLogLoss
    |> Seq.map string
    |> String.concat " , "
    |> printfn "PerClassLogLoss is: %s"

    printHeader ["Save model to local file"]
    let outFile = Path.Combine(dataRoot.Directory.FullName, imageClassifierZip)
    if File.Exists outFile then
        File.Delete(outFile)
    do
        use f = File.OpenWrite(outFile)
        mlContext.Model.Save(model, trainData.Schema, f)
    printfn "Model saved: %s" outFile

[<EntryPoint>]
let main _argv =
    let assetsPath = Path.Combine(dataRoot.Directory.FullName, @"..\..\..\assets")
    let tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv")
    let imagesFolder = Path.Combine(assetsPath, "inputs", "data")
    let inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb")
    let imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip")
    try
        buildAndTrainModel tagsTsv imagesFolder inceptionPb imageClassifierZip
    with
    | e -> printExn [e.ToString()]

    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Green
    printfn " "
    printfn "Press any key to finish."
    Console.ForegroundColor <- defaultColor
    Console.ReadKey() |> ignore
    0
