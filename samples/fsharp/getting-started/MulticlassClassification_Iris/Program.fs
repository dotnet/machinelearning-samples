module MulticlassClassification_Iris

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Runtime.Api
open Microsoft.ML.Models
open Microsoft.ML.Trainers
open Microsoft.ML.Transforms

let AppPath = Path.Combine(__SOURCE_DIRECTORY__, "../../../..")
let TrainDataPath= Path.Combine(AppPath, "datasets", "iris-train.txt")
let TestDataPath= Path.Combine(AppPath,  "datasets", "iris-test.txt")
let ModelPath= Path.Combine(AppPath, "IrisModel.zip")

type IrisData() = 
    [<Column("0")>]
    member val Label: float32 = 0.0f with get,set

    [<Column("1")>]
    member val SepalLength: float32 = 0.0f with get, set

    [<Column("2")>]
    member val SepalWidth: float32 = 0.0f with get, set

    [<Column("3")>]
    member val PetalLength: float32 = 0.0f with get, set

    [<Column("4")>]
    member val PetalWidth: float32 = 0.0f with get, set

type IrisPrediction() = 

    [<ColumnName("Score")>]
    member val Score: float32[] = null with get, set


let TrainAsync() =
   // LearningPipeline holds all steps of the learning process: data, transforms, learners.
    let pipeline = LearningPipeline()
        // The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
        // all the column names and their types.
    pipeline.Add(TextLoader(TrainDataPath).CreateFrom<IrisData>())
        
    // Transforms
    // When ML model starts training, it looks for two columns: Label and Features.
    // Label:   values that should be predicted. If you have a field named Label in your data type,
    //              like in this example, no extra actions required.
    //          If you don’t have it, copy the column you want to predict with ColumnCopier transform:
    //              new ColumnCopier(("FareAmount", "Label"))
    // Features: all data used for prediction. At the end of all transforms you need to concatenate
    //              all columns except the one you want to predict into Features column with
    //              ColumnConcatenator transform:
    pipeline.Add(ColumnConcatenator("Features",
                    "SepalLength",
                    "SepalWidth",
                    "PetalLength",
                    "PetalWidth"))

    // StochasticDualCoordinateAscentClassifier is an algorithm that will be used to train the model.
    pipeline.Add(StochasticDualCoordinateAscentClassifier())

    Console.WriteLine("=============== Training model ===============")
    // The pipeline is trained on the dataset that has been loaded and transformed.
    let model = pipeline.Train<IrisData, IrisPrediction>()

    // Saving the model as a .zip file.
    model.WriteAsync(ModelPath) |> Async.AwaitTask |> Async.RunSynchronously

    Console.WriteLine("=============== End training ===============")
    Console.WriteLine("The model is saved to {0}", ModelPath)

    model

module TestIrisData = 
    let Iris1 = IrisData(SepalLength = 5.1f, SepalWidth = 3.3f, PetalLength = 1.6f, PetalWidth= 0.2f)
    let Iris2 = IrisData(SepalLength = 6.4f, SepalWidth = 3.1f, PetalLength = 5.5f, PetalWidth = 2.2f)
    let Iris3 = IrisData(SepalLength = 4.4f, SepalWidth = 3.1f, PetalLength = 2.5f, PetalWidth = 1.2f)

let Evaluate(model : PredictionModel<IrisData, IrisPrediction>) =
    // To evaluate how good the model predicts values, the model is ran against new set
    // of data (test data) that was not involved in training.
    let testData = TextLoader(TestDataPath).CreateFrom<IrisData>()
    
    // ClassificationEvaluator performs evaluation for Multiclass Classification type of ML problems.
    let evaluator = ClassificationEvaluator(OutputTopKAcc = Nullable(3))
    
    Console.WriteLine("=============== Evaluating model ===============")

    let metrics = evaluator.Evaluate(model, testData)
    Console.WriteLine("Metrics:")
    Console.WriteLine(sprintf "    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better")
    Console.WriteLine(sprintf "    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better")
    Console.WriteLine(sprintf "    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better")
    Console.WriteLine(sprintf "    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better")
    Console.WriteLine(sprintf "    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better")
    Console.WriteLine(sprintf "    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better")
    Console.WriteLine()
    Console.WriteLine(sprintf "    ConfusionMatrix:")

    // Print confusion matrix
    for i in 0 .. metrics.ConfusionMatrix.Order - 1 do
        for j in 0 .. metrics.ConfusionMatrix.ClassNames.Count - 1 do
            Console.Write("\t" + string metrics.ConfusionMatrix.[i, j])
        Console.WriteLine()

    Console.WriteLine("=============== End evaluating ===============")
    Console.WriteLine()

// STEP 1: Create a model
let model = TrainAsync()

// STEP2: Test accuracy
Evaluate(model)

// STEP 3: Make a prediction
Console.WriteLine()
let prediction1 = model.Predict(TestIrisData.Iris1)
Console.WriteLine(sprintf "Actual: setosa.     Predicted probability: setosa:      %0.4f" prediction1.Score.[0])
Console.WriteLine(sprintf "                                           versicolor:  %0.4f" prediction1.Score.[1])
Console.WriteLine(sprintf "                                           virginica:   %0.4f" prediction1.Score.[2])
Console.WriteLine()

let prediction2 = model.Predict(TestIrisData.Iris2)
Console.WriteLine(sprintf "Actual: virginica.  Predicted probability: setosa:      %0.4f" prediction2.Score.[0])
Console.WriteLine(sprintf "                                           versicolor:  %0.4f" prediction2.Score.[1])
Console.WriteLine(sprintf "                                           virginica:   %0.4f" prediction2.Score.[2])
Console.WriteLine()

let prediction3 = model.Predict(TestIrisData.Iris3)
Console.WriteLine(sprintf "Actual: versicolor. Predicted probability: setosa:      %0.4f" prediction3.Score.[0])
Console.WriteLine(sprintf "                                           versicolor:  %0.4f" prediction3.Score.[1])
Console.WriteLine(sprintf "                                           virginica:   %0.4f" prediction3.Score.[2])

Console.ReadLine() |> ignore
