# Sentiment Analysis for User Reviews

| AutoML version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.3.0-preview          | Dynamic API | up-to-date | Console app | .tsv files | Sentiment Analysis | Two-class  classification | Linear Classification |

## Automated ML
Automated ML eliminates the task of selecting different algorithms and hyperparameters. With automated ML, you just bring in your dataset and specify a few parameters. Automated ML will do the rest i.e. data preprocessing, learning algorithm selection and hyperparameter selection to generate a high quality machine learning model that you can use for predictions.

In this introductory sample, you'll see how to use automated ML to predict a sentiment (positive or negative) for customer reviews. In the world of machine learning, this type of prediction is known as **binary classification**.

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

The general steps in building a model using AutoML are

1) Create necessary global variable to define the experiment

2) Define the data's schema mapped to the datasets to load the test and train data into IDataView's

3) Create a Machine Learning Experiment (currently Binary Classification, Multiclass Classification or Regression) by configuring set of parameters

4) Execute the experiment (Generates several models using the configuration settings you specified in Step 2)

5) Fetch the best model

6) Test and Deploy

### Step 1: Define Experiment Variables

Before the main method, create the global variable
```C#
private static uint ExperimentTime = 60;
```

### Step 2: Data loading

```C#
 IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TrainDataPath, hasHeader: true);
IDataView testDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TestDataPath, hasHeader: true);
```

### Step 3: Building a Machine Learning Model using AutoML

Create an AutoML experiment by specifying experiment settings. We have already determined this sentiment analysis problem to be a Binary Classification problem. Next, we should specify how long the experiment should run and set a progress handler that will receive notifications as and when new models are trained.

```C#
// Progress handler be will invoked after each model it produces and evaluates.
var progressHandler = new BinaryExperimentProgressHandler();

// Run AutoML binary classification experiment
ExperimentResult<BinaryClassificationMetrics> experimentResult = mlContext.Auto()
    .CreateBinaryClassificationExperiment(ExperimentTime)
    .Execute(trainingDataView, progressHandler: progressHandler);
```

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`). This dataset also contains known sentiments. 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```CSharp
var predictions = trainedModel.Transform(testData);
RunDetail<BinaryClassificationMetrics> bestRun = experimentResult.BestRun;
ITransformer trainedModel = bestRun.Model;
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(data:predictions, scoreColumnName: "Score");

ConsoleHelper.PrintBinaryClassificationMetrics(bestRun.TrainerName, metrics);
```

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict the sentiment for new sample text. 

```CSharp
// Create prediction engine related to the loaded trained model
var predEngine= mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(trainedModel);

//Score
var resultprediction = predEngine.Predict(sampleStatement);
```

Where in `resultprediction.PredictionLabel` will be either True or False depending if it is a Toxic or Non toxic predicted sentiment.
