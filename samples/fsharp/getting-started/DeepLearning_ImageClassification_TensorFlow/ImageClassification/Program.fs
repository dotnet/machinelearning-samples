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
[<Literal>]
let OutputTensorName = "softmax2"

[<CLIMutable>]
type ImageNetData =
    {
        [<LoadColumn(0)>]
        ImagePath : string
        [<LoadColumn(1)>]
        Label : string
    }

[<CLIMutable>]
type ImageNetDataProbability =
    {
        ImagePath : string
        Label : string
        PredictedLabel : string
        Probability : float32
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
[<CLIMutable>]
type ImageNetPrediction =
    {
        [<ColumnName(OutputTensorName)>]
        PredictedLabels : float32 []
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

let printImageNetProb (x : ImageNetDataProbability) =

    let defaultForeground = Console.ForegroundColor
    let labelColor = ConsoleColor.Magenta
    let probColor = ConsoleColor.Blue
    let exactLabel = ConsoleColor.Green
    let failLabel = ConsoleColor.Red

    printf "ImagePath: "
    Console.ForegroundColor <- labelColor
    printf "%s" (Path.GetFileName(x.ImagePath))
    Console.ForegroundColor <- defaultForeground
    printf " labeled as "
    Console.ForegroundColor <- labelColor
    printf "%s" x.Label
    Console.ForegroundColor <- defaultForeground
    printf " predicted as "
    if x.Label = x.PredictedLabel then
        Console.ForegroundColor <- exactLabel
        printf "%s" x.PredictedLabel
    else
        Console.ForegroundColor <- failLabel
        printf "%s" x.PredictedLabel
    Console.ForegroundColor <- defaultForeground
    printf " with probability "
    Console.ForegroundColor <- probColor
    printf "%0.7f" x.Probability
    Console.ForegroundColor <- defaultForeground
    printfn ""

let score dataLocation imagesFolder inputModelLocation labelsTxt =
    printHeader ["Read model"]
    printfn "Model location: %s" inputModelLocation
    printfn "Images folder: %s" imagesFolder
    printfn "Training file: %s" dataLocation
    printfn "Default parameters: image size=(%d,%d), image mean: %d" imageHeight imageWidth mean
    let mlContext = MLContext(seed = Nullable 1)
    let data = mlContext.Data.LoadFromTextFile<ImageNetData>(dataLocation, hasHeader = false)
    let pipeline =
        EstimatorChain()
            .Append(mlContext.Transforms.LoadImages(outputColumnName = "input", imageFolder = imagesFolder, inputColumnName = "ImagePath"))
            .Append(mlContext.Transforms.ResizeImages("input", imageWidth, imageHeight, inputColumnName = "input"))
            .Append(mlContext.Transforms.ExtractPixels(outputColumnName = "input", interleavePixelColors = channelsLast, offsetImage = float32 mean))
            .Append(mlContext.Model.LoadTensorFlowModel(inputModelLocation).ScoreTensorFlowModel([| "softmax2" |], [| "input" |], addBatchDimensionInput = true))
    let model = pipeline.Fit(data)
    let predictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(model)

    printHeader ["Classificate images"]
    printfn "Images folder: %s" imagesFolder
    printfn "Training file: %s" dataLocation
    printfn "Labels file: %s" labelsTxt

    let labels = File.ReadAllLines(labelsTxt)

    File.ReadAllLines(dataLocation)
    |> Seq.map (fun x -> let fields = x.Split '\t' in {ImagePath = Path.Combine(imagesFolder, fields.[0]); Label = fields.[1]})
    |> Seq.map
        (fun sample ->
            let preds = predictionEngine.Predict(sample).PredictedLabels
            let bestLabelIndex =
                preds
                |> Seq.mapi (fun i x -> i, x)
                |> Seq.maxBy snd
                |> fst
            {
                PredictedLabel = labels.[bestLabelIndex]
                Probability = preds.[bestLabelIndex]
                ImagePath = sample.ImagePath
                Label = sample.Label
            }
        )
    |> Seq.iter printImageNetProb

[<EntryPoint>]
let main _argv =
    let assetsPath = Path.Combine(dataRoot.Directory.FullName, @"..\..\..\assets")
    let tagsTsv = Path.Combine(assetsPath, "inputs", "images", "tags.tsv")
    let imagesFolder = Path.Combine(assetsPath, "inputs", "images")
    let inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb")
    let labelsTxt = Path.Combine(assetsPath, "inputs", "inception", "imagenet_comp_graph_label_strings.txt")

    try
        score tagsTsv imagesFolder inceptionPb labelsTxt
    with
    | e -> printExn [e.ToString()]

    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Green
    printfn " "
    printfn "Press any key to finish."
    Console.ForegroundColor <- defaultColor
    Console.ReadKey() |> ignore
    0
