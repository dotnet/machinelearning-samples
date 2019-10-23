// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms

[<CLIMutable>]
type ImageData = {
    ImagePath:string
    Label:string
}

[<CLIMutable>]
type ImagePrediction = {
    ImagePath:string
    PredictedLabel:string
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

    // Create MLContext
    let mlContext = MLContext()

    // Create collection of ImageData items   
    let images = loadImagesFromDirectory "../../../assets" true 

    // Load data into an IDataView
    let data = 
        images 
        |> mlContext.Data.LoadFromEnumerable
        |> mlContext.Data.ShuffleRows

    // Define preprocessing pipeline
    let preprocessingPipeline = 
        EstimatorChain()
            .Append(
                [|InputOutputColumnPair("LabelAsKey","Label")|]
                |> mlContext.Transforms.Conversion.MapValueToKey)
            .Append(
                ("Image", "Data", "ImagePath")
                |> mlContext.Transforms.LoadImages)

    // Apply data to preprocssing pipeline                    
    let preprocessedData = 
        let transformer = data |> preprocessingPipeline.Fit
        data |> transformer.Transform

    // Split data into train,validation,test sets
    let train, validation, test = 
        data
        |> ( fun originalData -> 
                let trainValSplit = mlContext.Data.TrainTestSplit(originalData, testFraction=0.7)
                let testValSplit = mlContext.Data.TrainTestSplit(trainValSplit.TestSet)
                (trainValSplit.TrainSet, testValSplit.TrainSet, testValSplit.TestSet))        

    // Define model training pipeline
    let trainingPipeline = 
        mlContext.Model.ImageClassification(
                featuresColumnName="Image",
                labelColumnName="LabelAsKey",
                arch=ImageClassificationEstimator.Architecture.ResnetV2101,
                validationSet=validation,
                testOnTrainSet=false)

    // Train the model
    let trainedModel = 
        train |> trainingPipeline.Fit

    0 // return an integer exit code
