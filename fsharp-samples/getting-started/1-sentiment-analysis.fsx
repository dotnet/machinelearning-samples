(* 1. Setup *)
#load "load.fsx"

(* 2. Schema *)

// Open some namespaces
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Models
open Microsoft.ML.Runtime.Api
open Microsoft.ML.Trainers
open Microsoft.ML.Transforms

// Create types for our input and predictions. ML .NET currently does not support F# Records, so we
// have to fall back to mutable classes.
type SentimentData() =
    [<Column(ordinal = "0"); DefaultValue>] val mutable SentimentText : string
    [<Column(ordinal = "1", name = "Label"); DefaultValue>] val mutable Sentiment : float32

type SentimentPrediction() =
    [<ColumnName "PredictedLabel"; DefaultValue>] val mutable Sentiment : bool

(* 3. Training *)

// Create our pipeline to train off the IMDB dataset
let pipeline = LearningPipeline()
pipeline.Add(TextLoader(Datasets.Imdb).CreateFrom<SentimentData>())
pipeline.Add(TextFeaturizer("Features", "SentimentText"))
pipeline.Add(FastTreeBinaryClassifier(NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2))

// Train the model
let model = pipeline.Train<SentimentData, SentimentPrediction>()

(* 4. Predictions *)

// Predict any text by calling the predict function defined below.
let predict text = model.Predict [ SentimentData(SentimentText = text) ] |> Seq.map(fun r -> text, r.Sentiment) |> Seq.head

predict "Contoso's 11 is a wonderful experience"
predict "Sort of ok"
predict "Joe versus the Volcano Coffee Company is a great film."

(* 5. Testing *)

// First load in some test data
let testData = TextLoader(Datasets.Yelp).CreateFrom<SentimentData>()

// Now run the binary classification evaluator over the model and test data
let metrics = BinaryClassificationEvaluator().Evaluate(model, testData)

// Evaluate this line to view the metrics
metrics