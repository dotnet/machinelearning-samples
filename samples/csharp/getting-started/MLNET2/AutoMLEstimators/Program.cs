// Initialize MLContext
using AutoMLAdvanced;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.AutoML.CodeGen;
using Microsoft.ML.Data;
using Microsoft.ML.SearchSpace;
using Microsoft.ML.SearchSpace.Option;
using Microsoft.ML.Trainers;
using static Microsoft.ML.DataOperationsCatalog;

// Initialize MLContext
MLContext ctx = new MLContext();

var dataPath = Path.GetFullPath(@"..\..\..\..\Data\taxi-fare-train.csv");

// Infer column information
ColumnInferenceResults columnInference =
    ctx.Auto().InferColumns(dataPath, labelColumnName: "fare_amount", groupColumns: false);

// Create text loader
TextLoader loader = ctx.Data.CreateTextLoader(columnInference.TextLoaderOptions);

// Load data into IDataView
IDataView data = loader.Load(dataPath);

// Split into train (80%), validation (20%) sets
TrainTestData trainValidationData = ctx.Data.TrainTestSplit(data, testFraction: 0.2);

// Initialize default Scda search space
var sdcaSearchSpace = new SearchSpace<SdcaOption>();

// Modify L1 search space range
sdcaSearchSpace["L1Regularization"] = new UniformSingleOption(min: 0.01f, max: 2.0f, logBase: false, defaultValue: 0.01f);

// Use the search space to define a custom factory to create an SdcaRegressionTrainer
var sdcaFactory = (MLContext ctx, SdcaOption param) =>
{
    var sdcaOption = new SdcaRegressionTrainer.Options();
    sdcaOption.L1Regularization = param.L1Regularization;
    sdcaOption.L2Regularization = 0.02f;

    return ctx.Regression.Trainers.Sdca(sdcaOption);
};

// Define Sdca sweepable estimator (SdcaRegressionTrainer + SdcaOption search space)
var sdcaSweepableEstimator = ctx.Auto().CreateSweepableEstimator(sdcaFactory, sdcaSearchSpace);

// Add sweepable estimator to sweepable pipeline
SweepablePipeline pipeline =
    ctx.Auto().Featurizer(data, columnInformation: columnInference.ColumnInformation)
        .Append(sdcaSweepableEstimator);

// Create AutoML experiment
AutoMLExperiment experiment = ctx.Auto().CreateExperiment();

// Configure experiment
experiment
    .SetPipeline(pipeline)
    .SetRegressionMetric(RegressionMetric.RSquared, labelColumn: columnInference.ColumnInformation.LabelColumnName)
    .SetTrainingTimeInSeconds(60)
    .SetDataset(trainValidationData);

// Log experiment trials
var monitor = new AutoMLMonitor(pipeline);
experiment.SetMonitor(monitor);

// Run experiment
var cts = new CancellationTokenSource();
TrialResult experimentResults = await experiment.RunAsync(cts.Token);

// Get best model
var model = experimentResults.Model;