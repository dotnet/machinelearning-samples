open System
open System.IO
open System.Linq
open Common
open Microsoft.ML
open Microsoft.ML.AutoML
open Microsoft.ML.Data
open Common
open System.Collections.Generic

[<CLIMutable>]
type InputData =
    {
        [<LoadColumn(0,63); VectorType(64)>]
        PixelValues : float32 []
        [<LoadColumn(64)>]
        Number : float32
    }
    
[<CLIMutable>]
type OutputData =
    {
        [<ColumnName("Score");VectorType(10)>]
        Score : float32 []
    }

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
    |]

let assemblyFolderPath = Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName
let absolutePath x = Path.Combine(assemblyFolderPath, x)

let baseDatasetsRelativePath = @"Data"

let trainDataRelativePath = Path.Combine(baseDatasetsRelativePath, "optdigits-train.csv")
let trainDataPath = absolutePath trainDataRelativePath

let testDataRelativePath = Path.Combine(baseDatasetsRelativePath, "optdigits-test.csv")
let testDataPath = absolutePath testDataRelativePath

let baseModelsRelativePath = @"../../../MLModels"
let modelRelativePath = Path.Combine(baseModelsRelativePath, "Model.zip")
let modelPath = absolutePath modelRelativePath

let experimentTimeInSeconds = 60u

let mlContext = MLContext()
            
try
    // STEP 1: Load the data
    let load path =
        mlContext.Data.LoadFromTextFile(path=path,
            columns=
                [|
                    TextLoader.Column("PixelValues", DataKind.Single, 0, 63)
                    TextLoader.Column("Number", DataKind.Single, 64)
                |],
            hasHeader = false,
            separatorChar = ',')
    let trainData = load trainDataPath
    let testData = load testDataPath

    // STEP 2: Initialize our user-defined progress handler that AutoML will 
    // invoke after each model it produces and evaluates.
    let progressHandler = ConsoleHelper.multiclassExperimentProgressHandler()

    // STEP 3: Run an AutoML multiclass classification experiment
    ConsoleHelper.consoleWriteHeader "=============== Running AutoML experiment ==============="
    printfn "Running AutoML multiclass classification experiment for %d seconds..." experimentTimeInSeconds
    let experimentResult = mlContext.Auto().CreateMulticlassClassificationExperiment(experimentTimeInSeconds).Execute(trainData, "Number", progressHandler = progressHandler)

    // Print top models found by AutoML
    printfn ""
    printfn "Top models ranked by accuracy --"
    experimentResult.RunDetails
    |> Seq.filter (fun r -> not (isNull r.ValidationMetrics) && not (Double.IsNaN r.ValidationMetrics.MicroAccuracy))
    |> Seq.sortBy (fun x -> x.ValidationMetrics.MicroAccuracy)
    |> Seq.truncate 3
    |> Seq.iteri (fun i x -> ConsoleHelper.printMulticlassIterationMetrics (i + 1) x.TrainerName x.ValidationMetrics x.RuntimeInSeconds) 

    // STEP 4: Evaluate the model and print metrics
    ConsoleHelper.consoleWriteHeader "===== Evaluating model's accuracy with test data ====="
    let bestRun = experimentResult.BestRun
    let trainedModel = bestRun.Model
    let predictions = trainedModel.Transform(testData)
    let metrics = mlContext.MulticlassClassification.Evaluate(data = predictions, labelColumnName = "Number", scoreColumnName = "Score")
    ConsoleHelper.printMultiClassClassificationMetrics bestRun.TrainerName metrics

    // STEP 5: Save/persist the trained model to a .ZIP file
    mlContext.Model.Save(trainedModel, trainData.Schema, modelPath);

    printfn "The model is saved to %s" modelPath
with 
| ex -> printfn "%O" ex


let loadedTrainedModel, modelInputSchema = mlContext.Model.Load modelPath

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(loadedTrainedModel)

// Get the key value mapping for Number to Score index
let key =
    let outputSchema = loadedTrainedModel.GetOutputSchema(modelInputSchema)
    let mutable keyValues = Unchecked.defaultof<_>
    outputSchema.["Number"].GetKeyValues<float32>(&keyValues)
    keyValues.Items() 
    |> Seq.map (fun x -> int x.Value, x.Key )
    |> dict
    

sampleData
|> Array.iter 
    (fun (n,dat) ->
        let p = predEngine.Predict dat
        printfn "Actual: %d     Predicted probability:       zero:  %.4f" n p.Score.[key.[0]]
        ["one:"; "two:"; "three:"; "four:"; "five:"; "six:"; "seven:"; "eight:"; "nine:"]
        |> List.iteri 
            (fun i w ->
                let i = i + 1
                printfn "                                           %-6s %.4f" w p.Score.[key.[i]]
            )
        printfn ""
    )


printfn "Hit any key to finish the app"
Console.ReadKey() |> ignore
