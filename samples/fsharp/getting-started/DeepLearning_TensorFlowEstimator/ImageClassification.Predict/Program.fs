open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open Common

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
type ImageNetPrediction =
    {
        ImagePath : string
        Label : string
        PredictedLabelValue : string
        Score : float32 []
    }

let printImagePrediction (x : ImageNetPrediction) =
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
    printf "%f" x.Probability
    Console.ForegroundColor <- defaultForeground
    printfn ""

let classifyImages dataLocation imagesFolder modelLocation =
    printHeader ["Loading model"]

    let mlContext = MLContext(seed = Nullable 1)
    let loadedModel, inputSchema =
        use f = File.OpenRead(modelLocation)
        mlContext.Model.Load(f)
    printfn "Model loaded: %s" modelLocation
    let predictor = mlContext.Model.CreatePredictionEngine<ImageNetData,ImageNetPrediction>(loadedModel)

    printHeader ["Making classifications"]
    File.ReadAllLines(dataLocation)
    |> Seq.map (fun x -> let fields = x.Split '\t' in {ImagePath = Path.Combine(imagesFolder, fields.[0]); Label = fields.[1]})
    |> Seq.iter (predictor.Predict >> printImagePrediction)


[<EntryPoint>]
let main _argv =
    let assetsPath = Path.Combine(dataRoot.Directory.FullName, @"..\..\..\assets")
    let tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv")
    let imagesFolder = Path.Combine(assetsPath, "inputs", "data")
    //let imageClassifierZip = Path.Combine(assetsPath, "inputs", "imageClassifier.zip")
    // Use directly the last saved:
    let imageClassifierZip = Path.Combine(dataRoot.Directory.FullName, @"..\..\..\..\ImageClassification.Train\assets\outputs\imageClassifier.zip")
    //let imageClassifierZip = Path.GetFullPath(imageClassifierZip)
    if not (File.Exists(imageClassifierZip)) then
        Console.WriteLine("Please run the model training first.")
        Environment.Exit(0)

    let commonDatasetsRelativePath = @"../../../../../../../../datasets"
    let imagesDatasetName = "flower_photos_prediction_set"
    let imagesDatasetZip = imagesDatasetName + ".zip"
    let imagesDatasetUrl = "https://bit.ly/3gGDev2"
    let destFolder = imagesFolder
    let imagePath1 = Path.GetFullPath(Path.Combine(imagesFolder, "broccoli.png"))
    let imagePath2 = Path.GetFullPath(Path.Combine(imagesFolder, "pizza3.jpg"))
    let imagePath3 = Path.GetFullPath(Path.Combine(imagesFolder, "teddy6.jpg"))
    let imagePath4 = Path.GetFullPath(Path.Combine(imagesFolder, "toaster3.jpg"))
    let destFiles: string list = [imagePath1;imagePath2;imagePath3;imagePath4]
    let datasetPath = 
        __SOURCE_DIRECTORY__ 
        |> Web.DownloadBigFile imagesFolder imagesDatasetUrl imagesDatasetZip commonDatasetsRelativePath destFiles destFolder

    try
        classifyImages tagsTsv imagesFolder imageClassifierZip
    with
    | e -> printExn [e.ToString()]

    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Green
    printfn " "
    printfn "Press any key to finish."
    Console.ForegroundColor <- defaultColor
    Console.ReadKey() |> ignore
    0
