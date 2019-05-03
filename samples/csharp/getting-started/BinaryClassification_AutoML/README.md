# Sentiment Analysis for User Reviews

## Automated Machine Learning
Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model.

## Problem
This problem is to predict if a Wikipedia comment contains a personal attack or not. We will use small sample of the Wikipedia Detox dataset: one dataset for training and a second to test the model produced by AutoML. Human judges assigned every comment in these datasets a toxicity label:
* 0 - nice/positive
* 1 - toxic/negative

We will build a model that will analyze a string and predict a sentiment value of 0 or 1.

## ML Task - Binary Classification
The generalized problem of **binary classification** is to classify items into one of two classes (classifying items into more than two classes is called **multiclass classification**).

Some examples:
* predict if an insurance claim is valid or not
* predict if a plane will be delayed or will arrive on time
* predict if a face ID (photo) belongs to the owner of a device

For all these examples, the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Step 1: Load the Data

Load the datasets required to train and test:

```C#
 IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TrainDataPath, hasHeader: true);
 IDataView testDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TestDataPath, hasHeader: true);
```

## Step 2: Build a Machine Learning Model Using AutoML

Instantiate and run an AutoML experiment. In doing so, specify how long the experiment should run in seconds (`ExperimentTime`), and set a progress handler that will receive notifications after AutoML trains & evaluates each new model.

```C#
// Run AutoML binary classification experiment
ExperimentResult<BinaryClassificationMetrics> experimentResult = mlContext.Auto()
    .CreateBinaryClassificationExperiment(ExperimentTime)
    .Execute(trainingDataView, progressHandler: new BinaryExperimentProgressHandler());
```

## Step 3: Evaluate Model

Grab the best model produced by the AutoML experiment

```C#
ITransformer model = experimentResult.BestRun.Model;
```

and evaluate its quality on a test dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`).

`EvaluateNonCalibrated` compares the predicted values for the test dataset to the real values. It produces various metrics, such as accuracy:

```C#
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(predictions, scoreColumnName: "Score");
```

## Step 4: Make Predictions

Using the trained model, call the `Predict()` API to predict the sentiment for new sample text `sampleStatement`:

```C#
// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(model);

// Score
var predictedResult = predEngine.Predict(sampleStatement);
```

`predictedResult.PredictionLabel` will be either true or false. If true, the predicted sentiment is toxic; if false, non-toxic.
