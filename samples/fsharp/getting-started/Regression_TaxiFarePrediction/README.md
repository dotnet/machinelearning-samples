# Taxi Fare Prediction
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

![Build -> Train -> Evaluate -> Consume](../../../../../master/samples/csharp/getting-started/shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: uploading data (`taxi-fare-train.csv` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `ColumnCopier`,`CategoricalOneHotVectorizer`,`ColumnConcatenator`), and choosing a learning algorithm (`FastTreeRegressor`). All of those steps are stored in a `LearningPipeline`:
```fsharp
// LearningPipeline holds all steps of the learning process: data, transforms, learners.
let pipeline = LearningPipeline()

// The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
// all the column names and their types. This will be used to create the model, and train it.
pipeline.Add(TextLoader(TrainDataPath).CreateFrom<TaxiTrip>(separator=',')               
// Transforms
// When ML model starts training, it looks for two columns: Label and Features.
// Label:   values that should be predicted. If you have a field named Label in your data type,
//              no extra actions required.
//          If you don’t have it, like in this example, copy the column you want to predict with
//              ColumnCopier transform:
pipeline.Add(ColumnCopier(("FareAmount", "Label")))
                
// CategoricalOneHotVectorizer transforms categorical (string) values into 0/1 vectors
pipeline.Add(CategoricalOneHotVectorizer("VendorId",
                    "RateCode",
                "PaymentType"))

// Features: all data used for prediction. At the end of all transforms you need to concatenate
//              all columns except the one you want to predict into Features column with
//              ColumnConcatenator transform:
pipeline.Add(ColumnConcatenator("Features",
                "VendorId",
                "RateCode",
                "PassengerCount",
                "TripDistance",
                "PaymentType"))

//FastTreeRegressor is an algorithm that will be used to train the model.
pipeline.Add(FastTreeRegressor())
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fare values) to tune the parameters of the model. It is implemented in the `Train()` API. To perform training we just call the method and provide the types for our data object `TaxiTrip` and  prediction object `TaxiTripFarePrediction`.

```fsharp
let model = pipeline.Train<TaxiTrip, TaxiTripFarePrediction>()
```

### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`taxi-fare-test.csv`). This dataset also contains known fares. `RegressionEvaluator` calculates the difference between known fares and values predicted by the model in various metrics.

```fsharp
let testData = TextLoader(TestDataPath).CreateFrom<TaxiTrip>(separator=',')

let evaluator = RegressionEvaluator()
let metrics = evaluator.Evaluate(model, testData)
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size for performance purposes. You can use the original datasets to significantly improve the quality (Original datasets are referenced in datasets [README](../../../datasets/README.md)).*

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the fare amount for specified trip. 

```fsharp
let prediction = model.Predict(TestTaxiTrips.Trip1)
Console.WriteLine(sprintf "Predicted fare: {prediction.FareAmount:0.####}, actual fare: 29.5")
```
Where `TestTaxiTrips.Trip1` stores the information about the trip we'd like to get the prediction for.

```fsharp
module TestTaxiTrips =
    let Trip1 = 
       TaxiTrip(
            VendorId = "VTS",
            RateCode = "1",
            PassengerCount = 1.0,
            TripDistance = 10.33,
            PaymentType = "CSH",
            FareAmount = 0.0 // predict it. actual = 29.5
       )
```
