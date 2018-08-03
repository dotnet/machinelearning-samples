# ML.NET Code Walkthrough

## ML.NET: Model creation

**ML.NET** defines and implements a well number of components to create modern machine learning pipelines:
* import models: ingest data (text files, images, etc) into your model
* transform models: handles and manipulates data 
* learners models: support for a variety of algorithms to model many kinds of machine learning problems (such as regression, classification, etc)

In the following sections, we will describe the workflow followed for training the machine learning models implemented in eShopDashboard.

The `LearningPipeline` will allow to you to build machine learning models easily. So first thing, is to create one basic pipeline that will host our components:

```csharp
var learningPipeline = new LearningPipeline();
```

The pipelines used in `eShopForecastModelsTrainer` are composed of several components that are added consecutively. Usually, the first stages are use to load and transform the data before being processed by the machine learning algorithm: 

1) **TextLoader**. This component loads the input data, using a csv file format. The format of the file is inferred from the generic class used to instantiate the object, so you don't need to specify the format by hand. You can also specify if a header row is present, or the separator used between columns.

```csharp
learningPipeline.Add(
  new TextLoader<CountryData>(dataPath, header: true, sep: ",")
);
```

2) **ColumnConcatenator**. Many models in ML.NET need data columns (also called *features*) to be arranged in a certain way. In our case, we will use FastTreeTweedieRegressor, and this model needs features to be combined in arrays of the same type. So, all numerical features must be combined in the same array (named `NumericalFeatures`); similarly, all categorical features must be combined in another array (named `CategoryFeatures`).
```csharp
learningPipeline.Add(
  new ML.Transforms.ColumnConcatenator(
    outputColumn: "NumericalFeatures",
    nameof(CountryData.year),
    nameof(CountryData.month),
    nameof(CountryData.max),
    nameof(CountryData.min),
    nameof(CountryData.idx),
    nameof(CountryData.count),
    nameof(CountryData.sales),
    nameof(CountryData.avg),
    nameof(CountryData.prev)
  )
);
```
```csharp
 learningPipeline.Add(
   new ML.Transforms.ColumnConcatenator(outputColumn: "CategoryFeatures", nameof(CountryData.country))
 );
```
3) **CategoricalOneHotVectorizer**. One common transformation when dealing with categorical features is [one-hot encoding](https://en.wikipedia.org/wiki/One-hot). This transformation is provided as a common transformation by ML.NET.

```csharp
learningPipeline.Add(
  new ML.Transforms.ColumnConcatenator(outputColumn: "Features", "NumericalFeatures", "CategoryFeatures")
);
```

4) **FastTreeTweedieRegressor**. ML.NET implements many machine learning algorithms supporting scenarios like regression, classification or anomaly detection. This experiment uses a FastTree algorithm named **`FastTreeTweedieRegressor`**, but you can look up many others under the namespace `Microsoft.ML.Trainers.*`.

```csharp
learningPipeline.Add(
  new ML.Trainers.FastTreeTweedieRegressor { NumThreads = 1, FeatureColumn = "Features" }
);
```

After composing all pipelines stages, all is left is to execute `Train` method and wait for **the MODEL**

```csharp
var model = learningPipeline.Train<CountryData, CountrySalesPrediction>();
```
After obtaining the model, we can write it to disk, to be able to reload later

```csharp
await model.WriteAsync(outputModelPath);
```

The code previously shown is an excerpt of [CountryModelHelper.cs](https://github.com/dotnet-architecture/eShopDashboardAI/blob/dev/src/eShopForecastModelsTrainer/CountryModelHelper.cs); check source code for more information. Same pattern is used in [ProductModelHelper](https://github.com/dotnet-architecture/eShopDashboardAI/blob/dev/src/eShopForecastModelsTrainer/ProductModelHelper.cs).

## ML.NET: Model evaluation

Machine learning models are created, but most of the times, they are used in a different place. For this reason, models are serialized and they can be used in environment not as resourceful as the training facilities. ML.NET provides an API for saving and loading models, which can be easily evaluated in a desktop, mobile app or containerized app.

The code for evaluating the model is split in two parts:
* **Read model from file.** The model file is read asynchronously; the input and output types are the same types used during training (in this case, input type is `ProductData` and the output type is`ProductUnitPrediction`.
```csharp
PredictionModel<ProductData, ProductUnitPrediction> model = 
    await PredictionModel.ReadAsync<ProductData, ProductUnitPrediction>(modelPath);
```

* **Predict using the model.** In this case, the result of the prediction is a number (score)

```csharp
var score = model.Predict(inputExample)
```
This pattern is used in the eShopDashboard ([ProductSales.cs](https://github.com/dotnet-architecture/eShopDashboardAI/blob/dev/src/eShopDashboard/Forecasting/ProductSales.cs) and [CountrySales.cs](https://github.com/dotnet-architecture/eShopDashboardAI/blob/dev/src/eShopDashboard/Forecasting/CountrySales.cs)) and serves the data which is used to plot the forecasts in the web dashboard plots.

