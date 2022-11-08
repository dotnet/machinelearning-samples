// Initialize MLContext
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

// Initialize MLContext
MLContext ctx = new MLContext();

// Infer column information
ColumnInferenceResults columnInference =
    ctx.Auto().InferColumns("taxi-fare.csv", labelColumnName: "fare_amount", groupColumns: false);

// Create text loader
TextLoader loader = ctx.Data.CreateTextLoader(columnInference.TextLoaderOptions);

// Load data into IDataView
IDataView data = loader.Load("taxi-fare.csv");

// Split into train (80%), validation (20%) sets
TrainTestData trainValidationData = ctx.Data.TrainTestSplit(data, testFraction: 0.2);

//Define pipeline
SweepablePipeline pipeline =
    ctx.Auto().Featurizer(data, columnInformation: columnInference.ColumnInformation)
        .Append(ctx.Auto().Regression(labelColumnName: columnInference.ColumnInformation.LabelColumnName));

// Create AutoML experiment
AutoMLExperiment experiment = ctx.Auto().CreateExperiment();

// Configure experiment
experiment
    .SetPipeline(pipeline)
    .SetRegressionMetric(RegressionMetric.RSquared, labelColumn: columnInference.ColumnInformation.LabelColumnName)
    .SetTrainingTimeInSeconds(60)
    .SetDataset(trainValidationData);

// Log experiment trials
ctx.Log += (_, e) => {
    if (e.Source.Equals("AutoMLExperiment"))
    {
        Console.WriteLine(e.RawMessage);
    }
};

// Run experiment
TrialResult experimentResults = await experiment.RunAsync();

// Get best model
var model = experimentResults.Model;