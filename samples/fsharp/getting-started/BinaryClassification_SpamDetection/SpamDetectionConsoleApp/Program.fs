open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open System.Net
open System.IO.Compression
open Microsoft.ML.Core.Data
open Microsoft.ML.Transforms.Conversions

[<CLIMutable>]
type SpamInput = 
    {
        LabelText : string
        Message : string
    }

[<CLIMutable>]
type SpamPrediction = 
    {
        PredictedLabel : bool
        Score : float32
        Probability : float32
    }

let downcastPipeline (x : IEstimator<_>) = 
    match x with 
    | :? IEstimator<ITransformer> as y -> y
    | _ -> failwith "downcastPipeline: expecting a IEstimator<ITransformer>"

let classify (p : PredictionEngine<_,_>) x = 
    let prediction = p.Predict({LabelText = ""; Message = x})
    printfn "The message '%s' is %s" x (if prediction.PredictedLabel then "spam" else "not spam")

[<EntryPoint>]
let main _argv =
    let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])
    let dataDirectoryPath = Path.Combine(appPath,"../../../","Data","spamfolder")
    let trainDataPath  = Path.Combine(appPath,"../../../","Data","spamfolder","SMSSpamCollection")

    // Download the dataset if it doesn't exist.
    if not (File.Exists trainDataPath) then 
        printfn "%A" (File.Exists trainDataPath)
        use wc = new WebClient()
        wc.DownloadFile("https://archive.ics.uci.edu/ml/machine-learning-databases/00228/smsspamcollection.zip", "spam.zip")
        ZipFile.ExtractToDirectory("spam.zip", dataDirectoryPath)

    // Set up the MLContext, which is a catalog of components in ML.NET.
    let mlContext = MLContext(seed = Nullable 1)
    
    let data = 
        mlContext.Data.ReadFromTextFile(trainDataPath,
            columns = 
                [|
                    TextLoader.Column("LabelText" , Nullable DataKind.Text, 0)
                    TextLoader.Column("Message" , Nullable DataKind.Text, 1)
                |],
            hasHeader = false,
            separatorChar = '\t')
    
    // Create the estimator which converts the text label to a bool then featurizes the text, and add a linear trainer.
    let estimator = 
        EstimatorChain()
            .Append(mlContext.Transforms.Conversion.ValueMap(["ham"; "spam"], [false; true],[| struct ("Label", "LabelText") |]))
            .Append(mlContext.Transforms.Text.FeaturizeText("Features", "Message"))
            .AppendCacheCheckpoint(mlContext)
            .Append(mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent("Label", "Features"))
        
    // Evaluate the model using cross-validation.
    // Cross-validation splits our dataset into 'folds', trains a model on some folds and 
    // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
    // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
    let cvResults = mlContext.BinaryClassification.CrossValidate(data, downcastPipeline estimator, numFolds = 5);
    let avgAuc = cvResults |> Seq.map (fun struct (metrics,_,_) -> metrics.Auc) |> Seq.average
    printfn "The AUC is %f" avgAuc
    
    // Now let's train a model on the full dataset to help us get better results
    let model = estimator.Fit(data)

    // The dataset we have is skewed, as there are many more non-spam messages than spam messages.
    // While our model is relatively good at detecting the difference, this skewness leads it to always
    // say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
    // it is useful to look at the precision-recall curve to identify the best possible threshold.
    let newModel = 
        let lastTransformer = 
            BinaryPredictionTransformer<IPredictorProducing<float32>>(
                mlContext, 
                model.LastTransformer.Model, 
                model.GetOutputSchema(data.Schema), 
                model.LastTransformer.FeatureColumn, 
                threshold = 0.15f, 
                thresholdColumn = DefaultColumnNames.Probability);
        let parts = model |> Seq.toArray
        parts.[parts.Length - 1] <- lastTransformer :> _
        TransformerChain<ITransformer>(parts)


    // Create a PredictionFunction from our model 
    let predictor = newModel.CreatePredictionEngine<SpamInput, SpamPrediction>(mlContext);

    // Test a few examples
    [
        "That's a great idea. It should work."
        "free medicine winner! congratulations"
        "Yes we should meet over the weekend!"
        "you win pills and free entry vouchers"
    ] 
    |> List.iter (classify predictor)

    Console.ReadLine() |> ignore
    0


