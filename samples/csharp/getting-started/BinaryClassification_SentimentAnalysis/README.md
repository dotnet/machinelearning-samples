# Sentiment Analysis for User Reviews

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.1.0          | Dynamic API | up-to-date | Console app | .tsv files | Sentiment Analysis | Two-class  classification | Linear Classification |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict a sentiment (positive or negative) for customer reviews. In the world of machine learning, this type of prediction is known as **binary classification**.

## Problem
This problem is centered around predicting if a customer's review has positive or negative sentiment. We will use small wikipedia-detox-datasets (one dataset for training and a second dataset for model's accuracy evaluation) that were processed by humans and each comment has been assigned a sentiment label: 
* 0 - nice/positive
* 1 - toxic/negative

Using those datasets we will build a model that when predicting it will analyze a string and predict a sentiment value of 0 or 1.

## ML task - Binary classification
The generalized problem of **binary classification** is to classify items into one of two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Define the data's schema maped to the datasets to load (`wikiDetoxAnnotated40kRows.tsv`) with a TextLoader. Split the dataset into train and test in ratio of 80:20

* Create an Estimator and transform the data to numeric vectors so it can be used effectively by an ML algorithm (with `FeaturizeText`)

* Choosing a trainer/learning algorithm (such as `SdcaLogisticRegression`) to train the model with. 

The initial code is similar to the following:

```cs --source-file ./SentimentAnalysis/SentimentAnalysisConsoleApp/Program.cs --project ./SentimentAnalysis/SentimentAnalysisConsoleApp/SentimentAnalysisConsoleApp.csproj --editable false  --region step1to3
// STEP 1: Common data loading configuration
IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(DataPath, hasHeader: true);

TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
IDataView trainingData = trainTestSplit.TrainSet;
IDataView testData = trainTestSplit.TestSet;

// STEP 2: Common data process configuration with pipeline data transformations          
var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

// STEP 3: Set the training algorithm, then create and config the modelBuilder                            
var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
var trainingPipeline = dataProcessPipeline.Append(trainer);
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known sentiment values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training you need to call the `Fit()` method while providing the training dataset in a DataView object.

```cs --source-file ./SentimentAnalysis/SentimentAnalysisConsoleApp/Program.cs --project ./SentimentAnalysis/SentimentAnalysisConsoleApp/SentimentAnalysisConsoleApp.csproj --editable false  --region step4
// STEP 4: Train the model fitting to the DataSet
ITransformer trainedModel = trainingPipeline.Fit(trainingData);
```

Note that ML.NET works with data with a lazy-load approach, so in reality no data is really loaded in memory until you actually call the method .Fit().

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (trainTestSplit.TestSet). This dataset also contains known sentiments. 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```cs --source-file ./SentimentAnalysis/SentimentAnalysisConsoleApp/Program.cs --project ./SentimentAnalysis/SentimentAnalysisConsoleApp/SentimentAnalysisConsoleApp.csproj --editable false  --region step5
// STEP 5: Evaluate the model and show accuracy stats
var predictions = trainedModel.Transform(testData);
var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
```

If you are not satisfied with the quality of the model, you can try to improve it by providing larger training datasets and by choosing different training algorithms with different hyper-parameters for each algorithm.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size so the training is quick. You should use bigger labeled sentiment datasets to significantly improve the quality of your models.*

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict the sentiment for new sample text. 

```cs --source-file ./SentimentAnalysis/SentimentAnalysisConsoleApp/Program.cs --project ./SentimentAnalysis/SentimentAnalysisConsoleApp/SentimentAnalysisConsoleApp.csproj --editable false  --region consume
// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(trainedModel);

// Score
var resultprediction = predEngine.Predict(sampleStatement);
```

Where in `resultprediction.PredictionLabel` will be either True or False depending if it is a Toxic or Non toxic predicted sentiment.


## Try it
This sample is compatible with [Try .NET](https://github.com/dotnet/try).  Follow [these instructions](https://github.com/dotnet/try#setup) to setup Try .NET, then `cd` to this directory and run `dotnet try` to run the sample.
```cs --source-file ./SentimentAnalysis/SentimentAnalysisConsoleApp/Program.cs --project ./SentimentAnalysis/SentimentAnalysisConsoleApp/SentimentAnalysisConsoleApp.csproj  --region try
```
