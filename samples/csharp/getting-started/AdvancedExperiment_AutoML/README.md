# Taxi Fare Prediction - Advanced AutoML Experiment

## Automated Machine Learning
Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model based on training scores.

## Problem
This problem is to predict the fare of a taxi trip in New York City. At first glance, the fare may seem to depend simply on the distance traveled. However, taxi vendors in New York charge varying amounts for other factors such as additional passengers, paying with a credit card instead of cash, and so on. This prediction could help taxi providers give passengers and drivers estimates on ride fares.

## Step 1: Infer Columns

Using the `InferColumns` API, auto-infer the name, data type, and purpose of each column in the dataset:

```C#
ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(TrainDataPath, LabelColumnName, groupColumns: false);
```

## Step 2: Load the Data

Load the datasets required to train and test:

```C#
TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
TrainDataView = textLoader.Load(TrainDataPath);
TestDataView = textLoader.Load(TestDataPath);
TrainSmallDataView = textLoader.Load(TrainDataSmallPath);
```

`TrainSmallDataView` is a subsample of the full training data. In this sample, AutoML will consume `TrainSmallDataView` instead of `TrainDataView`. This will speed up the training of each model AutoML produces, enabling AutoML to search & evaluate more models.

## Step 3: Add a pre-featurizer

Build a pre-featurizer for use in the AutoML experiment. Internally, AutoML uses one or more train/validation data splits to evaluate the models it produces. The pre-featurizer is fit only on the training data split to produce a trained transform. Then, the trained transform is applied to both the train and validation data splits.

```C#
IEstimator<ITransformer> preFeaturizer = mlContext.Transforms.Conversion.MapValue("is_cash",
    new[] { new KeyValuePair<string, bool>("CSH", true) }, "payment_type");
```

## Step 4: Edit Inferred Column Information
Edit or correct the column information that was inferred:

```C#
ColumnInformation columnInformation = columnInference.ColumnInformation;
columnInformation.CategoricalColumnNames.Remove("payment_type");
columnInformation.IgnoredColumnNames.Add("payment_type");
```

## Step 5: Initialize Experiment Settings

Initialize the AutoML experiment settings:

```C#
var experimentSettings = new RegressionExperimentSettings();

experimentSettings.MaxExperimentTimeInSeconds = 3600;
experimentSettings.CancellationToken = cts.Token;

// Set the metric that AutoML will try to optimize over the course of the experiment.
experimentSettings.OptimizingMetric = RegressionMetric.RootMeanSquaredError;

// Set the cache directory to null.
// This will cause all models produced by AutoML to be kept in memory 
// instead of written to disk after each run, as AutoML is training.
experimentSettings.CacheDirectory = null;

// Don't use LbfgsPoissonRegression and OnlineGradientDescent trainers during this experiment.
experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression);
experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent);

// Cancel experiment after the user presses any key
CancelExperimentAfterAnyKeyPress(cts);
```

## Step 6: Create the AutoML Experiment

Create the AutoML experiment using the initialized experiment settings:

```C#
var experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings);
```

## Step 7: Execute the AutoML Experiment

Executing triggers data featurization, learning algorithm selection, hyperparameter tuning, etc. AutoML will iterate over different data featurizations, machine learning algorithms, hyperparamters, etc. until `MaxExperimentTimeInSeconds` is reached or the experiment is terminated.

```C#            
 ExperimentResult<RegressionMetrics> experimentResult = experiment.Execute(TrainSmallDataView, columnInformation, preFeaturizer, progressHandler);
```

## Step 8: Re-fit Best Pipeline
Re-fit the best pipeline (trained from subsample of AutoML data) on entirety of training data. (This step is optional. By no means is it required in your workflow.)

```C#
private static ITransformer RefitBestPipeline(MLContext mlContext, ExperimentResult<RegressionMetrics> experimentResult)
{
	RunDetail<RegressionMetrics> best = experimentResult.BestRun;
	return best.Estimator.Fit(TrainDataView);
}
```

## Step 9: Evaluate Model

Evaluate the quality of the re-fit model on a test dataset that was not used in training (`taxi-fare-test.csv`).

```
IDataView predictions = model.Transform(TestDataView);
var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
```

## Step 10: Make Predictions

Using the trained model, use the `Predict()` API to predict the fare amount for specified trip:

````C#
//Sample: 
//vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
//VTS,1,1,1140,3.75,CRD,15.5

var taxiTripSample = new TaxiTrip()
{
    VendorId = "VTS",
    RateCode = "1",
    PassengerCount = 1,
    TripTime = 1140,
    TripDistance = 3.75f,
    PaymentType = "CRD",
    FareAmount = 0 // To predict. Actual/Observed = 15.5
};

// Create prediction engine
var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(model);

// Score
var predictedResult = predEngine.Predict(taxiTripSample);

Console.WriteLine($"**********************************************************************");
Console.WriteLine($"Predicted fare: {predictedResult.FareAmount:0.####}, actual fare: 15.5");
Console.WriteLine($"**********************************************************************");
```

Finally, you can plot in a chart how the tested predictions are distributed and how the regression is performing with the implemented method `PlotRegressionChart()` as in the following screenshot:

![Regression plot-chart](images/Sample-Regression-Chart.png)