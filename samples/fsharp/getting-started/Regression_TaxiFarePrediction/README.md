# Taxi Fare Prediction

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1 | Dynamic API | Up-to-date | Console app | .csv files | Price prediction | Regression | Sdca Regression |

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

### 1. Build model's pipeline

Building a model includes: uploading data (`taxi-fare-train.csv` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (`StochasticDualCoordinateAscent` in this case):

```fsharp
    // STEP 1: Common data loading configuration
    let baseTrainingDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(trainDataPath, hasHeader = true, separatorChar = ',')
    let testDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(testDataPath, hasHeader = true, separatorChar = ',')

    //Sample code of removing extreme data like "outliers" for FareAmounts higher than $150 and lower than $1 which can be error-data 
    //let cnt = baseTrainingDataView.GetColumn<decimal>(mlContext, "FareAmount").Count()
    let trainingDataView = mlContext.Data.FilterRowsByColumn(baseTrainingDataView, "FareAmount", lowerBound = 1., upperBound = 150.)
    //let cnt2 = trainingDataView.GetColumn<float>(mlContext, "FareAmount").Count()

    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline =
        EstimatorChain()
            .Append(mlContext.Transforms.CopyColumns("Label", "FareAmount"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorIdEncoded", "VendorId"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCodeEncoded", "RateCode"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("PaymentTypeEncoded", "PaymentType"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("PassengerCount", "PassengerCount"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("TripTime", "TripTime"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("TripDistance", "TripDistance"))
            .Append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PaymentTypeEncoded", "PassengerCount", "TripTime", "TripDistance"))
            .AppendCacheCheckpoint(mlContext)
            |> downcastPipeline

    // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<TaxiTrip> mlContext trainingDataView dataProcessPipeline 5 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 5 |> ignore

    // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
    let trainer = mlContext.Regression.Trainers.Sdca(labelColumnName = "Label", featureColumnName = "Features")

    let modelBuilder = dataProcessPipeline.Append trainer
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fare values) to tune the parameters of the model. It is implemented in the `Fit()` API. To perform training we just call the method while providing the DataView.

```fsharp
    let trainedModel = modelBuilder.Fit trainingDataView
```

### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`taxi-fare-test.csv`). This dataset also contains known fares. `Regression.Evaluate()` calculates the difference between known fares and values predicted by the model in various metrics.

```fsharp
    let metrics = 
        let predictions = trainedModel.Transform testDataView
        mlContext.Regression.Evaluate(predictions, "Label", "Score")

    Common.ConsoleHelper.printRegressionMetrics (trainer.ToString()) metrics
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size for performance purposes. You can use the original datasets to significantly improve the quality (Original datasets are referenced in datasets [README](../../../datasets/README.md)).*

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the fare amount for specified trip. 

```fsharp
    //Sample: 
    //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
    //VTS,1,1,1140,3.75,CRD,15.5
    let taxiTripSample = 
        {
            VendorId = "VTS"
            RateCode = "1"
            PassengerCount = 1.0f
            TripTime = 1140.0f
            TripDistance = 3.75f
            PaymentType = "CRD"
            FareAmount = 0.0f // To predict. Actual/Observed = 15.5
        };

    let resultprediction = 
        let model, inputSchema = 
            use s = File.OpenRead(modelPath)
            mlContext.Model.Load(s)
        let predictionFunction = mlContext.Model.CreatePredictionEngine(model)
        predictionFunction.Predict taxiTripSample

    printfn "=============== Single Prediction  ==============="
    printfn "Predicted fare: %.4f, actual fare: 15.5" resultprediction.FareAmount
    printfn "=================================================="
```

Finally, you can plot in a chart how the tested predictions are distributed and how the regression is performing with the implemented method `PlotRegressionChart()` as in the following screenshot:

![Regression plot-chart](images/Sample-Regression-Chart.png)
