// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.IO.Compression
open System.Net
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms
open Microsoft.ML.Vision

// Define input and output schema
[<CLIMutable>]
type ImageData = {
    ImagePath:string
    Label:string
}

[<CLIMutable>]
type ImagePrediction = {
    ImagePath:string
    Label: string
    PredictedLabel:string
}

// Helper functions
let downloadZippedImageSetAsync (fileName:string) (downloadUrl:string) (imageDownloadFolder:string) = 
    async {
        let zippedPath = Path.Join(imageDownloadFolder, fileName)
        let unzippedPath = Path.Join(imageDownloadFolder,Path.GetFileNameWithoutExtension(zippedPath));

        if not (File.Exists zippedPath) then
            let client = new WebClient()
            client.DownloadFile(Uri(downloadUrl), zippedPath)

        if not (Directory.Exists unzippedPath) then
            ZipFile.ExtractToDirectory(zippedPath, unzippedPath)

        return Path.Join(unzippedPath,Path.GetFileNameWithoutExtension(fileName))
    }

let loadImagesFromDirectory (path:string) (useDirectoryAsLabel:bool) = 

    let files = Directory.GetFiles(path, "*",searchOption=SearchOption.AllDirectories)

    files
    |> Array.filter(fun file -> 
        (Path.GetExtension(file) = ".jpg") ||
        (Path.GetExtension(file) = ".png"))
    |> Array.map(fun file -> 
        let mutable label = Path.GetFileName(file)
        if useDirectoryAsLabel then
            label <-  Directory.GetParent(file).Name
        else
            let mutable brk = false
            for index in 0..label.Length do
                while not brk do
                    if not (label.[index] |> Char.IsLetter) then
                        label <- label.Substring(0,index)
                        brk <- true

        {ImagePath=file; Label=label}
    )


[<EntryPoint>]
let main argv =
    
    let fileName = "flower_photos_small_set.zip"
    let downloadUrl = "https://mlnetfilestorage.file.core.windows.net/imagesets/flower_images/flower_photos_small_set.zip?st=2019-08-07T21%3A27%3A44Z&se=2030-08-08T21%3A27%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=SZ0UBX47pXD0F1rmrOM%2BfcwbPVob8hlgFtIlN89micM%3D"
 
    // Download Data
    let datasetPath = 
        __SOURCE_DIRECTORY__ 
        |> downloadZippedImageSetAsync fileName downloadUrl
        |> Async.RunSynchronously 
    
    // Initialize MLContext
    let mlContext = MLContext()

    // Get List of Images        
    let images = loadImagesFromDirectory datasetPath true

    // Load Data into IDataView and Shuffle
    let data = 
        images 
        |> mlContext.Data.LoadFromEnumerable
        |> mlContext.Data.ShuffleRows

    // Define preprocessing pipeline
    let preprocessingPipeline = 
        EstimatorChain()
            .Append(mlContext.Transforms.Conversion.MapValueToKey("LabelAsKey","Label"))
            .Append(mlContext.Transforms.LoadRawImageBytes("Image", null, "ImagePath"))    

    // Preprocess the data
    let preprocessedData = 
        let processingTransformer = data |> preprocessingPipeline.Fit 
        data |> processingTransformer.Transform

    // Split data into train,validation, test sets
    let train, validation, test = 
        preprocessedData
        |> ( fun originalData -> 
                let trainValSplit = mlContext.Data.TrainTestSplit(originalData, testFraction=0.7)
                let testValSplit = mlContext.Data.TrainTestSplit(trainValSplit.TestSet)
                (trainValSplit.TrainSet, testValSplit.TrainSet, testValSplit.TestSet))

    // Define ImageClassificationTrainer Options
    let classifierOptions = ImageClassificationTrainer.Options()
    classifierOptions.FeatureColumnName <- "Image" 
    classifierOptions.LabelColumnName <- "LabelAsKey"
    classifierOptions.ValidationSet <- validation
    classifierOptions.Arch <- ImageClassificationTrainer.Architecture.ResnetV2101
    classifierOptions.MetricsCallback <- Action<ImageClassificationTrainer.ImageClassificationMetrics>(fun x -> printfn "%s" (x.ToString()))

    // Define model training pipeline
    let trainingPipeline = 
        EstimatorChain()
            .Append(mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel","LabelAsKey"))

    // Train the model
    let trainedModel = 
        train 
        |> trainingPipeline.Fit

    // Get Prediction IDataView
    let predictions = test |> trainedModel.Transform

    // Evaluate the model
    let metrics = mlContext.MulticlassClassification.Evaluate(predictions,labelColumnName="LabelAsKey")
    printfn "MacroAccurracy: %f | LogLoss: %f" metrics.MacroAccuracy metrics.LogLoss

    // Save Model
    mlContext.Model.Save(trainedModel, preprocessedData.Schema, Path.Join(__SOURCE_DIRECTORY__,"model.zip"))

    // Display 5 predictions
    mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, reuseRowObject=true)
    |> Seq.take 5
    |> Seq.iter(fun prediction -> printfn "Original: %s | Predicted: %s" prediction.Label prediction.PredictedLabel) 
    
    0 // return an integer exit code
