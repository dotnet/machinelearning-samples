open System

open Microsoft.ML.Runtime.Learners
open Microsoft.ML.Runtime.Data
open Microsoft.ML
open System.IO


/// Type representing the text to run sentiment analysis on.
[<CLIMutable>] 
type SentimentIssue = 
    { 
        Text : string 
    }

/// Result of sentiment prediction.
[<CLIMutable>]
type  SentimentPrediction = 
    { 
        // Predicted sentiment: 0 - negative, 1 - positive
        PredictedLabel : bool; 
        Probability : float32; 
        Score : float32 
    }


module Pipeline =
    open Microsoft.ML.Core.Data

    let textTransform (inputColumn : string) outputColumn env =
        TextTransform(env, inputColumn, outputColumn)

    let append (estimator : IEstimator<'b>) (pipeline : IEstimator<ITransformer>)  = 
        pipeline.Append estimator
        
    let fit (dataView : IDataView) (pipeline : EstimatorChain<'a>) =
        pipeline.Fit dataView


let AppPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])
let TrainDataPath = Path.Combine(AppPath, "datasets", "wikipedia-detox-250-line-data.tsv")
let TestDataPath = Path.Combine(AppPath, "datasets", "wikipedia-detox-250-line-test.tsv")
let modelPath = Path.Combine(AppPath, "SentimentModel.zip")


let printPrediction statement resultprediction =
    printfn 
        "Text: %s | Prediction: %s sentiment | Probability: %f"
        statement
        (if resultprediction.PredictedLabel then "Toxic" else "Nice")
        resultprediction.Probability


let saveModelAsFile env (model : TransformerChain<'a>)=
    use fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
    model.SaveTo(env, fs)

    printfn "The model is saved to %s" modelPath


let predictWithModelLoadedFromFile (sampleStatement : SentimentIssue) =
    // Test with Loaded Model from .zip file
    use env = new LocalEnvironment()
    use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
    let loadedModel = TransformerChain.LoadFrom(env, stream)

    // Create prediction engine and make prediction.
    let engine = loadedModel.MakePredictionFunction<SentimentIssue, SentimentPrediction> env

    let predictionFromLoaded = engine.Predict sampleStatement
       
    printfn ""
    printfn "=============== Test of model with a sample ==============="
    printPrediction sampleStatement.Text predictionFromLoaded


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
                        TextLoader.Column("Label", Nullable DataKind.Bool, 0)
                        TextLoader.Column("Text", Nullable DataKind.Text, 1)
                    |]
                )
            )

    //Load training data
    let trainingDataView = MultiFileSource(TrainDataPath) |> reader.Read

    printfn "=============== Create and Train the Model ==============="

    let model = 
        env
        //3.Create a flexible pipeline (composed by a chain of estimators) for creating/traing the model.
        |> Pipeline.textTransform "Text" "Features"
        |> Pipeline.append (LinearClassificationTrainer(env, LinearClassificationTrainer.Arguments(), "Features", "Label"))
        //4. Create and train the model            
        |> Pipeline.fit trainingDataView

    printfn "=============== End of training ==============="
    printfn ""

    //5. Evaluate the model and show accuracy stats
    let testDataView = MultiFileSource(TestDataPath) |> reader.Read

    printfn "=============== Evaluating Model's accuracy with Test data==============="
    let predictions = model.Transform testDataView
    let binClassificationCtx = env |> BinaryClassificationContext
    let metrics = binClassificationCtx.Evaluate(predictions, "Label")

    printfn ""
    printfn "Model quality metrics evaluation"
    printfn "------------------------------------------"
    printfn "Accuracy: %.2f%%" (metrics.Accuracy * 100.)
    printfn "Auc: %.2f%%" (metrics.Auc * 100.)
    printfn "F1Score: %.2f%%" (metrics.F1Score * 100.)
    printfn "=============== End of Model's evaluation ==============="
    printfn ""

    //6. Test Sentiment Prediction with one sample text 
    let predictionFunct = model.MakePredictionFunction<SentimentIssue, SentimentPrediction> env
    let sampleStatement = { Text = "This is a very rude movie" }
    let resultprediction = predictionFunct.Predict sampleStatement

    printfn ""
    printfn "=============== Test of model with a sample ==============="
    printPrediction sampleStatement.Text resultprediction


    // Save model to .ZIP file

    saveModelAsFile env model



    // Predict again but now testing the model loading from the .ZIP file

    predictWithModelLoadedFromFile sampleStatement

    printfn "=============== End of process, hit any key to finish ==============="

    Console.ReadLine() |> ignore
    0 // return an integer exit code
