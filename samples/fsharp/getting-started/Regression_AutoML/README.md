# Taxi Fare Prediction

## Automated Machine Learning
Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model.

## Problem
This problem is to predict the fare of a taxi trip in New York City. At first glance, the fare may seem to depend simply on the distance traveled. However, taxi vendors in New York charge varying amounts for other factors such as additional passengers, paying with a credit card instead of cash, and so on. This prediction could help taxi providers give passengers and drivers estimates on ride fares.

## ML Task - Regression
The generalized problem of **regression** is to predict some continuous value for given parameters, for example:
* predict a house prise based on number of rooms, location, year built, etc.
* predict a car fuel consumption based on fuel type and car parameters.
* predict a time estimate for fixing an issue based on issue attributes.

For all these examples, the parameter we want to predict can take any numeric value in a certain range. In other words, this value is represented by `integer` or `float`/ `double`, not by `enum` or `boolean` types.

## Step 1: Load the Data

Load the datasets required to train and test:

```fsharp
let trainingDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(trainDataPath, hasHeader = true, separatorChar = ',')
let testDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(testDataPath, hasHeader = true, separatorChar = ',')
```

## Step 2: Building a Machine Learning Model using AutoML

Instantiate and run an AutoML experiment. In doing so, specify how long the experiment should run in seconds (`ExperimentTime`), and set a progress handler that will receive notifications after AutoML trains & evaluates each new model.

```fsharp
// Run AutoML binary classification experiment
let experimentResult = mlContext.Auto().CreateRegressionExperiment(experimentTimeInSeconds).Execute(trainingDataView, labelColumnName, progressHandler= progressHandler)
```

## Step 3: Evaluate Model

Grab the best model produced by the AutoML experiment

```fsharp
let model = experimentResult.BestRun.Model
```

and evaluate its quality on a test dataset that was not used in training (`taxi-fare-test.csv`).

`Regression.Evaluate()` calculates the difference between known fares and values predicted by the model to produce various metrics.

```C#
let predictions = trainedModel.Transform(testDataView)
let metrics = mlContext.Regression.Evaluate(predictions, scoreColumnName = "Score")
```

## Step 4: Make Predictions

Using the trained model, use the `Predict()` API to predict the fare amount for specified trip:

```fsharp
//Sample: 
//vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
//VTS,1,1,1140,3.75,CRD,15.5
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

let loadedTrainedModel, modelInputSchema = mlContext.Model.Load(modelPath)

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(loadedTrainedModel)

// Score
let predictedResult = predEngine.Predict(taxiTripSample)

printfn "**********************************************************************"
printfn "Predicted fare: %0.4f, actual fare: 15.5" predictedResult.FareAmount
printfn "**********************************************************************"
```

Finally, you can plot in a chart how the tested predictions are distributed and how the regression is performing with the implemented function `plotRegressionChart` as in the following screenshot:

![Regression plot-chart](images/Sample-Regression-Chart.png)
