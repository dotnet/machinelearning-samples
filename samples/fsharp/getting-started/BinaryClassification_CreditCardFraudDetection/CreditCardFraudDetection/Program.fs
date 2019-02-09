open System
open System.IO
open System.IO.Compression

open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms.Normalizers

// Data models
[<CLIMutable>]
type TransactionObservation = {
    Label: bool
    V1: float32
    V2: float32
    V3: float32
    V4: float32
    V5: float32
    V6: float32
    V7: float32
    V8: float32
    V9: float32
    V10: float32
    V11: float32
    V12: float32
    V13: float32
    V14: float32
    V15: float32
    V16: float32
    V17: float32
    V18: float32
    V19: float32
    V20: float32
    V21: float32
    V22: float32
    V23: float32
    V24: float32
    V25: float32
    V26: float32
    V27: float32
    V28: float32
    Amount: float32
    }

[<CLIMutable>]
type TransactionFraudPrediction = {
    Label: bool
    PredictedLabel: bool
    Score: float32
    Probability: float32
    }

[<EntryPoint>]
let main _ =

    (*
    File names and location
    *) 

    let appDirectory = 
        Environment.GetCommandLineArgs().[0]
        |> Path.GetDirectoryName

    let dataDirectory = Path.Combine (appDirectory, "../../../../Data/")

    let zippedDatasetFile = Path.Combine (dataDirectory, "creditcardfraud-dataset.zip")

    let inputFile = Path.Combine (dataDirectory, "creditcard.csv")
    let trainFile = Path.Combine (dataDirectory, "trainData.csv")
    let testFile = Path.Combine (dataDirectory, "testData.csv")

    let modelFile = Path.Combine (dataDirectory, "fastTree.zip")

    (*
    Prepare input file from original zipped dataset
    *)

    if not (File.Exists (inputFile))
    then
        printfn "Extracting dataset"
        ZipFile.ExtractToDirectory (zippedDatasetFile, dataDirectory)

    
    let seed = Nullable 1
    let mlContext = MLContext seed

    let columns = 
        [
            // A boolean column depicting the 'label'.
            TextLoader.Column("Label", Nullable DataKind.BL, 30)
            // 29 Features V1..V28 + Amount
            TextLoader.Column("V1", Nullable DataKind.R4, 1)
            TextLoader.Column("V2", Nullable DataKind.R4, 2)
            TextLoader.Column("V3", Nullable DataKind.R4, 3)
            TextLoader.Column("V4", Nullable DataKind.R4, 4)
            TextLoader.Column("V5", Nullable DataKind.R4, 5)
            TextLoader.Column("V6", Nullable DataKind.R4, 6)
            TextLoader.Column("V7", Nullable DataKind.R4, 7)
            TextLoader.Column("V8", Nullable DataKind.R4, 8)
            TextLoader.Column("V9", Nullable DataKind.R4, 9)
            TextLoader.Column("V10", Nullable DataKind.R4, 10)
            TextLoader.Column("V11", Nullable DataKind.R4, 11)
            TextLoader.Column("V12", Nullable DataKind.R4, 12)
            TextLoader.Column("V13", Nullable DataKind.R4, 13)
            TextLoader.Column("V14", Nullable DataKind.R4, 14)
            TextLoader.Column("V15", Nullable DataKind.R4, 15)
            TextLoader.Column("V16", Nullable DataKind.R4, 16)
            TextLoader.Column("V17", Nullable DataKind.R4, 17)
            TextLoader.Column("V18", Nullable DataKind.R4, 18)
            TextLoader.Column("V19", Nullable DataKind.R4, 19)
            TextLoader.Column("V20", Nullable DataKind.R4, 20)
            TextLoader.Column("V21", Nullable DataKind.R4, 21)
            TextLoader.Column("V22", Nullable DataKind.R4, 22)
            TextLoader.Column("V23", Nullable DataKind.R4, 23)
            TextLoader.Column("V24", Nullable DataKind.R4, 24)
            TextLoader.Column("V25", Nullable DataKind.R4, 25)
            TextLoader.Column("V26", Nullable DataKind.R4, 26)
            TextLoader.Column("V27", Nullable DataKind.R4, 27)
            TextLoader.Column("V28", Nullable DataKind.R4, 28)
            TextLoader.Column("Amount", Nullable DataKind.R4, 29)
        ]
        |> List.toArray

    let loaderArgs = TextLoader.Arguments()
    loaderArgs.Column <- columns
    loaderArgs.HasHeader <- true
    loaderArgs.Separators <- [| ',' |]
    
    let reader = TextLoader (mlContext, loaderArgs)
    
    let classification = BinaryClassificationCatalog mlContext
  
    (*
    Split the data 80:20 into train and test files, 
    if the files do not exist yet.
    *)

    if not (File.Exists trainFile && File.Exists testFile)
    then
        printfn "Preparing train and test data"

        let data = 
            MultiFileSource inputFile
            |> reader.Read

        let trainData, testData = 
            classification.TrainTestSplit (data, 0.2) 
            |> fun x -> x.ToTuple ()

        // save test split
        use fileStream = File.Create testFile
        mlContext.Data.SaveAsText(testData, fileStream, separatorChar = ',', headerRow = true, schema = true)
        
        // save train split 
        use fileStream = File.Create trainFile
        mlContext.Data.SaveAsText(trainData, fileStream, separatorChar = ',', headerRow = true, schema = true)

    (*
    Read the train and test data from file
    *)

    // Add the "StratificationColumn" that was added by classification.TrainTestSplit()
    // And Label is moved to column 0
    let columnsPlus = 
        [
            // A boolean column depicting the 'label'.
            TextLoader.Column("Label", Nullable DataKind.BL, 0)
            // 30 Features V1..V28 + Amount + StratificationColumn
            TextLoader.Column("V1", Nullable DataKind.R4, 1)
            TextLoader.Column("V2", Nullable DataKind.R4, 2)
            TextLoader.Column("V3", Nullable DataKind.R4, 3)
            TextLoader.Column("V4", Nullable DataKind.R4, 4)
            TextLoader.Column("V5", Nullable DataKind.R4, 5)
            TextLoader.Column("V6", Nullable DataKind.R4, 6)
            TextLoader.Column("V7", Nullable DataKind.R4, 7)
            TextLoader.Column("V8", Nullable DataKind.R4, 8)
            TextLoader.Column("V9", Nullable DataKind.R4, 9)
            TextLoader.Column("V10", Nullable DataKind.R4, 10)
            TextLoader.Column("V11", Nullable DataKind.R4, 11)
            TextLoader.Column("V12", Nullable DataKind.R4, 12)
            TextLoader.Column("V13", Nullable DataKind.R4, 13)
            TextLoader.Column("V14", Nullable DataKind.R4, 14)
            TextLoader.Column("V15", Nullable DataKind.R4, 15)
            TextLoader.Column("V16", Nullable DataKind.R4, 16)
            TextLoader.Column("V17", Nullable DataKind.R4, 17)
            TextLoader.Column("V18", Nullable DataKind.R4, 18)
            TextLoader.Column("V19", Nullable DataKind.R4, 19)
            TextLoader.Column("V20", Nullable DataKind.R4, 20)
            TextLoader.Column("V21", Nullable DataKind.R4, 21)
            TextLoader.Column("V22", Nullable DataKind.R4, 22)
            TextLoader.Column("V23", Nullable DataKind.R4, 23)
            TextLoader.Column("V24", Nullable DataKind.R4, 24)
            TextLoader.Column("V25", Nullable DataKind.R4, 25)
            TextLoader.Column("V26", Nullable DataKind.R4, 26)
            TextLoader.Column("V27", Nullable DataKind.R4, 27)
            TextLoader.Column("V28", Nullable DataKind.R4, 28)
            TextLoader.Column("Amount", Nullable DataKind.R4, 29)
            TextLoader.Column("StratificationColumn", Nullable DataKind.R4, 30)
        ]
        |> List.toArray

    let trainData, testData = 

        printfn "Reading train and test data"

        let trainData =
            mlContext.Data.ReadFromTextFile(
                trainFile,
                columnsPlus,                                                           
                loaderArgs.HasHeader,
                loaderArgs.Separators.[0]
                )
                                                                  
        let testData = 
            mlContext.Data.ReadFromTextFile(
                testFile,
                columnsPlus,
                loaderArgs.HasHeader,
                loaderArgs.Separators.[0]
                )
    
        trainData, testData
      
    (*
    Create a flexible pipeline (composed by a chain of estimators) 
    for building/traing the model.
    *)

    let featureColumnNames = 
        trainData.Schema
        |> Seq.map (fun column -> column.Name)
        |> Seq.filter (fun name -> name <> "Label")
        |> Seq.filter (fun name -> name <> "StratificationColumn")
        |> Seq.toArray

    let pipeline = 
        mlContext.Transforms.Concatenate ("Features", featureColumnNames)
        |> fun x -> 
            x.Append (
                mlContext.Transforms.Normalize (
                    "FeaturesNormalizedByMeanVar", 
                    "Features", 
                    NormalizingEstimator.NormalizerMode.MeanVariance
                    )
                )
        |> fun x -> 
            x.Append (
                mlContext.BinaryClassification.Trainers.FastTree(
                    "Label", 
                    "Features", 
                    numLeaves = 20, 
                    numTrees = 100, 
                    minDatapointsInLeaves = 10, 
                    learningRate = 0.2
                    )
                )

    printfn "Training model"
    let model = pipeline.Fit trainData

    let metrics = classification.Evaluate (model.Transform (testData), "Label")   
    printfn "Accuracy: %.2f" metrics.Accuracy 

    printfn "Saving model to file"
    let _ = 
        use fs = new FileStream (modelFile, FileMode.Create, FileAccess.Write, FileShare.Write)
        mlContext.Model.Save(model, fs)

    (*
    Read the model and test data from file,
    and make predictions
    *)

    printfn "Reading model and test data"
    let modelEvaluator = 
        use file = File.OpenRead modelFile           
        mlContext.Model.Load(file)
    let predictionEngine = modelEvaluator.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(mlContext)

    let testData = mlContext.Data.ReadFromTextFile (testFile, columnsPlus, hasHeader = true, separatorChar = ',')

    printfn "Making predictions"
    mlContext.CreateEnumerable<TransactionObservation>(testData, reuseRowObject = false)
    |> Seq.filter (fun x -> x.Label = true)
    // use 5 observations from the test data
    |> Seq.take 5
    |> Seq.iter (fun testData -> 
        let prediction = predictionEngine.Predict testData
        printfn "%A" prediction
        printfn "------"
        )

    printfn "Press Enter to quit"
    let _ = Console.ReadKey ()

    0 // return an integer exit code
