using AutoMLTrialRunner;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.SearchSpace;
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

var ssaSearchSpace = new SearchSpace<SSAOption>();

var ssaFactory = (MLContext ctx, SSAOption param) =>
{
    return ctx.Forecasting.ForecastBySsa(
        outputColumnName: "prediction",
        inputColumnName: "load",
        windowSize: param.WindowSize,
        seriesLength: param.SeriesLength,
        trainSize: param.TrainSize,
        horizon: param.Horizon);
};

var ssaSweepableEstimator = ctx.Auto().CreateSweepableEstimator(ssaFactory, ssaSearchSpace);

var ssaPipeline =
    new EstimatorChain<ITransformer>()
        .Append(ssaSweepableEstimator);

var ssaRunner = new SSARunner(ctx, trainValidationData, labelColumnName: "load", pipeline: ssaPipeline);

AutoMLExperiment ssaExperiment = ctx.Auto().CreateExperiment();

ssaExperiment
    .SetPipeline(ssaPipeline)
    .SetRegressionMetric(RegressionMetric.RootMeanSquaredError, labelColumn: "load", scoreColumn: "prediction")
    .SetTrainingTimeInSeconds(60)
    .SetDataset(trainValidationData)
    .SetTrialRunner(ssaRunner);

var ssaCts = new CancellationTokenSource();
TrialResult ssaExperimentResults = await ssaExperiment.RunAsync(ssaCts.Token);

var model = ssaExperimentResults.Model;