using AutoMLTrialRunner;
using AutoMLTrialRunner.AutoMLTrialRunner;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.SearchSpace;
using Microsoft.ML.Recommender;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Trainers;
using System.Reflection.Emit;
using Microsoft.ML.TorchSharp;

// Initialize MLContext
MLContext ctx = new MLContext();

// (Recommended) Use GPU
ctx.GpuDeviceId = 0;
ctx.FallbackToCpu = false;

var dataPath = Path.GetFullPath(@"..\..\..\..\Data\yelp_labelled.txt");

var textColumnName = "col0";

// Infer column information
ColumnInferenceResults columnInference =
    ctx.Auto().InferColumns(dataPath, labelColumnIndex: 1, groupColumns: false);

// Create text loader
TextLoader loader = ctx.Data.CreateTextLoader(columnInference.TextLoaderOptions);

// Load data into IDataView
IDataView data = loader.Load(dataPath);

// Split into train (80%), validation (20%) sets
TrainTestData trainValidationData = ctx.Data.TrainTestSplit(data, testFraction: 0.2);

// Initialize serach space
var tcSearchSpace = new SearchSpace<TCOption>();

// Create factory for Text Classification trainer
var tcFactory = (MLContext ctx, TCOption param) =>
{
    return ctx.MulticlassClassification.Trainers.TextClassification(
        sentence1ColumnName: textColumnName,
        batchSize:param.BatchSize);
};

// Create text classification sweepable estimator
var tcEstimator = 
    ctx.Auto().CreateSweepableEstimator(tcFactory, tcSearchSpace);

// Define text classification pipeline
var pipeline =
    ctx.Transforms.Conversion.MapValueToKey(columnInference.ColumnInformation.LabelColumnName)
        .Append(tcEstimator);

// Initialize custom text classification runner
var tcRunner = new TCRunner(context: ctx, data: trainValidationData, pipeline: pipeline);

// Create AutoML experiment
AutoMLExperiment experiment = ctx.Auto().CreateExperiment();

// Configure AutoML experiment
experiment
    .SetPipeline(pipeline)
    .SetMulticlassClassificationMetric(MulticlassClassificationMetric.MicroAccuracy, labelColumn: columnInference.ColumnInformation.LabelColumnName)
    .SetTrainingTimeInSeconds(120)
    .SetDataset(trainValidationData)
    .SetTrialRunner(tcRunner);

// Log experiment trials
var monitor = new AutoMLMonitor(pipeline);
experiment.SetMonitor(monitor);

// Run experiment
var tcCts = new CancellationTokenSource();
TrialResult textClassificationExperimentResults = await experiment.RunAsync(tcCts.Token);

// Get model
var model = textClassificationExperimentResults.Model;