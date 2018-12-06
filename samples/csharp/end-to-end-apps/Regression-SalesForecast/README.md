# eShopDashboardML - Sales forecasting 

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7           | Dynamic API | Update-to-date | ASP.NET Core web app and Console app | SQL Server and .csv files | Sales forecast | Regression | FastTreeTweedie Regression |


eShopDashboardML is a web app with Sales Forecast predictions (per product and per country) using [Microsoft Machine Learning .NET (ML.NET)](https://github.com/dotnet/machinelearning).


# Overview

This end-to-end sample app highlights the usage of ML.NET API by showing the following topics:

1. How to train, build and generate ML models 
   - Implemented as a [console app](src\eShopForecastModelsTrainer) using .NET Core.
2. How to predict the next month of Sales Forecasts by using the trained ML model 
   - Implemented as a single, monolithic [web app](src\eShopDashboard) using [ASP.NET Core Razor](https://docs.microsoft.com/aspnet/core/tutorials/razor-pages/). 

The app is also using a SQL Server database for regular product catalog and orders info, as many typical web apps using SQL Server. In this case, since it is an example, it is, by default, using a localdb SQL database so there's no need to setup a real SQL Server. The localdb database will be created, along with sample populated data, the first time you run the web app.

If you want to use a real SQL Server or Azure SQL Database, you just need to change the connection string in the app.

Here's a sample screenshot of the web app and one of the forecast predictions:

![image](./docs/images/eShopDashboard.png)

## Walkthroughs on how to set it up

Learn how to set it up in Visual Studio plus further explanations on the code:

- [Setting up eShopDashboard in Visual Studio and running the web app](docs/Setting-up-eShopDashboard-in-Visual-Studio-and-running-it.md)

- [Create and Train your ML models](docs/Create-and-train-the-models-%5BOptional%5D.md)
  - This step is optional as the web app is already configured to use a pre-trained model. But you can create your own trained model and swap the pre-trained model with your own.

## Walkthrough on the implemented ML.NET code

### Problem

This problem is centered around country and product forecasting based on previous sales

### DataSet

To solve this problem, you build two independent ML models that take the following datasets as input:  

| Data Set | columns |
|----------|--------|
| **products stats**  | next, productId, year, month, units, avg, count, max, min, prev      |
| **country stats**  | next, country, year, month, max, min, std, count, sales, med, prev   |

### ML task - [Regression](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#regression)

The ML Task for this sample is a Regression, which is a supervised machine learning task that is used to predict the value of the next period (in this case the sales prediction) from a set of related features/variables.

### Solution

To solve this problem, first we will build the ML models while training each model on existing data, evaluate how good it is, and finally you consume the model to predict sales.

Note that the sample implements two independent models:
- Model to predict product's demand forecast for the next period (month)
- Model to predict country's sales forecast for the next period (month)

However, when learning/researching the sample, you can focus just on one of the scenarios/models.

![Build -> Train -> Evaluate -> Consume](docs/images/modelpipeline.png)

#### 1. Build Model

The first step you need to implement is to define the data columns to be loaded from the dataset files, like in the following code:

[Model build and train](./src/eShopForecastModelsTrainer/ProductModelHelper.cs)

```csharp
var textLoader = mlContext.Data.TextReader(new TextLoader.Arguments
                        {
                            Column = new[] {
                                new TextLoader.Column("next", DataKind.R4, 0 ),
                                new TextLoader.Column("productId", DataKind.Text, 1 ),
                                new TextLoader.Column("year", DataKind.R4, 2 ),
                                new TextLoader.Column("month", DataKind.R4, 3 ),
                                new TextLoader.Column("units", DataKind.R4, 4 ),
                                new TextLoader.Column("avg", DataKind.R4, 5 ),
                                new TextLoader.Column("count", DataKind.R4, 6 ),
                                new TextLoader.Column("max", DataKind.R4, 7 ),
                                new TextLoader.Column("min", DataKind.R4, 8 ),
                                new TextLoader.Column("prev", DataKind.R4, 9 )
                            },
                            HasHeader = true,
                            Separator = ","
                        });
```

Then, the next step is to build the pipeline transformations and to specify what trainer/algorithm you are going to use.
In this case you are doing the following transformations:
- Concat current features to a new Column named NumFeatures
- Transform  productId using [one-hot encoding](https://en.wikipedia.org/wiki/One-hot)
- Concat all generated fetures in one column named 'Features'
- Copy next column to rename it to "Label"
- Specify the "Fast Tree Tweedie" Trainer as the algorithm to apply to the model

After designing the pipeline, you can load the dataset into the DataView, although this step is just configuration, it is lazy and won't be loaded until training the model in the next step.

```csharp
var trainingPipeline = mlContext.Transforms.Concatenate(outputColumn: "NumFeatures", "year", "month", "units", "avg", "count", "max", "min", "prev" )
    .Append(mlContext.Transforms.Categorical.OneHotEncoding(inputColumn:"productId", outputColumn:"CatFeatures"))
    .Append(mlContext.Transforms.Concatenate(outputColumn: "Features", "NumFeatures", "CatFeatures"))
    .Append(mlContext.Transforms.CopyColumns("next", "Label"))
    .Append(trainer = mlContext.Regression.Trainers.FastTreeTweedie("Label", "Features"));

var trainingDataView = textLoader.Read(dataPath);
```


#### 2. Train model

After building the pipeline, we train the forecast model by fitting or using the training data with the selected algorithm. In that step, the model is built, trained and returned as an object:

```csharp
var model = trainingPipeline.Fit(trainingDataView);
```

#### 3. Evaluate model

In this case, the evaluation of the model is performed before training the model with a cross-validation approach, so you obtain metrics telling you how good is the accuracy of the model. 

```csharp
var crossValidationResults = mlContext.Regression.CrossValidate(trainingDataView, trainingPipeline, numFolds: 6, labelColumn: "Label");
            
ConsoleHelper.PrintRegressionFoldsAverageMetrics(trainer.ToString(), crossValidationResults);
```

#### 4. Save the model for later comsumption from end-user apps

Once the model is created and evaluated, you can save it into a .ZIP file which could be consumed by any end-user application with the following code:

```csharp            
using (var file = File.OpenWrite(outputModelPath))
    model.SaveTo(mlContext, file);
```

#### 5. Try the model with a simple test prediction

Basically, you can load the model from the .ZIP file create some sample data, create the "prediction function" and finally you make a prediction.


```csharp
ITransformer trainedModel;
using (var stream = File.OpenRead(outputModelPath))
{
    trainedModel = mlContext.Model.Load(stream);
}

var predictionFunct = trainedModel.MakePredictionFunction<ProductData, ProductUnitPrediction>(mlContext);

Console.WriteLine("** Testing Product 1 **");

// Build sample data
ProductData dataSample = new ProductData()
{
    productId = "263",
    month = 10,
    year = 2017,
    avg = 91,
    max = 370,
    min = 1,
    count = 10,
    prev = 1675,
    units = 910
};

//model.Predict() predicts the nextperiod/month forecast to the one provided
ProductUnitPrediction prediction = predictionFunct.Predict(dataSample);
Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (units): 551, Forecast Prediction (units): {prediction.Score}");

```

## Citation
eShopDashboardML dataset is based on a public Online Retail Dataset from **UCI**: http://archive.ics.uci.edu/ml/datasets/online+retail
> Daqing Chen, Sai Liang Sain, and Kun Guo, Data mining for the online retail industry: A case study of RFM model-based customer segmentation using data mining, Journal of Database Marketing and Customer Strategy Management, Vol. 19, No. 3, pp. 197â€“208, 2012 (Published online before print: 27 August 2012. doi: 10.1057/dbm.2012.17).
