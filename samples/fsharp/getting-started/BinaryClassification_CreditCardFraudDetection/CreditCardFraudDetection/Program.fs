open System
open System.IO
open System.IO.Compression

open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms

// Data models
[<CLIMutable>]
type TransactionObservation = {
    [<LoadColumn(0)>] Time: float32
    [<LoadColumn(1)>] V1: float32
    [<LoadColumn(2)>] V2: float32
    [<LoadColumn(3)>] V3: float32
    [<LoadColumn(4)>] V4: float32
    [<LoadColumn(5)>] V5: float32
    [<LoadColumn(6)>] V6: float32
    [<LoadColumn(7)>] V7: float32
    [<LoadColumn(8)>] V8: float32
    [<LoadColumn(9)>] V9: float32
    [<LoadColumn(10)>] V10: float32
    [<LoadColumn(11)>] V11: float32
    [<LoadColumn(12)>] V12: float32
    [<LoadColumn(13)>] V13: float32
    [<LoadColumn(14)>] V14: float32
    [<LoadColumn(15)>] V15: float32
    [<LoadColumn(16)>] V16: float32
    [<LoadColumn(17)>] V17: float32
    [<LoadColumn(18)>] V18: float32
    [<LoadColumn(19)>] V19: float32
    [<LoadColumn(20)>] V20: float32
    [<LoadColumn(21)>] V21: float32
    [<LoadColumn(22)>] V22: float32
    [<LoadColumn(23)>] V23: float32
    [<LoadColumn(24)>] V24: float32
    [<LoadColumn(25)>] V25: float32
    [<LoadColumn(26)>] V26: float32
    [<LoadColumn(27)>] V27: float32
    [<LoadColumn(28)>] V28: float32
    [<LoadColumn(29)>] Amount: float32
    [<LoadColumn(30)>] Label: bool
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

    (*
    Split the data 80:20 into train and test files, 
    if the files do not exist yet.
    *)

    if not (File.Exists trainFile && File.Exists testFile)
    then
        printfn "Preparing train and test data"

        let data = mlContext.Data.LoadFromTextFile<TransactionObservation>(inputFile, separatorChar = ',', hasHeader = true, allowQuoting = true)

        let trainData, testData = 
            let y = mlContext.Data.TrainTestSplit(data, 0.2, seed = Nullable 1) 
            y.TrainSet, y.TestSet

        // save test split
        use fileStream = File.Create testFile
        mlContext.Data.SaveAsText(testData, fileStream, separatorChar = ',', headerRow = true, schema = true)
        
        // save train split 
        use fileStream = File.Create trainFile
        mlContext.Data.SaveAsText(trainData, fileStream, separatorChar = ',', headerRow = true, schema = true)

    (*
    Read the train and test data from file
    *)

    let trainData, testData = 
        printfn "Reading train and test data"
        let trainData = mlContext.Data.LoadFromTextFile<TransactionObservation>(trainFile, separatorChar = ',', hasHeader = true)
        let testData = mlContext.Data.LoadFromTextFile<TransactionObservation>(testFile, separatorChar = ',', hasHeader = true)
        trainData, testData
      
    (*
    Create a flexible pipeline (composed by a chain of estimators) 
    for building/traing the model.
    *)

    let featureColumnNames = 
        trainData.Schema
        |> Seq.map (fun column -> column.Name)
        |> Seq.filter (fun name -> name <> "Time")
        |> Seq.filter (fun name -> name <> "Label")
        |> Seq.filter (fun name -> name <> "IdPreservationColumn")
        |> Seq.toArray

    let pipeline = 
        EstimatorChain()
        |> fun x -> x.Append(mlContext.Transforms.Concatenate("Features", featureColumnNames))
        |> fun x -> x.Append(mlContext.Transforms.DropColumns [|"Time"|])
        |> fun x -> 
            x.Append (
                mlContext.Transforms.NormalizeMeanVariance (
                    "FeaturesNormalizedByMeanVar", 
                    "Features"
                    )
                )
        |> fun x -> 
            x.Append (
                mlContext.BinaryClassification.Trainers.FastTree(
                    "Label", 
                    "FeaturesNormalizedByMeanVar", 
                    numberOfLeaves = 20, 
                    numberOfTrees = 100, 
                    minimumExampleCountPerLeaf = 10, 
                    learningRate = 0.2
                    )
                )

    printfn "Training model"
    let model = pipeline.Fit trainData

    let metrics = mlContext.BinaryClassification.Evaluate(model.Transform (testData), "Label")   
    printfn "Accuracy: %.12f" metrics.Accuracy 

    printfn "Saving model to file"
    let _ = 
        use fs = new FileStream (modelFile, FileMode.Create, FileAccess.Write, FileShare.Write)
        mlContext.Model.Save(model, trainData.Schema, fs)

    (*
    Read the model and test data from file,
    and make predictions
    *)

    printfn "Reading model and test data"
    let modelEvaluator, inputSchema = 
        use file = File.OpenRead modelFile           
        mlContext.Model.Load(file)
    let predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(modelEvaluator)

    let testData = mlContext.Data.LoadFromTextFile<TransactionObservation>(testFile, hasHeader = true, separatorChar = ',')

    printfn "Making predictions"
    mlContext.Data.CreateEnumerable<TransactionObservation>(testData, reuseRowObject = false)
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
