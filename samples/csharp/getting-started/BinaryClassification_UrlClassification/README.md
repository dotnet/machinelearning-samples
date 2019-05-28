# URL Classification

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0           | Dynamic API | Up-to-date | Console app | .txt files | URL classification | Binary classification | FieldAwareFactorizationMachine |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict whether an URL is malicious or not. In the world of machine learning, this type of classification is known as **binary classification**.

## Dataset
The dataset used is this: [URL Reputation Data Set] (https://archive.ics.uci.edu/ml/datasets/URL+Reputation)
This dastaset is collection of 120-days data which contains 2.3 million records and 3.2 million features.

* The first column is Label column where
  - +1 corresponds to Malicious URL
  - -1 corresponds to Benign URL
* Remaining columns are features which are arranged in [sparse matrix](https://en.wikipedia.org/wiki/Sparse_matrix) format. 

## Problem
This problem is to classify whether a URL is malicious or not. To solve this problem, we will build an ML model by using  one of the Binary Classification algorithms i.e FieldAwareFactorizationMachine.

## ML task - Binary classification
The generalized problem of **binary classification** is to classify items into items into one of the two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

**Download Data:**

* In this sample, we are downloading the dataset using HttpClient as the dataset is very large.  

```CSharp
//STEP 1: Download dataset
DownloadDataset(originalDataDirectoryPath);
```

**Prepare Data:**
* As the downloaded dataset contains feature columns in **sparse matrix format**, we need to prepare/transform the dataset by adding a new column that is **total number of features in dataset**   before the  features columns(second column in this case) so that the dataset is compatable   for ML.Net API for training and evaluation. As our dataset contains **3231961** features, All the rows in all files contain **3231961**  as second column value after transformation.

Note: As the preparation of data takes some time around 2-3 minutes, this step does not run every time if the data is already trasformed. If you need this to be run everytime then remove the condition if (Directory.GetFiles(transformedDataPath).Length == 0) inside PrepareDataset() method.

```CSharp
//Step 2:Prepare/Transofrm data
PrepareDataset(originalDataPath, transformedDataPath);
```

* Define the schema of dataset using **UrlData** class. 

```CSharp
public class UrlData
    {
        [LoadColumn(0)]
        public string LabelColumn;
        
        [LoadColumn(1, 3231961)]
        [VectorType(3231961)]
        public float[] FeatureVector;
    }
```
* Load the data into dataview using Text Loader.

```CSharp
var fullDataView = mlContext.Data.LoadFromTextFile<UrlData>(path: Path.Combine(transformedDataPath, "*"),
                                                      hasHeader: false,
                                                      allowSparse: true);
```                                               

* split the full dataview into 80-20 ratio to train and test data

```CSharp
//Step 4: Divide the whole dataset into 80% training and 20% testing data.
TrainTestData trainTestData = mlContext.Data.TrainTestSplit(fullDataView, testFraction: 0.2, seed: 1);
IDataView trainDataView = trainTestData.TrainSet;
IDataView testDataView = trainTestData.TestSet;
```
* ML.Net API accepts Label value in **Boolean** format. Our dataset contains Label value in **string** format. So map the string values of label into to boolean values.

```CSharp
//Step 5: Map label value from string to bool
var UrlLabelMap = new Dictionary<string, bool>();
UrlLabelMap["+1"] = true; //Malicious url
UrlLabelMap["-1"] = false; //Benign 
var dataProcessingPipeLine = mlContext.Transforms.Conversion.MapValue("LabelKey", UrlLabelMap, "LabelColumn");
```

* Choosing a trainer/learning algorithm (such as `FieldAwareFactorizationMachine`) to train the model with. 

```CSharp
//Step 6: Append trainer to pipeline
 var trainingPipeLine = dataProcessingPipeLine.Append(
                mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(labelColumnName: "LabelKey", featureColumnName: "FeatureVector")); 
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known sentiment values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training you need to call the `Fit()` method while providing the training dataset in a DataView object.

```CSharp
//Step 7: Train the model
ITransformer trainedModel = pipeline.Fit(trainDataView);
```

Note that ML.NET works with data with a lazy-load approach, so in reality no data is really loaded in memory until you actually call the method .Fit().

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`). This dataset also contains known sentiments. 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```CSharp
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
```

### 4. Consume model

After the model is trained, you can use the `Predict()` API check if a URL is malicious or benign. Here I have taken first 4 rows from the testDataView for prediction as it is difficult to create a sample data with millions of features manually.

```CSharp
// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<UrlData, UrlPrediction>(mlModel);                 

var sampleDatas = CreateSingleDataSample(mlContext, trainDataView);
foreach (var sampleData in sampleDatas)
{
    UrlPrediction predictionResult = predEngine.Predict(sampleData);
    Console.WriteLine($"Single Prediction --> Actual value: {sampleData.LabelColumn} | Predicted value: {predictionResult.Prediction}");
}

```


