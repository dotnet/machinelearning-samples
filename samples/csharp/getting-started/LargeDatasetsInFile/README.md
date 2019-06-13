#  Using SQL Server database as a data source - Url Clikc Prediction
Need to add more changes

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0           | Dynamic API | Up-to-date | Console app | .csv file | SQL server Database integration -Fraud Detection | Two-class classification | FastTree Binary Classification |


This sample demonstrates how to use SQL server database as a data source for an ML.Net pipeline. As ML.Net does not have native support for a database, this sample shows how data can be accessed using an IEnumerable. Since the database is treated as any other datasource, it is possible to query the database and use the resulting data for training and prediction scenarios.

## DataSet

For this sample we are using Credit Card Fraud Transactions data to predict whether a transaction is fraud or not. In this sample, I have exporimported the data from .csv file to local SQL server and inserted auto-identity column so that Entity framework creates entity models from database. The table name in my lcoal database is CreditCardTransaction. 

Details of Credit card dataset can be found [here](./docs/Dataset_Details.txt)

## Problem
Enterprise users have a need to use their existing data set that is in their company's database to train and predict with ML.Net. They need support to leverage their existing relational table schema, ability to read from the database directly, and to be aware of memory limitations as the data is being consumed.

## Solution
This sample shows how to use the Entity Framework Core to connect to a database, query  and feed the resulting data into an ML.Net pipeline.

This sample uses SQL Database server to help demonstrate the database integration, but any database that is supported by the Entity Framwork Core can be used. As ML.Net can create an **IDataView** from an **IEnumerable**, this sample will use the IEnumerable that is returned from a query to feed the data into the ML.Net pipeline. To prevent the Entity Framework Core from loading all the data in from a result, a no tracking query is used. 

The steps Build, train, consume model are same as other samples. what we need to consider more important here is how are we loading data from database to in-memory obejct and from in-memory object to IDataView obejct.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)


### 1. Build model
Building a model includes:

 - **Read the data from database into in-memory variable i.e IEnumerable object:**

    create Database context object and read database data into IEnumerable object, here the data is in CreditCardTransaction table.

```csharp
dbContext = new masterContext();

var fullData = dbContext.CreditCardTransaction;
```

-  **Load the data from IEnumerable object to IDataView of ML.Net:** 

    Load the in-memory data held in IEnumerable object into IDataView using TextLoader. 

```csharp
IDataView fullDataView = mlContext.Data.LoadFromEnumerable(fullData);
```

- Split the above data into train and test data sets.

```csharp
var trainTestData = mlContext.Data.TrainTestSplit(fullDataView, testFraction: 0.2, seed: 1);
 ```

- In this dataset we don't need all the columns for training so select the feature columns that you need for training.

```csharp
//Get all the feature column names (All except the Label and the IdPreservationColumn)
string[] featureColumnNames = trainDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != nameof(CreditCardTransaction.Class)) // Do not include the Label column
                .Where(name => name != nameof(CreditCardTransaction.Idkey))               // Do not include the IdPreservationColumn/StratificationColumn
                .Where(name => name != nameof(CreditCardTransaction.Time)) // Do not include the Time column. Not needed as feature column
                .Where(name => name != "SamplingKeyColumn")
                .ToArray();
```
- Create an Estimator and transform the data with a Concatenate() and Normalize by Mean Variance. 

```csharp
// Create the data process pipeline
IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { "Time", nameof(CreditCardTransaction.Idkey) })
                                            .Append(mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
                                                                                 outputColumnName: "FeaturesNormalizedByMeanVar")));
 ```                                                                                

- Choosing a trainer/learning algorithm (FastTree) to train the model with.

```csharp
 // Set the training algorithm
var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(CreditCardTransaction.Class),
                                                                                                featureColumnName: "FeaturesNormalizedByMeanVar",
                                                                                                numberOfLeaves: 20,
                                                                                                numberOfTrees: 100,
                                                                                                minimumExampleCountPerLeaf: 10,
                                                                                                learningRate: 0.2);
 ```                                                                                               
### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fraud values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`trainDataView`) in a DataView object.

`````csharp    
    ITransformer model = pipeline.Fit(trainDataView);
`````

### 3. Evaluate model
We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against testDataView.

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```csharp
var predictions = model.Transform(testDataView);

var metrics = mlContext.BinaryClassification.Evaluate(data: predictions,
                                                                  labelColumnName: nameof(CreditCardTransaction.Class),
                                                                  scoreColumnName: "Score");
```

### 4. Consume model
After the model is trained, you can use the `Predict()` API to predict if a transaction is a fraud, using a IDataSet.

```csharp
var predictionEngine = mlContext.Model.CreatePredictionEngine<CreditCardTransaction, TransactionFraudPredictionWithContribution>(model);
            Console.WriteLine($"\n \n Test 5 transactions, from the test datasource, that should be predicted as fraud (true):");

mlContext.Data.CreateEnumerable<CreditCardTransaction>(predictDataView, reuseRowObject: false)
                       .Where(x => x.Class == true)
                       .Take(5)
                       .Select(predictData => predictData)
                       .ToList()
                       .ForEach(predictData =>
                       {
                           Console.WriteLine($"--- Transaction ---");
                           PrintToConsole(predictData);
                           predictionEngine.Predict(predictData).PrintToConsole();
                           Console.WriteLine($"-------------------");
                       });

Console.WriteLine($"\n \n Test 5 transactions, from the test datasource, that should NOT be predicted as fraud (false):");

mlContext.Data.CreateEnumerable<CreditCardTransaction>(predictDataView, reuseRowObject: false)
                       .Where(x => x.Class == false)
                       .Take(5)
                       .Select(predictData => predictData)
                       .ToList()
                       .ForEach(predictData =>
                       {
                           Console.WriteLine($"--- Transaction ---");
                           PrintToConsole(predictData);
                           predictionEngine.Predict(predictData).PrintToConsole();
                           Console.WriteLine($"-------------------");
                       });
```