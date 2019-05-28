# Taxi Fare Prediction - Advanced AutoML Experiment

## Automated Machine Learning

Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model.

While running a default experiment can be achieved simply, many aspects of the experiment can be customized. The following walk through demonstrates those details.

## Problem
This problem is to predict the fare of a taxi trip in New York City. At first glance, the fare may seem to depend simply on the distance traveled. However, taxi vendors in New York charge varying amounts for other factors such as additional passengers, paying with a credit card instead of cash, and so on. This prediction could help taxi providers give passengers and drivers estimates on ride fares.

## Step 1: Infer Columns

Using the `InferColumns` API, auto-infer the name, data type, and purpose of each column in the dataset:

```fsharp
let columnInference = mlContext.Auto().InferColumns(trainDataPath, labelColumnName, groupColumns=false)
```

## Step 2: Load the Data

Load the datasets required to train and test:

```fsharp
let textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions)
let trainDataView = textLoader.Load(trainDataPath)
let testDataView = textLoader.Load(testDataPath)
```

## Step 3: Add a pre-featurizer

Build a pre-featurizer for use in the AutoML experiment. Internally, AutoML uses one or more train/validation data splits to evaluate the models it produces. The pre-featurizer is fit only on the training data split to produce a trained transform. Then, the trained transform is applied to both the train and validation data splits.

```fsharp
let preFeaturizer = mlContext.Transforms.Conversion.MapValue("is_cash", [| KeyValuePair("CSH", true) |], "payment_type")
```

## Step 4: Edit Inferred Column Information
Edit or correct the column information that was inferred:

```fsharp
let columnInformation = columnInference.ColumnInformation
columnInformation.CategoricalColumnNames.Remove("payment_type") |> ignore
columnInformation.IgnoredColumnNames.Add("payment_type")
```

## Step 5: Initialize Experiment Settings

Initialize the AutoML experiment settings:

```fsharp
let experimentSettings = new RegressionExperimentSettings()
experimentSettings.MaxExperimentTimeInSeconds <- 3600u
experimentSettings.CancellationToken <- cts.Token

// Set the metric that AutoML will try to optimize over the course of the experiment.
experimentSettings.OptimizingMetric <- RegressionMetric.RootMeanSquaredError

// Set the cache directory to null.
// This will cause all models produced by AutoML to be kept in memory 
// instead of written to disk after each run, as AutoML is training.
// (Please note: for an experiment on a large dataset, opting to keep all 
// models trained by AutoML in memory could cause your system to run out 
// of memory.)
//experimentSettings.CacheDirectory <- null

// Don't use LbfgsPoissonRegression and OnlineGradientDescent trainers during this experiment.
// (These trainers sometimes underperform on this dataset.)
experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression) |> ignore
experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent) |> ignore
```

## Step 6: Create the AutoML Experiment

Create the AutoML experiment using the initialized experiment settings:

```fsharp
let experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings)
```

## Step 7: Execute the AutoML Experiment

Executing triggers data featurization, learning algorithm selection, hyperparameter tuning, etc. AutoML will iterate over different data featurizations, machine learning algorithms, hyperparamters, etc. until `MaxExperimentTimeInSeconds` is reached or the experiment is terminated.

```fsharp
let experimentResult = experiment.Execute(trainDataView, columnInformation, preFeaturizer, progressHandler)
```

## Step 8: Evaluate Model

Evaluate the quality of the model on a test dataset that was not used in training (`taxi-fare-test.csv`).

```fsharp
let predictions = experimentResult.BestRun.Model.Transform(testDataView)
let metrics = mlContext.Regression.Evaluate(predictions, labelColumnName = labelColumnName, scoreColumnName = "Score")
```

## Step 9: Make Predictions

Using the trained model, use the `Predict()` API to predict the fare amount for specified trip:

```fsharp
// Sample: 
// vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
// VTS,1,1,1140,3.75,CRD,15.5

let taxiTripSample = 
    {
        VendorId = "VTS"
        RateCode = 1.f
        PassengerCount = 1.f
        TripTime = 1140.f
        TripDistance = 3.75f
        PaymentType = "CRD"
        FareAmount = 0.f // To predict. Actual/Observed = 15.5
    }

let trainedModel, modelInputSchema = mlContext.Model.Load(modelPath)

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel)

// Score
let predictedResult = predEngine.Predict(taxiTripSample)

printfn "**********************************************************************"
printfn "Predicted fare: %0.4f, actual fare: 15.5" predictedResult.FareAmount
printfn "**********************************************************************"
```

## Step 10: Re-fit Best Pipeline

Re-fit best pipeline on train and test data, to produce a model that is trained on as much data as is available. This is the final model that can be deployed to production.

```fsharp
ConsoleHelper.consoleWriteHeader "=============== Re-fitting best pipeline ==============="
let refitModel = 
    let textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions)
    MultiFileSource(trainDataPath, testDataPath)
    |> textLoader.Load
    |> experimentResult.BestRun.Estimator.Fit
```
