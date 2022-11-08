using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TorchSharp;
using MathNet.Numerics.Statistics;
using Microsoft.ML.Transforms;

// Initialize MLContext
var ctx = new MLContext();

// (Optional) Use GPU
ctx.GpuDeviceId = 0;
ctx.FallbackToCpu = false;

// Log training output
ctx.Log += (o, e) => {
    if (e.Source.Contains("NasBertTrainer"))
        Console.WriteLine(e.Message);
};

// Load data into IDataView
var columns = new[]
{
    new TextLoader.Column("search_term",DataKind.String,3),
    new TextLoader.Column("relevance",DataKind.Single,4),
    new TextLoader.Column("product_description",DataKind.String,5)
};

var loaderOptions = new TextLoader.Options()
{
    Columns = columns,
    HasHeader = true,
    Separators = new[] { ',' },
    MaxRows = 1000 // Dataset has 75k rows. Only load 1k for quicker training
};

var textLoader = ctx.Data.CreateTextLoader(loaderOptions);
var data = textLoader.Load(@"C:\Datasets\home-depot-sentence-similarity.csv");

// Split data into 80% training, 20% testing
var dataSplit = ctx.Data.TrainTestSplit(data, testFraction: 0.2);

// Define pipeline
var pipeline =
    ctx.Transforms.ReplaceMissingValues("relevance", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean)
    .Append(ctx.Regression.Trainers.SentenceSimilarity(labelColumnName: "relevance", sentence1ColumnName: "search_term", sentence2ColumnName: "product_description"));

// Train the model
var model = pipeline.Fit(dataSplit.TrainSet);

// Use the model to make predictions on the test dataset
var predictions = model.Transform(dataSplit.TestSet);

// Evaluate the model
Evaluate(predictions, "relevance", "Score");

// Save the model
ctx.Model.Save(model, data.Schema, "model.zip");

void Evaluate(IDataView predictions, string actualColumnName, string predictedColumnName)
{
    var actual =
        predictions.GetColumn<float>(actualColumnName)
            .Select(x => (double)x);
    var predicted =
        predictions.GetColumn<float>(predictedColumnName)
            .Select(x => (double)x);
    var corr = Correlation.Pearson(actual, predicted);
    Console.WriteLine($"Pearson Correlation: {corr}");
}