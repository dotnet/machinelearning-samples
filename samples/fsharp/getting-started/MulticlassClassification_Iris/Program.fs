module MulticlassClassification_Iris

open System
open System.IO

open Microsoft.ML.Runtime.Learners
open Microsoft.ML.Runtime.Data
open Microsoft.ML
open System.IO


let AppPath = Path.Combine(__SOURCE_DIRECTORY__, "../../../..")
let TrainDataPath= Path.Combine(AppPath, "datasets", "iris-train.txt")
let TestDataPath= Path.Combine(AppPath,  "datasets", "iris-test.txt")
let ModelPath= Path.Combine(AppPath, "IrisModel.zip")

/// Holds information about Iris flower to be classified.
[<CLIMutable>]
type IrisData = {
    SepalLength : float32
    SepalWidth : float32
    PetalLength : float32
    PetalWidth : float32
} 

/// Result of Iris classification. The array holds probability of the flower to be one of setosa, virginica or versicolor.
[<CLIMutable>]
type IrisPrediction = {
        Score : float32 []
    }


module TestIrisData =
    let Iris1 = { SepalLength = 5.1f; SepalWidth = 3.3f; PetalLength = 1.6f; PetalWidth= 0.2f }
    let Iris2 = { SepalLength = 6.4f; SepalWidth = 3.1f; PetalLength = 5.5f; PetalWidth = 2.2f }
    let Iris3 = { SepalLength = 4.4f; SepalWidth = 3.1f; PetalLength = 2.5f; PetalWidth = 1.2f }


module Pipeline =
    open Microsoft.ML.Core.Data

    let textTransform (inputColumn : string) outputColumn env =
        TextTransform(env, inputColumn, outputColumn)

    let concatEstimator name source env =
        ConcatEstimator(env,name, source)

    let append (estimator : IEstimator<'b>) (pipeline : IEstimator<ITransformer>)  = 
        pipeline.Append estimator
        
    let fit (dataView : IDataView) (pipeline : EstimatorChain<'a>) =
        pipeline.Fit dataView



[<EntryPoint>]
let main argv =

    //1. Create ML.NET context/environment
    use env = new LocalEnvironment()

    //2. Create DataReader with data schema mapped to file's columns
    let reader = 
        TextLoader(
            env, 
            TextLoader.Arguments(
                Separator = "tab", 
                HasHeader = true, 
                Column = 
                    [|
                        TextLoader.Column("Label", Nullable DataKind.R4, 0)
                        TextLoader.Column("SepalLength", Nullable DataKind.R4, 1)
                        TextLoader.Column("SepalWidth", Nullable DataKind.R4, 2)
                        TextLoader.Column("PetalLength", Nullable DataKind.R4, 3)
                        TextLoader.Column("PetalWidth", Nullable DataKind.R4, 4)
                    |]
                )
            )

    //Load training data
    let trainingDataView = MultiFileSource(TrainDataPath) |> reader.Read

    printfn "=============== Create and Train the Model ==============="

    let model = 
        env
        //3.Create a flexible pipeline (composed by a chain of estimators) for creating/traing the model.
        |> Pipeline.concatEstimator "Features" [| "SepalLength"; "SepalWidth"; "PetalLength"; "PetalWidth" |]
        |> Pipeline.append (SdcaMultiClassTrainer(env, SdcaMultiClassTrainer.Arguments(), "Features", "Label"))
        //4. Create and train the model            
        |> Pipeline.fit trainingDataView

    printfn "=============== End of training ==============="
    printfn ""


    //5. Evaluate the model and show accuracy stats

    //Load evaluation/test data
    let testDataView = new MultiFileSource(TestDataPath) |> reader.Read

    printfn "=============== Evaluating Model's accuracy with Test data==============="
    let predictions = model.Transform testDataView

    let multiClassificationCtx = MulticlassClassificationContext env
    let metrics = multiClassificationCtx.Evaluate(predictions, "Label")

    printfn "Metrics:"
    printfn "     AccuracyMacro = %.4f, a value between 0 and 1, the closer to 1, the better" metrics.AccuracyMacro
    printfn "     AccuracyMicro = %.4f, a value between 0 and 1, the closer to 1, the better" metrics.AccuracyMicro
    printfn "     LogLoss = %.4f, the closer to 0, the better" metrics.LogLoss
    printfn "     LogLoss for class 1 = %.4f, the closer to 0, the better" metrics.PerClassLogLoss.[0]
    printfn "     LogLoss for class 2 = %.4f, the closer to 0, the better" metrics.PerClassLogLoss.[1]
    printfn "     LogLoss for class 3 = %.4f, the closer to 0, the better" metrics.PerClassLogLoss.[2]
    printfn ""

    //6. Test Sentiment Prediction with one sample text 
    let predictionFunct = model.MakePredictionFunction<IrisData, IrisPrediction> env

    let prediction = predictionFunct.Predict TestIrisData.Iris1
    printfn "Actual: setosa.     Predicted probability: setosa:      %.4f"prediction.Score.[0]
    printfn "                                           versicolor:  %.4f"prediction.Score.[1]
    printfn "                                           virginica:   %.4f"prediction.Score.[2]
    printfn ""

    let prediction = predictionFunct.Predict TestIrisData.Iris2
    printfn "Actual: virginica.  Predicted probability: setosa:      %.4f"prediction.Score.[0]
    printfn "                                           versicolor:  %.4f"prediction.Score.[1]
    printfn "                                           virginica:   %.4f"prediction.Score.[2]
    printfn ""

    let prediction = predictionFunct.Predict TestIrisData.Iris3
    printfn "Actual: versicolor. Predicted probability: setosa:      %.4f"prediction.Score.[0]
    printfn "                                           versicolor:  %.4f"prediction.Score.[1]
    printfn "                                           virginica:   %.4f"prediction.Score.[2]
    printfn ""
       
    Console.ReadLine() |> ignore

    0 // return an integer exit code
