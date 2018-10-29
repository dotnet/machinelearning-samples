module MulticlassClassification_Iris

open System
open System.IO

open Microsoft.ML.Runtime.Api;
open Microsoft.ML.Legacy;
open Microsoft.ML.Legacy.Models;
open Microsoft.ML.Legacy.Data;
open Microsoft.ML.Legacy.Transforms;
open Microsoft.ML.Legacy.Trainers;

let AppPath = Path.Combine(__SOURCE_DIRECTORY__, "../../../..")
let TrainDataPath= Path.Combine(AppPath, "datasets", "iris-train.txt")
let TestDataPath= Path.Combine(AppPath,  "datasets", "iris-test.txt")
let ModelPath= Path.Combine(AppPath, "IrisModel.zip")

[<CLIMutable>]
type IrisData = {
    [<Column("0")>] Label : float32
    [<Column("1")>] SepalLength : float32
    [<Column("2")>] SepalWidth : float32
    [<Column("3")>] PetalLength : float32
    [<Column("4")>] PetalWidth : float32
} with static member Empty = {
        Label = 0.0f
        SepalLength = 0.0f
        SepalWidth = 0.0f
        PetalLength = 0.0f
        PetalWidth = 0.0f
    }

[<CLIMutable>]
type IrisPrediction = {
        [<ColumnName("Score")>] Score : float32 []
    }

//type IrisData() =
//    [<Column("0")>]
//    member val Label: float32 = 0.0f with get,set

//    [<Column("1")>]
//    member val SepalLength: float32 = 0.0f with get, set

//    [<Column("2")>]
//    member val SepalWidth: float32 = 0.0f with get, set

//    [<Column("3")>]
//    member val PetalLength: float32 = 0.0f with get, set

//    [<Column("4")>]
//    member val PetalWidth: float32 = 0.0f with get, set

//type IrisPrediction() =

    //[<ColumnName("Score")>]
    //member val Score: float32[] = null with get, set


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
    let Iris1 = { IrisData.Empty with SepalLength = 5.1f; SepalWidth = 3.3f; PetalLength = 1.6f; PetalWidth= 0.2f}
    let Iris2 = { IrisData.Empty with SepalLength = 6.4f; SepalWidth = 3.1f; PetalLength = 5.5f; PetalWidth = 2.2f}
    let Iris3 = { IrisData.Empty with SepalLength = 4.4f; SepalWidth = 3.1f; PetalLength = 2.5f; PetalWidth = 1.2f}

let Evaluate(model : PredictionModel<IrisData, IrisPrediction>) =
    // To evaluate how good the model predicts values, the model is ran against new set
    // of data (test data) that was not involved in training.
    let testData = TextLoader(TestDataPath).CreateFrom<IrisData>()

    // ClassificationEvaluator performs evaluation for Multiclass Classification type of ML problems.
    let evaluator = ClassificationEvaluator(OutputTopKAcc = Nullable(3))

    printfn "=============== Evaluating model ==============="

    let metrics = evaluator.Evaluate(model, testData)
    printfn "Metrics:"
    printfn "    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better"
    printfn "    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better"
    printfn "    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better"
    printfn "    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better"
    printfn "    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better"
    printfn "    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better"
    printfn ""
    printfn "    ConfusionMatrix:"

    // Print confusion matrix
    for i in 0 .. metrics.ConfusionMatrix.Order - 1 do
        for j in 0 .. metrics.ConfusionMatrix.ClassNames.Count - 1 do
            printfn "\t%s" (string metrics.ConfusionMatrix.[i, j])
        printfn ""

    printfn "=============== End evaluating ==============="
    printfn ""

// STEP 1: Create a model
let model = TrainAsync()

// STEP2: Test accuracy
Evaluate(model)

// STEP 3: Make a prediction
printfn ""
let prediction1 = model.Predict(TestIrisData.Iris1)
printfn "Actual: setosa.     Predicted probability: setosa:      %0.4f" prediction1.Score.[0]
printfn "                                           versicolor:  %0.4f" prediction1.Score.[1]
printfn "                                           virginica:   %0.4f" prediction1.Score.[2]
printfn ""

let prediction2 = model.Predict(TestIrisData.Iris2)
printfn "Actual: virginica.  Predicted probability: setosa:      %0.4f" prediction2.Score.[0]
printfn "                                           versicolor:  %0.4f" prediction2.Score.[1]
printfn "                                           virginica:   %0.4f" prediction2.Score.[2]
printfn ""

let prediction3 = model.Predict(TestIrisData.Iris3)
printfn "Actual: versicolor. Predicted probability: setosa:      %0.4f" prediction3.Score.[0]
printfn "                                           versicolor:  %0.4f" prediction3.Score.[1]
printfn "                                           virginica:   %0.4f" prediction3.Score.[2]

Console.ReadLine() |> ignore
