open Microsoft.ML
open Microsoft.ML.Data
open System
open System.IO
open Microsoft.ML.Transforms

[<CLIMutable>]
type Input =
    {
        [<LoadColumn(0,63); VectorType(64)>]
        PixelValues : float32 []
        [<LoadColumn(64)>]
        Number : float32
    }
[<CLIMutable>]
type Output = {Score : float32 []}
    
let sampleData = 
    [|
        1, {
            Number = 0.f
            PixelValues = [|0.f;0.f;0.f;0.f;14.f;13.f;1.f;0.f;0.f;0.f;0.f;5.f;16.f;16.f;2.f;0.f;0.f;0.f;0.f;14.f;16.f;12.f;0.f;0.f;0.f;1.f;10.f;16.f;16.f;12.f;0.f;0.f;0.f;3.f;12.f;14.f;16.f;9.f;0.f;0.f;0.f;0.f;0.f;5.f;16.f;15.f;0.f;0.f;0.f;0.f;0.f;4.f;16.f;14.f;0.f;0.f;0.f;0.f;0.f;1.f;13.f;16.f;1.f;0.f|]
        }
        7, {
            Number = 0.f
            PixelValues = [|0.f;0.f;1.f;8.f;15.f;10.f;0.f;0.f;0.f;3.f;13.f;15.f;14.f;14.f;0.f;0.f;0.f;5.f;10.f;0.f;10.f;12.f;0.f;0.f;0.f;0.f;3.f;5.f;15.f;10.f;2.f;0.f;0.f;0.f;16.f;16.f;16.f;16.f;12.f;0.f;0.f;1.f;8.f;12.f;14.f;8.f;3.f;0.f;0.f;0.f;0.f;10.f;13.f;0.f;0.f;0.f;0.f;0.f;0.f;11.f;9.f;0.f;0.f;0.f|]
        }
        9, {
            Number = 0.f
            PixelValues = [|0.f;0.f;6.f;14.f;4.f;0.f;0.f;0.f;0.f;0.f;11.f;16.f;10.f;0.f;0.f;0.f;0.f;0.f;8.f;14.f;16.f;2.f;0.f;0.f;0.f;0.f;1.f;12.f;12.f;11.f;0.f;0.f;0.f;0.f;0.f;0.f;0.f;11.f;3.f;0.f;0.f;0.f;0.f;0.f;0.f;5.f;11.f;0.f;0.f;0.f;1.f;4.f;4.f;7.f;16.f;2.f;0.f;0.f;7.f;16.f;16.f;13.f;11.f;1.f|]
        }
    |]

let assemblyFolderPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

let baseDatasetsRelativePath = @"../../../Data"
let trianDataRealtivePath = Path.Combine(baseDatasetsRelativePath, "optdigits-train.csv")
let testDataRealtivePath = Path.Combine(baseDatasetsRelativePath, "optdigits-val.csv")
let trainDataPath = Path.Combine(assemblyFolderPath, trianDataRealtivePath)
let testDataPath = Path.Combine(assemblyFolderPath, testDataRealtivePath)

let baseModelsRelativePath = @"../../../MLModels";
let modelRelativePath = Path.Combine(baseModelsRelativePath, "Model.zip")
let modelPath = Path.Combine(assemblyFolderPath, modelRelativePath)

let mlContext = new MLContext()


// STEP 1: Common data loading configuration
let trainData = mlContext.Data.LoadFromTextFile<Input>(trainDataPath, separatorChar=',', hasHeader=false)
let testData = mlContext.Data.LoadFromTextFile<Input>(testDataPath, separatorChar=',', hasHeader=false)

// STEP 2: Common data process configuration with pipeline data transformations
// Use in-memory cache for small/medium datasets to lower training time. Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.
let dataProcessPipeline = 
    EstimatorChain() 
        .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", "Number", keyOrdinality=ValueToKeyMappingEstimator.KeyOrdinality.ByValue))
        .Append(mlContext.Transforms.Concatenate("Features", "PixelValues"))
        .AppendCacheCheckpoint(mlContext)

// STEP 3: Set the training algorithm, then create and config the modelBuilder
let trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features")
let trainingPipeline = 
    dataProcessPipeline
        .Append(trainer)
        .Append(mlContext.Transforms.Conversion.MapKeyToValue("Number", "Label"))

// STEP 4: Train the model fitting to the DataSet
printfn "=============== Training the model ==============="
let trainedModel = trainingPipeline.Fit(trainData)

printfn "===== Evaluating Model's accuracy with Test data ====="
let predictions = trainedModel.Transform(testData)
let metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Number", "Score")

Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metrics

mlContext.Model.Save(trainedModel, trainData.Schema, modelPath)

printfn "The model is saved to %s" modelPath

// Test some predicitions

let loadedTrainedModel, modelInputSchema = mlContext.Model.Load modelPath

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<Input, Output>(loadedTrainedModel)
            
sampleData
|> Array.iter 
    (fun (n,dat) ->
        let p = predEngine.Predict dat
        printfn "Actual: %d     Predicted probability:       zero:  %.4f" n p.Score.[0]
        ["one:"; "two:"; "three:"; "four:"; "five:"; "six:"; "seven:"; "eight:"; "nine:"]
        |> List.iteri 
            (fun i w ->
                let i = i + 1
                printfn "                                           %-6s %.4f" w p.Score.[i]
            )
        printfn ""
    )


printfn "Hit any key to finish the app"
Console.ReadKey() |> ignore



