# Taxi Fare Prediction

| AutoML version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.30.0-preview           | Dynamic API | Up-to-date | Console app | .csv files | Price prediction | Regression | Sdca Regression |

## Automated ML
Automated ML eliminates the task of selecting different algorithms and hyperparameters. With automated ML, you just bring in your dataset and specify a few parameters. Automated ML will do the rest i.e. data preprocessing, learning algorithm selection and hyperparameter selection to generate a high quality machine learning model that you can use for predictions.

In this introductory sample, you'll see how to use AutoML to predict taxi fares. In the world of machine learning, this type of prediction is known as **regression**.

## Problem
This problem is centered around predicting the fare of a taxi trip in New York City. At first glance, it may seem to depend simply on the distance traveled. However, taxi vendors in New York charge varying amounts for other factors such as additional passengers, paying with a credit card instead of cash and so on. This prediction can be used in application for taxi providers to give users and drivers an estimate on ride fares.

To solve this problem, we will build an ML model that takes as inputs: 
* vendor ID
* rate code
* passenger count
* trip time
* trip distance
* payment type

and predicts the fare of the ride.

## ML task - Regression
The generalized problem of **regression** is to predict some continuous value for given parameters, for example:
* predict a house prise based on number of rooms, location, year built, etc.
* predict a car fuel consumption based on fuel type and car parameters.
* predict a time estimate for fixing an issue based on issue attributes.

The common feature for all those examples is that the parameter we want to predict can take any numeric value in certain range. In other words, this value is represented by `integer` or `float`/`double`, not by `enum` or `boolean` types.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict taxi fares.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

The general steps in building a model using AutoML are

1) Create necessary global variable to define the experiment

2) Define the data's schema mapped to the datasets to load the test and train data into IDataView's

3) Create a Machine Learning Experiment (currently Binary Classification, Multiclass Classification or Regression) by configuring set of parameters

4) Execute the experiment (Generates several models using the configuration settings you specified in Step 2)

5) Fetch the best model

6) Test and Deploy

### Step 1: Define Experiment Variables

Before the main method, create the global variable
```C#
private static uint ExperimentTime = 60;
```

### Step 2: Data loading

```C#
IDataView trainingDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(TrainDataPath, hasHeader: true, separatorChar: ',');
IDataView testDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(TestDataPath, hasHeader: true, separatorChar: ',');
```
### Step 3: Building a Machine Learning Model using AutoML

Create an AutoML experiment by specifying experiment settings. We have already determined this sentiment analysis problem to be a Binary Classification problem. Next, we should specify how long the experiment should run and set a progress handler that will receive notifications as and when new models are trained.

```C#
// Progress handler be will invoked after each model it produces and evaluates.
var progressHandler = new RegressionExperimentProgressHandler();

// Run AutoML binary classification experiment
ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
    .CreateRegressionExperiment(ExperimentTime)
    .Execute(trainingDataView, LabelColumnName, progressHandler: progressHandler);

```

### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`taxi-fare-test.csv`). This dataset also contains known fares. `Regression.Evaluate()` calculates the difference between known fares and values predicted by the model in various metrics.

```CSharp
RunDetail<RegressionMetrics> best = experimentResult.BestRun;
ITransformer trainedModel = best.Model;

IDataView predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");

// Print metrics from top model
ConsoleHelper.PrintRegressionMetrics(best.TrainerName, metrics);
```

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the fare amount for specified trip. 

```CSharp
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

ITransformer trainedModel;
using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
{
    trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
}

// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

//Score
var resultprediction = predEngine.Predict(taxiTripSample);

Console.WriteLine($"**********************************************************************");
Console.WriteLine($"Predicted fare: {resultprediction.FareAmount:0.####}, actual fare: 15.5");
Console.WriteLine($"**********************************************************************");

```

Finally, you can plot in a chart how the tested predictions are distributed and how the regression is performing with the implemented method `PlotRegressionChart()` as in the following screenshot:

![Regression plot-chart](images/Sample-Regression-Chart.png)
