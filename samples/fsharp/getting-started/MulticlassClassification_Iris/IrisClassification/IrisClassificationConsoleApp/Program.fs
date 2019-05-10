// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.ML
open MulticlassClassification_Iris
open MulticlassClassification_Iris.DataStructures
open Microsoft.ML.Data

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data";
let trainDataPath = sprintf @"%s/iris-train.txt" baseDatasetsLocation
let testDataPath = sprintf @"%s/iris-test.txt" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels"
let modelPath = sprintf @"%s/IrisClassificationModel.zip" baseModelsPath


let buildTrainEvaluateAndSaveModel (mlContext : MLContext) =
    
    // STEP 1: Common data loading configuration
    let trainingDataView = mlContext.Data.LoadFromTextFile<IrisData>(trainDataPath, hasHeader = true)
    let testDataView = mlContext.Data.LoadFromTextFile<IrisData>(testDataPath, hasHeader = true)

    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline = 
        EstimatorChain()
            .Append(mlContext.Transforms.Conversion.MapValueToKey("LabelKey","Label"))
            .Append(mlContext.Transforms.Concatenate("Features", "SepalLength",
                                                     "SepalWidth",
                                                     "PetalLength",
                                                     "PetalWidth"))
            .AppendCacheCheckpoint(mlContext)

    // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
    let trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName = "LabelKey", featureColumnName = "Features")
    let trainingPipeline = dataProcessPipeline.Append(trainer)

    // STEP 4: Train the model fitting to the DataSet
    printfn "=============== Training the model ==============="
    let trainedModel = trainingPipeline.Fit(trainingDataView)

    // STEP 5: Evaluate the model and show accuracy stats
    printfn "===== Evaluating Model's accuracy with Test data ====="
    let predictions = trainedModel.Transform(testDataView)
    let metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score")

    Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metrics

    // STEP 6: Save/persist the trained model to a .ZIP file
    use fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
    mlContext.Model.Save(trainedModel, trainingDataView.Schema, fs);

    printfn "The model is saved to %s" modelPath


let testSomePredictions (mlContext : MLContext) =
    //Test Classification Predictions with some hard-coded samples 
    use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
    let trainedModel, inputSchema = mlContext.Model.Load(stream);

    // Create prediction engine related to the loaded trained model
    let predEngine = mlContext.Model.CreatePredictionEngine<IrisData, IrisPrediction>(trainedModel)

    //Score sample 1
    let resultprediction1 = predEngine.Predict(DataStructures.SampleIrisData.Iris1)

    printfn "Actual: setosa.     Predicted probability: setosa:      %.4f" resultprediction1.Score.[0]
    printfn "                                           versicolor:  %.4f" resultprediction1.Score.[1]
    printfn "                                           virginica:   %.4f" resultprediction1.Score.[2]
    printfn ""

    //Score sample 2
    let resultprediction2 = predEngine.Predict(DataStructures.SampleIrisData.Iris2);
    printfn "Actual: virginica.  Predicted probability: setosa:      %.4f" resultprediction2.Score.[0]
    printfn "                                           versicolor:  %.4f" resultprediction2.Score.[1]
    printfn "                                           virginica:   %.4f" resultprediction2.Score.[2]
    printfn ""

    //Score sample 3
    let resultprediction2 = predEngine.Predict(DataStructures.SampleIrisData.Iris3);
    printfn "Actual: versicolor. Predicted probability: setosa:      %.4f" resultprediction2.Score.[0]
    printfn "                                           versicolor:  %.4f" resultprediction2.Score.[1]
    printfn "                                           virginica:   %.4f" resultprediction2.Score.[2]
    printfn ""


[<EntryPoint>]
let main argv =
    
    // Create MLContext to be shared across the model creation workflow objects 
    // Set a random seed for repeatable/deterministic results across multiple trainings.
    let mlContext = MLContext(seed = Nullable 0)

    //1.
    buildTrainEvaluateAndSaveModel mlContext

    //2.
    testSomePredictions mlContext

    printfn "=============== End of process, hit any key to finish ==============="
    Console.ReadKey() |> ignore

    0 // return an integer exit code
