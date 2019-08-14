# eShopDashboardML - Sales forecasting 

| ML.NET version | API type    | Status     | App Type                             | Data type                 | Scenario       | ML Task                 | Algorithms                                           |
|----------------|-------------|------------|--------------------------------------|---------------------------|----------------|-------------------------|------------------------------------------------------|
| v1.3.1         | Dynamic API | Up-to-date | ASP.NET Core web app and Console app | SQL Server and .csv files | Sales forecast | Regression, Time Series | FastTreeTweedie Regression, Single Spectrum Analysis |


eShopDashboardML is a web app with Sales Forecast predictions (per product and per country) using [Microsoft Machine Learning .NET (ML.NET)](https://github.com/dotnet/machinelearning).


# Overview

This end-to-end sample app highlights the usage of ML.NET API by showing the following topics:

1. How to train, build and generate ML models:
   - Implemented as a [console app](src\eShopForecastModelsTrainer) using .NET Core.
2. How to predict upcoming months of sales forecasts by using the trained ML model: 
   - Implemented as a single, monolithic [web app](src/eShopDashboard) using [ASP.NET Core Razor](https://docs.microsoft.com/aspnet/core/tutorials/razor-pages/). 

The app is also using a SQL Server database for regular product catalog and orders info, as many typical web apps using SQL Server. In this case, since it is an example, it is, by default, using a localdb SQL database so there's no need to setup a real SQL Server. The localdb database will be created, along with sample populated data, the first time you run the web app.

If you want to use a real SQL Server or Azure SQL Database, you just need to change the connection string in the app.

When you run the app, it opens the webpage with a search box says "Type a product." You can type for any product, i.e. "bottle." Then a list of products related to keyword "bottle" will show in autocomplete suggestions. Once you select any product, then the sales forecast of that product will be shown as below.

Here's a sample screenshot of the web app and one of the forecast predictions:

![image](./docs/images/eShopDashboard.png)

## Setup

Learn how to set up the sample's environment in Visual Studio along with further explanations on the code:

- [Setting up eShopDashboard in Visual Studio and running the web app](docs/Setting-up-eShopDashboard-in-Visual-Studio-and-running-it.md)

- [Create and Train your ML models](docs/Create-and-train-the-models-%5BOptional%5D.md)
  - This step is optional as the web app is already configured to use a pre-trained model. But you can create your own trained model and swap the pre-trained model with your own.

## ML.NET Code Overview

### Problem

This problem is centered around country and product forecasting based on previous sales.

### DataSet

To solve this problem, two independent ML models are built that take the following datasets as input:  

| Data Set | columns |
|----------|--------|
| **products stats**  | next, productId, year, month, units, avg, count, max, min, prev      |
| **country stats**  | next, country, year, month, max, min, std, count, sales, med, prev   |

[Explanation of Dataset](docs/Details-of-Dataset.md) - Goto this link for detailed information on dataset.

### ML Tasks - [Regression](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#regression) and Time Series

The sample shows two different ML tasks and algorithms that can be used for forecasting:
-  **Regression** using FastTreeTweedie Regression
-  **Time Series** using Single Spectrum Analysis

**Regression** is a supervised machine learning task that is used to predict the value of the **next** period (in this case, the sales prediction) from a set of related features/variables. **Regression** works best with linear data.

**Time Series** is an estimation technique that can be used to forecast **multiple** periods in the future. **Time Series** works well in scenarios that involve non-linear data where trends and patterns are difficult to distinguish.

### Solution

To solve this problem, first we will build the ML models by training each model on existing data. Next, we will evaluate how good it is. Finally, we will consume the model to predict sales.

Note that the **Regression** sample implements two independent models to forecast linear data:
- Model to predict product's demand forecast for the next period (month)
- Model to predict country's sales forecast for the next period (month)

The **Time Series** sample currently implements the product's demand forecast for the next **two** periods (months). The **Time Series** sample uses the same products as in the **Regression** sample so that you can compare the forecasts from the two algorithms.

When learning/researching the samples, you can focus choose to focus specifically on regression or time series.

![Build -> Train -> Evaluate -> Consume](docs/images/modelpipeline.png)

#### 1. Load the Dataset

Both the **Regression** and **Time Series** samples start by loading data using **TextLoader**. To use **TextLoader**, we must specify the type of the class that represents the data schema. Our class type is **ProductData**. 

```csharp
 public class ProductData
    {
        // The index of column in LoadColumn(int index) should be matched with the position of columns in the underlying data file.

        // The next column is used by the Regression algorithm as the Label (e.g. the value that is being predicted by the Regression model).
        [LoadColumn(0)]
        public float next;

        [LoadColumn(1)]
        public string productId;

        [LoadColumn(2)]
        public float year;

        [LoadColumn(3)]
        public float month;

        [LoadColumn(4)]
        public float units;

        [LoadColumn(5)]
        public float avg;

        [LoadColumn(6)]
        public float count;

        [LoadColumn(7)]
        public float max;

        [LoadColumn(8)]
        public float min;

        [LoadColumn(9)]
        public float prev;
    }
```
Load the dataset into the **DataView**. 

```csharp

var trainingDataView = mlContext.Data.LoadFromTextFile<ProductData>(dataPath, hasHeader: true, separatorChar:',');

```

In the following steps, we will build the pipeline transformations, specify which trainer/algorithm to use, evaluate the models, and test their predictions. This is where the steps start to differ between the [**Regression**](#2-regression-create-the-pipeline) and [**Time Series**](#6-time-series-create-the-pipeline) samples - the remainder of this walkthrough looks at each of these algorithms separately.


#### 2. Regression: Create the Pipeline

This step shows how to create the pipeline that will later be used for building and training the **Regression** model.

Specifically, we do the following transformations:
- Concatenate current features to a new column named **NumFeatures**.
- Transform **productId** using [one-hot encoding](https://en.wikipedia.org/wiki/One-hot).
- Concatenate all generated features in one column named **Features**.
- Copy **next** column to rename it to **Label**.
- Specify the **Fast Tree Tweedie** trainer as the algorithm to apply to the model.

You can load the dataset either before or after designing the pipeline. Although this step is just configuration, it is lazy and won't be loaded until training the model in the next step.

[Model build and train](./src/eShopForecastModelsTrainer/ProductModelHelper.cs)

```csharp

var trainer = mlContext.Regression.Trainers.FastTreeTweedie("Label", "Features");

var trainingPipeline = mlContext.Transforms.Concatenate(outputColumnName: "NumFeatures", nameof(CountryData.year),
                                nameof(CountryData.month), nameof(CountryData.max), nameof(CountryData.min),
                                nameof(CountryData.std), nameof(CountryData.count), nameof(CountryData.sales),
                                nameof(CountryData.med), nameof(CountryData.prev))
                    .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeatures", inputColumnName: nameof(CountryData.country)))
                    .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", "NumFeatures", "CatFeatures"))
                    .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CountryData.next)))
                    .Append(trainer);

```

#### 3. Regression: Evaluate the Model

In this case, the **Regression** model is evaluated before training the model with a cross-validation approach. This is to obtain metrics that indicate the accuracy of the model. 

```csharp
var crossValidationResults = mlContext.Regression.CrossValidate(data:trainingDataView, estimator:trainingPipeline, numberOfFolds: 6, labelColumnName: "Label");
            
ConsoleHelper.PrintRegressionFoldsAverageMetrics(trainer.ToString(), crossValidationResults);
```
#### 4. Regression: Train the Model

After building the pipeline, we train the **Regression** forecast model by fitting or using the training data with the selected algorithm. In this step, the model is built, trained and returned as an object:

```csharp
var model = trainingPipeline.Fit(trainingDataView);
```

#### 4. Regression: Save the Model

Once the **Regression** model is created and evaluated, you can save it into a **.zip** file which can be consumed by any end-user application with the following code:

```csharp            
using (var file = File.OpenWrite(outputModelPath))
                mlContext.Model.Save(model, trainingDataView.Schema, file);
```

#### 5. Regression: Test the Prediction

To create a prediction, load the **Regression** model from the **.zip** file. 

This sample uses the last month of a product's sample data to predict the unit sales in the next month. 

```csharp
ITransformer trainedModel;
using (var stream = File.OpenRead(outputModelPath))
{
    trainedModel = mlContext.Model.Load(stream,out var modelInputSchema);
}

var predictionEngine = mlContext.Model.CreatePredictionEngine<CountryData, CountrySalesPrediction>(trainedModel);

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

// Predict the next period/month forecast to the one provided
ProductUnitPrediction prediction = predictionEngine.Predict(dataSample);
Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (units): 551, Forecast Prediction (units): {prediction.Score}");

```

#### 6. Time Series: Create the Pipeline

This step shows how to create the pipeline that will later be used for training the **Time Series** model.

Specifically, the **Single Spectrum Analysis (SSA)** trainer is the algorithm that is used. This algorithm uses the following parameters:
- **outputColumnName**: This is the name of the column that will be used to store predictions. The column must be a vector of type **Single**. In a later step, we define a class named **ProductUnitTimeSeriesPrediction** that contains this output column.
- **inputColumnName**: This is the name of the column that is being predicted/forecasted. The column contains a value at a timestamp in the time series and must be of type **Single**. In our sample, we are predicting/forecasting product **units**.
- **windowSize**:  This parameter is used to define a sliding window of time that is used by the algorithm to decompose the time series data into trend, seasonal, or noise components. Typically, you should start with a window size that is representative of the business cycle in your scenario. In our sample, the product data is based on a 12 month cycle so we will select a window size that is a multiple of 12. 
- **seriesLength**: TODO - Need guidance
- **trainSize**: TODO - Need guidance
- **horizon**: This parameter indicates the number of time periods to predict/forecast. In our sample, we specify 2 to indicate that the next 2 months of product units will be predicated/forecasted.
- **confidenceLevel**: This parameter indicates the likelihood the prediction/forecast value will fall within the specified interval bounds. TODO - Need to confirm this is correct. Typically, .95 is an acceptable starting point.
- **confidenceLowerBoundColumn**: This is the name of the column that will be used to store the **lower** confidence interval bound for each forecasted value. The **ProductUnitTimeSeriesPrediction** class also contains this output column.
- **confidenceUpperBoundColumn**: This is the name of the column that will be used to store the **upper** confidence interval bound for each forecasted value. The **ProductUnitTimeSeriesPrediction** class also contains this output column.

Specifically, we add the following trainer to the pipeline:

```csharp
    // Create and add the forecast estimator to the pipeline.
    IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(
        outputColumnName: nameof(ProductUnitTimeSeriesPrediction.ForecastedProductUnits), 
        inputColumnName: nameof(ProductData.units),
        windowSize: 3,
        seriesLength: productDataSeriesLength,
        trainSize: productDataSeriesLength,
        horizon: 2,
        confidenceLevel: 0.95f,
        confidenceLowerBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceLowerBound),
        confidenceUpperBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceUpperBound));
```

#### 7. Time Series: Train the Model

Before training the **Time Series** model, we first must filter the loaded dataset to select the data series for the specific product that will be used for forecasting sales.

```csharp
var productId = 988;
IDataView productDataView = mlContext.Data.FilterRowsByColumn(allProductsDataView, nameof(ProductData.productId), productId, productId + 1);
```

Next, we train the model using the data series for the specified product.

```csharp
// Train the forecasting model for the specified product's data series.
ITransformer forecastTransformer = forecastEstimator.Fit(productDataView);
```

#### 8. Time Series: Save the Model

To save the model, we first must create the **TimeSeriesPredictionEngine** which is used for both getting predictions and saving the model.

The **Time Series** model is saved using the **CheckPoint** method which saves the model to a **.zip** file that can be consumed by any end-user application:

```csharp
// Create the forecast engine used for creating predictions.
TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

// Save the forecasting model so that it can be loaded within an end-user app.
forecastEngine.CheckPoint(mlContext, outputModelPath);
```

You may notice that this is different from the above **Regression** sample which instead used the **Save** method for saving the model. **Time Series** is different because it requires that the model's state to be continuously updated with new observed values as predictions are made. As a result, the **CheckPoint** method exists to update and save the model state on a reoccurring basis. This will be shown in further detail in a later step of this sample. For now, just remember that **Checkpoint** is used for saving the **Time Series** model.

#### 9. Time Series: Test the Prediction

To get a prediction, load the **Time Series** model from the **.zip** file and create a new **TimeSeriesPredictionEngine**. After this, we can get a prediction.

```csharp
// Load the forecast engine that has been previously saved.
ITransformer forecaster;
using (var file = File.OpenRead(outputModelPath))
{
    forecaster = mlContext.Model.Load(file, out DataViewSchema schema);
}

// We must create a new prediction engine from the persisted model.
TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext); forecastEngine = forecaster.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

ProductUnitTimeSeriesPrediction originalSalesPrediction = forecastEngine.Predict();
```

The **ProductUnitTimeSeriesPrediction** type that we specified when we created the **TimeSeriesPredictionEngine** is used to store the prediction results:

```csharp
   public class ProductUnitTimeSeriesPrediction
    {
        public float[] ForecastedProductUnits { get; set; }

        public float[] ConfidenceLowerBound { get; set; }

        public float[] ConfidenceUpperBound { get; set; }
    }
```

Remember that when we created the SSA forecasting trainer using the **ForecastBySsa** method, we provided the following parameter values:
- **horizon**: 2
- **confidenceLevel**: .95f

As a result of this, when we call the **Predict** method using the loaded model, the **ForecastedProductUnits** vector will contain **two** forecasted values. Similarly, the **ConfidenceLowerBound** and **ConfidenceUpperBound** vectors will each contain **two** values based on the specified **confidenceLevel**.

You may notice that the **Predict** method has several overloads that accept the following parameters:
- **horizon**
- **confidenceLevel**
- **ProductData example**

This allows you to specify new values for **horizon** and **confidenceLevel** each time that you do a prediction. Also, you can pass in new observed **ProductData** values for the time series using the **example** parameter. 

When calling **Predict** with new observed **ProductData** values, this updates the model state with these data points in the time series. You may then choose to save this model to disk by calling the **CheckPoint** method.

This is also seen in our sample:

```csharp
ProductUnitTimeSeriesPrediction updatedSalesPrediction = forecastEngine.Predict(newProductData, horizon: 1);

 // Save the updated forecasting model.
 forecastEngine.CheckPoint(mlContext, outputModelPath);
```

// TODO: Need clarification on how to evaluate the accuracy of this model; there is the confidence level, but any other mechanism besides that?

## Citation
eShopDashboardML dataset is based on a public Online Retail Dataset from **UCI**: http://archive.ics.uci.edu/ml/datasets/online+retail
> Daqing Chen, Sai Liang Sain, and Kun Guo, Data mining for the online retail industry: A case study of RFM model-based customer segmentation using data mining, Journal of Database Marketing and Customer Strategy Management, Vol. 19, No. 3, pp. 197â€“208, 2012 (Published online before print: 27 August 2012. doi: 10.1057/dbm.2012.17).
