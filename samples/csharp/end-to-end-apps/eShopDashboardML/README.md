# eShopDashboardML - Sales forecasting 

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.6           | Dynamic API | *Evolving* | ASP.NET Core web app and Console app | SQL Server and .csv files | Sales forecast | Regression | FastTreeTweedie Regression |


eShopDashboardML is a web app with Sales Forecast predictions (per product and per country) using [Microsoft Machine Learning .NET (ML.NET)](https://github.com/dotnet/machinelearning).

## API version: New Dynamic/Estimators API
It is important to note that this sample uses the **dynamic API with Estimators**, available since ML.NET v0.6.

# Overview

This end-to-end sample app highlights the usage of ML.NET API by showing the following topics:

1. How to train, build and generate ML models 
   - Implemented as a [console app](src\eShopForecastModelsTrainer) using .NET Core.
2. How to predict the next month of Sales Forecasts by using the trained ML model 
   - Implemented as a single, monolithic [web app](src\eShopDashboard) using [ASP.NET Core Razor](https://docs.microsoft.com/aspnet/core/tutorials/razor-pages/). 

The app is also using a SQL Server database for regular product catalog and orders info, as many typical web apps using SQL Server. In this case, since it is an example, it is, by default, using a localdb SQL database so there's no need to setup a real SQL Server. The localdb database will be created, along with with sample populated data, the first time you run the web app.

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

This problem is centered around country and product forecasting based on previus sales

### DataSet

To solve this problem, we will build an ML model that takes as inputs:  

| Data Set | columns |
|----------|--------|
| **products stats**  | next, productId, year, month, units, avg, count, max, min, prev      |
| **products stats**  | next, country, year, month, max, min, std, count, sales, med, prev   |

### ML task - [Regression](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#regression)

The ML Task for this sample is a Regression, which is a supervised machine learning task that is used to predict the value of the next period (in this case the sales prediction) from a set of related features/variables.

### Solution

To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and finally we'll consume the model to predict sales.

![Build -> Train -> Evaluate -> Consume](./../../getting-started/shared_content/modelpipeline.png)

#### 1. Build Model

Next, the model's pipeline is built

Then, you need to apply some transformations to the data:
- Concat current features to a new Column named NumFeatures
- Tramsform  productId using [one-hot](https://en.wikipedia.org/wiki/One-hot)
- Concat all generated fetures in one column
- Copy next colmun to rename it to label
- Add Fast Tree Tweedie Trainer

Add a KMeansPlusPlusTrainer; main parameter to use with this learner is clustersCount, that specifies the number of clusters

[Model build and train](./src/eShopForecastModelsTrainer/ProductModelHelper.cs)

```csharp
 var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
 var ctx = new RegressionContext(env);


 var reader = new TextLoader(env, new TextLoader.Arguments
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


 var pipeline = new ConcatEstimator(env, "NumFeatures", new[] { "year", "month", "units", "avg", "count", "max", "min", "prev" })
     .Append(new CategoricalEstimator(env, "CatFeatures", "productId"))
     .Append(new ConcatEstimator(env, "Features", new[] { "NumFeatures", "CatFeatures" }))
     .Append(new CopyColumnsEstimator(env, "next", "Label"))
     .Append(new FastTreeTweedieTrainer(env, "Label", "Features"));

```

#### 2. Train model

After building the pipeline, we train the forecast model by fitting or using the training data with the selected algorithm:

```csharp
 var model = pipeline.Fit(datasource);
```

#### 3. Evaluate model

We evaluate the accuracy of the model. Evaluate model with a sample products

```csharp
 // Read the model that has been previously saved by the method SaveModel
 var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
 ITransformer model;
 using (var file = File.OpenRead(outputModelPath))
 {
     model = TransformerChain
         .LoadFrom(env, file);
 }

 var predictor = model.MakePredictionFunction<ProductData, ProductUnitPrediction>(env);
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
 ProductUnitPrediction prediction = predictor.Predict(dataSample);
 Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (units): 551, Forecast Prediction (units): {prediction.Score}");
```


#### 4. Consume the model

Basically, we load the model, then the data file and finally we make a prediction function.

- [Country sales prediction](./src/eShopDashboard/Forecast/CountrySales.cs##L66)
- [Product sales sales prediction](./src/eShopDashboard/Forecast/ProductSales.cs##L66)

```csharp
 var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
 ITransformer model;
 using (var file = File.OpenRead(modelPath))
 {
     model = TransformerChain
         .LoadFrom(env, file);
 }
 return model.MakePredictionFunction<ProductData, ProductUnitPrediction>(env);
```

## Citation
eShopDashboardML dataset is based on a public Online Retail Dataset from **UCI**: http://archive.ics.uci.edu/ml/datasets/online+retail
> Daqing Chen, Sai Liang Sain, and Kun Guo, Data mining for the online retail industry: A case study of RFM model-based customer segmentation using data mining, Journal of Database Marketing and Customer Strategy Management, Vol. 19, No. 3, pp. 197â€“208, 2012 (Published online before print: 27 August 2012. doi: 10.1057/dbm.2012.17).