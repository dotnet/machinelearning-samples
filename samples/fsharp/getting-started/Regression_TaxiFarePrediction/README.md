# Taxi Fare Prediction (F#)

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7           | README.md needs update | Up-to date | Console app | .csv files | Price prediction | Regression | Sdca Regression |


In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict taxi fares. In the world of machine learning, this type of prediction is known as **regression**.

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

### 1. Build model

Building a model includes: uploading data (`taxi-fare-train.csv` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (`FastTreeRegressor` in this case):

```fsharp
// LearningPipeline holds all steps of the learning process: data, transforms, learners.
let mlcontext = new LocalEnvironment()

// Create the TextLoader by defining the data columns and where to find (column position) them in the text file.
TextLoader(
	mlcontext, 
	TextLoader.Arguments(
		Separator = ",", 
		HasHeader = true, 
		Column = 
			[|
				TextLoader.Column("VendorId", Nullable DataKind.Text, 0)
				TextLoader.Column("RateCode", Nullable DataKind.Text, 1)
				TextLoader.Column("PassengerCount", Nullable DataKind.R4, 2)
				TextLoader.Column("TripTime", Nullable DataKind.R4, 3)
				TextLoader.Column("TripDistance", Nullable DataKind.R4, 4)
				TextLoader.Column("PaymentType", Nullable DataKind.Text, 5)
				TextLoader.Column("FareAmount", Nullable DataKind.R4, 6)
			|]
		)
	)

// Now read the file (remember though, readers are lazy, so the actual reading will happen when 'fitting').
let dataView = MultiFileSource(TrainDataPath) |> textLoader.Read

//Copy the Count column to the Label column 
let pipeline = 
    CopyColumnsEstimator(mlcontext, "FareAmount", "Label")
    |> Pipeline.append(new CategoricalEstimator(mlcontext, "VendorId"))
    |> Pipeline.append(new CategoricalEstimator(mlcontext, "RateCode"))
    |> Pipeline.append(new CategoricalEstimator(mlcontext, "PaymentType"))
    |> Pipeline.append(new Normalizer(mlcontext, "PassengerCount", Normalizer.NormalizerMode.MeanVariance))
    |> Pipeline.append(new Normalizer(mlcontext, "TripTime", Normalizer.NormalizerMode.MeanVariance))
    |> Pipeline.append(new Normalizer(mlcontext, "TripDistance", Normalizer.NormalizerMode.MeanVariance))
    |> Pipeline.append(new ConcatEstimator(mlcontext, "Features", "VendorId", "RateCode", "PassengerCount", "TripTime", "TripDistance", "PaymentType"))

// We apply our selected Trainer (SDCA Regression algorithm)
let pipelineWithTrainer = 
    pipeline
    |> Pipeline.append(new SdcaRegressionTrainer(mlcontext, new SdcaRegressionTrainer.Arguments(), "Features", "Label"))
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fare values) to tune the parameters of the model. It is implemented in the `Fit()` API. To perform training we just call the method while providing the DataView.

```fsharp
let model = pipelineWithTrainer.Fit dataView
```

### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`taxi-fare-test.csv`). This dataset also contains known fares. `RegressionEvaluator` calculates the difference between known fares and values predicted by the model in various metrics.

```fsharp
let testDataView = MultiFileSource testDataLocation |> textLoader.Read

printfn "=============== Evaluating Model's accuracy with Test data==============="

let predictions = model.Transform testDataView 

let regressionCtx = RegressionContext mlcontext
let metrics = regressionCtx.Evaluate(predictions, "Label", "Score")
let algorithmName = "SdcaRegressionTrainer"
printfn "*************************************************"
printfn "*       Metrics for %s" algorithmName
printfn "*------------------------------------------------"
printfn "*       R2 Score: %.2f" metrics.RSquared
printfn "*       RMS loss: %.2f" metrics.Rms
printfn "*       Absolute loss: %.2f" metrics.L1
printfn "*       Squared loss: %.2f" metrics.L2
printfn "*************************************************"
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size for performance purposes. You can use the original datasets to significantly improve the quality (Original datasets are referenced in datasets [README](../../../datasets/README.md)).*

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the fare amount for specified trip. 

```fsharp
//Prediction test
// Create prediction engine and make prediction.
let engine = model.MakePredictionFunction<TaxiTrip, TaxiTripFarePrediction> mlcontext

//Sample: 
//vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
//VTS,1,1,1140,3.75,CRD,15.5
let taxiTripSample = {
        VendorId = "VTS"
        RateCode = "1"
        PassengerCount = 1.0f
        TripTime = 1140.0f
        TripDistance = 3.75f
        PaymentType = "CRD"
        FareAmount = 0.0f // To predict. Actual/Observed = 15.5
    }

let prediction = engine.Predict taxiTripSample
printfn "**********************************************************************"
printfn "Predicted fare: %.4f, actual fare: 29.5" prediction.FareAmount
printfn "**********************************************************************"
```


Finally, you can plot in a chart how the tested predictions are distributed and how the regression is performing with the implemented method `PlotRegressionChart()` as in the following screenshot:


![Regression plot-chart](images/Sample-Regression-Chart.png)
