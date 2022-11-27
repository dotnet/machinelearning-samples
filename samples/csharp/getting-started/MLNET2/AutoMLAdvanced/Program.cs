// Initialize MLContext
using AutoMLAdvanced;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

// Initialize MLContext
MLContext ctx = new MLContext();

var dataPath = Path.GetFullPath(@"..\..\..\..\Data\taxi-fare-train.csv");

// Infer column information
ColumnInferenceResults columnInference =
    ctx.Auto().InferColumns(dataPath, labelColumnName: "fare_amount", groupColumns: false);

// Modify column inference results
columnInference.ColumnInformation.NumericColumnNames.Remove("rate_code");
columnInference.ColumnInformation.CategoricalColumnNames.Add("rate_code");

// Create text loader
TextLoader loader = ctx.Data.CreateTextLoader(columnInference.TextLoaderOptions);

// Load data into IDataView
IDataView data = loader.Load(dataPath);

// Split into train (80%), validation (20%) sets
TrainTestData trainValidationData = ctx.Data.TrainTestSplit(data, testFraction: 0.2);

//Define pipeline
SweepablePipeline pipeline =
    ctx.Auto().Featurizer(data, columnInformation: columnInference.ColumnInformation)
        .Append(ctx.Auto().Regression(labelColumnName: columnInference.ColumnInformation.LabelColumnName, useLgbm:false));

// Create AutoML experiment
AutoMLExperiment experiment = ctx.Auto().CreateExperiment();

// Configure experiment
experiment
    .SetPipeline(pipeline)
    .SetRegressionMetric(RegressionMetric.RSquared, labelColumn: columnInference.ColumnInformation.LabelColumnName)
    .SetTrainingTimeInSeconds(60)
    .SetGridSearchTuner()
    .SetDataset(trainValidationData);

// Log experiment trials
var monitor = new AutoMLMonitor(pipeline);
experiment.SetMonitor(monitor);

// Set checkpoints
var checkpointPath = Path.Join(Directory.GetCurrentDirectory(), "automl");
experiment.SetCheckpoint(checkpointPath);

// Run experiment
var cts = new CancellationTokenSource();
TrialResult experimentResults = await experiment.RunAsync(cts.Token);

// Get best model
var model = experimentResults.Model;

// Get all completed trials
var completedTrials = monitor.GetCompletedTrials();