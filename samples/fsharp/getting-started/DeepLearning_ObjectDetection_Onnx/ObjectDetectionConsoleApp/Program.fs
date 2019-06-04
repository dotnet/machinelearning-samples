open System
open System.IO
open Microsoft.ML.Data
open Microsoft.ML
open System.Drawing


let imageHeight = 416
let imageWidth = 416

[<Literal>]
let InputTensorName = "image"
[<Literal>]
let OutputTensorName = "grid"


type YoloBoundingBox =
    {
        Label : string
        X : float32
        Y : float32
        Height : float32
        Width : float32
        Confidence : float32
    }
    member x.Rect = RectangleF(x.X, x.Y, x.Width, x.Height)

[<CLIMutable>]
type ImageNetData =
    {
        [<LoadColumn(0)>]
        ImagePath : string
        [<LoadColumn(1)>]
        Label : string
    }

[<CLIMutable>]
type ImageNetPrediction =
    {
        [<ColumnName(OutputTensorName)>]
        PredictedLabels : float32[]
    }
let assemblyFolderPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

let assetsRelativePath = @"../../../assets"
let assetsPath = Path.Combine(assemblyFolderPath, assetsRelativePath)
let modelFilePath = Path.Combine(assetsPath, "Model", "TinyYolo2_model.onnx")
let imagesFolder = Path.Combine(assetsPath,"images")
let tagsTsv = Path.Combine(assetsPath,"images", "tags.tsv")


[<Literal>]
let ROW_COUNT = 13
[<Literal>]
let COL_COUNT = 13
[<Literal>]
let CHANNEL_COUNT = 125
[<Literal>]
let BOXES_PER_CELL = 5
[<Literal>]
let BOX_INFO_FEATURE_COUNT = 5
[<Literal>]
let CLASS_COUNT = 20
[<Literal>]
let CELL_WIDTH = 32.f
[<Literal>]
let CELL_HEIGHT = 32.f
let channelStride = ROW_COUNT * COL_COUNT
let anchors = [|1.08f; 1.19f; 3.42f; 4.41f; 6.63f; 11.38f; 9.42f; 5.11f; 16.62f; 10.52f|]
let labels =
    [|"aeroplane"; "bicycle"; "bird"; "boat"; "bottle"; "bus"; "car";
      "cat"; "chair"; "cow"; "diningtable"; "dog"; "horse"; "motorbike";
      "person"; "pottedplant"; "sheep"; "sofa"; "train"; "tvmonitor" |]

let sigmoid x = let k = exp x in k / (1.f + k)
let softmax x =
    let mx = x |> Array.max
    let expX = x |> Array.map (fun i -> exp (i - mx))
    let sm = expX |> Array.sum
    expX |> Array.map (fun i -> i / sm)

let intersectionOverUnion (a : RectangleF) (b : RectangleF) =
    let areaA = a.Width * a.Height
    let areaB = b.Width * b.Height
    if areaA <= 0.f || areaB <= 0.f then
        0.f
    else
        let minX = max a.Left b.Left
        let minY = max a.Top b.Top
        let maxX = min a.Right b.Right
        let maxY = min a.Bottom b.Bottom
        let intersectionArea = (max (maxY - minY) 0.f) * max (maxX - minX) 0.f
        intersectionArea / (areaA + areaB - intersectionArea)

let parseOutputs threshold (outputs : float32 []) =
    let boxes = ResizeArray()
    for cy = 0 to ROW_COUNT - 1 do
        for cx = 0 to COL_COUNT - 1 do
            for b = 0 to BOXES_PER_CELL - 1 do
                let get channel = outputs.[channel*channelStride + cy*COL_COUNT + cx]
                let channel = b * (CLASS_COUNT + BOX_INFO_FEATURE_COUNT)
                let tx = get channel
                let ty = get (channel + 1)
                let tw = get (channel + 2)
                let th = get (channel + 3)
                let tc = get (channel + 4)
                let x = (float32 cx + sigmoid tx) * CELL_WIDTH
                let y = (float32 cy + sigmoid ty) * CELL_HEIGHT
                let width = exp tw * CELL_WIDTH * anchors.[b*2]
                let height = exp th * CELL_HEIGHT * anchors.[b*2 + 1]
                let confidence = sigmoid tc
                if confidence >= threshold then
                    let classes =
                        let classOffset = channel + BOX_INFO_FEATURE_COUNT;
                        Array.init CLASS_COUNT (fun i -> get (i + classOffset))
                    let results = softmax classes |> Array.mapi (fun i x -> (i,x))
                    let topClass = results |> Array.maxBy snd |> fst
                    let topScore = results |> Array.maxBy snd |> (fun (_, x) -> x * confidence)
                    if topScore >= threshold then
                        boxes.Add {
                                Confidence = topScore
                                X = (x - width / 2.f)
                                Y = (y - height / 2.f)
                                Width = width
                                Height = height
                                Label = labels.[topClass] } |> ignore
    boxes.ToArray()

let nonMaxSuppress limit threshold boxes =
    let rec loop count acc l =
        match l with
        | [] -> acc
        | (a : YoloBoundingBox) :: t ->
            let acc = a :: acc
            let count = count + 1
            if count >= limit then
                acc
            else
                t
                |> List.filter (fun b ->intersectionOverUnion a.Rect b.Rect <= threshold)
                |> loop count acc
    boxes
    |> Array.sortByDescending (fun x -> x.Confidence)
    |> Array.toList
    |> loop 0 []


try
    let mlContext = MLContext()
    let model =
        printfn "Read model"
        printfn "Model location: %s" modelFilePath
        printfn "Images folder: %s" imagesFolder
        printfn "Default parameters: image size=(%d,%d)" imageWidth imageHeight
        let data = mlContext.Data.LoadFromTextFile<ImageNetData>(imagesFolder, hasHeader=true)
        let pipeline =
            EstimatorChain()
                .Append(mlContext.Transforms.LoadImages("image", imageFolder = imagesFolder, inputColumnName = "ImagePath"))
                .Append(mlContext.Transforms.ResizeImages("image", imageWidth = imageWidth, imageHeight = imageHeight, inputColumnName = "image"))
                .Append(mlContext.Transforms.ExtractPixels("image"))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile = modelFilePath, outputColumnNames = [|OutputTensorName|], inputColumnNames = [|InputTensorName|]))
        let model = pipeline.Fit(data)
        mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(model)

    printfn "Tags file location: %s" tagsTsv
    printfn ""
    printfn "=====Identify the objects in the images====="
    printfn ""

    File.ReadAllLines(tagsTsv)
    |> Seq.map
        (fun x ->
            let a = x.Split '\t'
            {ImagePath = Path.Combine(imagesFolder, a.[0]); Label = a.[1]}
        )
    |> Seq.iter
        (fun sample ->
            let probs = model.Predict(sample).PredictedLabels
            let boundingBoxes = parseOutputs 0.3f probs
            let filteredBoxes = nonMaxSuppress 5 0.5f boundingBoxes

            printfn ".....The objects in the image %s are detected as below...."  sample.Label
            filteredBoxes
            |> Seq.iter
                (fun fbox ->
                    printfn "%s and its Confidence score: %0.7f" fbox.Label fbox.Confidence
                )
            printfn ""
        )
 with
 | e -> printfn "%s" (e.ToString())

printfn "========= End of Process. Hit any Key ========"
Console.ReadLine() |> ignore