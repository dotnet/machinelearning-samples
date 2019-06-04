#  Using SQLite database as a data source 

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0           | Dynamic API | Up-to-date | Console app | .csv file | SQLite Database integration -Fraud Detection | Two-class classification | FastTree Binary Classification |


This sample demonstrates how to use SQLite database as a data source for an ML.Net pipeline. As ML.Net does not have native support for a database, this sample shows how data can be accessed using an IEnumerable. Since the database is treated as any other datasource, it is possible to query the database and use the resulting data for training and prediction scenarios.

## Problem
Enterprise users have a need to use their existing data set that is in their company's database to train and predict with ML.Net. They need support to leverage their existing relational table schema, ability to read from the database directly, and to be aware of memory limitations as the data is being consumed.

## Solution
This sample shows how to use the Entity Framework Core to connect to a database, query  and feed the resulting data into an ML.Net pipeline.

This sample uses SQLite Database  to help demonstrate the database integration, but any database that is supported by the Entity Framwork Core can be used. As ML.Net can create an **IDataView** from an **IEnumerable**, this sample will use the IEnumerable that is returned from a query to feed the data into the ML.Net pipeline. To prevent the Entity Framework Core from loading all the data in from a result, a no tracking query is used. 

The steps Build, train, consume model are same as other samples. what we need to consider more important here is how are we loading data from database to in-memory obejct and from in-memory object to IDataView obejct.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model
Building a model includes:

 - **Read the data from database into in-memory variable i.e IEnumerable object:**

      create Database context object and read database data into IEnumerable object, here the data is in CreditCardTransaction table.

```csharp
using (var db = new AdultCensusContext())
{
  // Query our training data from the database. This query is selecting everything from the AdultCensus table. The
  // result is then loaded by ML.Net through the LoadFromEnumerable. LoadFromEnumerable returns an IDataView which
  // can be consumed by an ML.Net pipeline.
  // NOTE: For training, ML.Net requires that the training data is processed in the same order to produce consistent results.
  // Therefore we are sorting the data by the AdultCensusId, which is an auto-generated id.
  // NOTE: That the query used here sets the query tracking behavior to be NoTracking, this is particularly useful because
  // our scenarios only require read-only access.
  foreach (var adult in db.AdultCensus.AsNoTracking().OrderBy(x => x.AdultCensusId))
  {
      yield return adult;
  }
}
```

-  **Load the data from IEnumerable object to IDataView of ML.Net:** 

    Load the in-memory data held in IEnumerable object into IDataView using TextLoader. 

```csharp
/// Query the data from the database, please see <see cref="QueryData"/> for more information.
var dataView = mlContext.Data.LoadFromEnumerable(QueryData());
```

- Split the above data into train and test data sets.

```csharp
 var trainTestData = mlContext.Data.TrainTestSplit(dataView);
 ```

 - Create an Estimator and transform the data by applying OneHotEncoding transformation to categorical data and then concatenate the selected field into features.
 - Choosing a trainer/learning algorithm (LightGbm) to train the model with.

 ```csharp
var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] {
                new InputOutputColumnPair("MsOHE", "MaritalStatus"),
                new InputOutputColumnPair("OccOHE", "Occupation"),
                new InputOutputColumnPair("RelOHE", "Relationship"),
                new InputOutputColumnPair("SOHE", "Sex"),
                new InputOutputColumnPair("NatOHE", "NativeCountry")
            }, OneHotEncodingEstimator.OutputKind.Binary)
                .Append(mlContext.Transforms.Concatenate("Features", "MsOHE", "OccOHE", "RelOHE", "SOHE", "NatOHE"))
                .Append(mlContext.BinaryClassification.Trainers.LightGbm());
 ``` 

 ### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fraud values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`trainDataView`) in a DataView object.

```csharp    
ITransformer model = pipeline.Fit(trainDataView);
```

### 3. Evaluate model
We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against testDataView.

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```csharp
var predictions = model.Transform(testDataView);

var metrics = mlContext.BinaryClassification.Evaluate(data: predictions);
```

### 4. Consume model
After the model is trained, you can use the `Predict()` API to predict if a transaction is a fraud, using a IDataSet.

```csharp
var predictionEngine = mlContext.Model.CreatePredictionEngine<AdultCensus, AdultCensusPrediction>(model);
 

var recordsToPredict = mlContext.Data.CreateEnumerable<AdultCensus>(predictDataView, reuseRowObject: false).Take(5);

foreach (var x in recordsToPredict)
{
    var y = predictionEngine.Predict(x);
    Console.WriteLine("Actual Label ={0}, Predicted Label = {0}", x.Label, y.Label);
}
```