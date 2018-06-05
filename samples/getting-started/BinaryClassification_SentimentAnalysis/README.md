# Sentiment Analysis for User Reviews
In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict a sentiment (positive or negative) for customer reviews. In the world of machine learning, this type of prediction is known as **binary classification**.

## Problem
This problem is centered around predicting if a customer's review has positive or negative sentiment. We will use IMDB and Yelp comments that were processed by humans and each comment has been assigned a label: 
* 0 - negative
* 1 - positive

Using those datasets we will build a model that will analyze a string and predict a sentiment value of 0 or 1.

## ML task - Binary classification
The generalized problem of **binary classification** is to classify items into one of two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](https://github.com/dotnet/machinelearning-samples/raw/master/samples/getting-started/shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: uploading data (`sentiment-imdb-train.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `TextFeaturizer`), and choosing a learning algorithm (`FastTreeBinaryClassifier`). All of those steps are stored in a `LearningPipeline`:
```CSharp
// LearningPipeline holds all steps of the learning process: data, transforms, learners.  
var pipeline = new LearningPipeline();
// The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
// all the column names and their types.
pipeline.Add(new TextLoader(TrainDataPath).CreateFrom<SentimentData>());
// TextFeaturizer is a transform that will be used to featurize an input column to format and clean the data.
pipeline.Add(new TextFeaturizer("Features", "SentimentText"));
// FastTreeBinaryClassifier is an algorithm that will be used to train the model.
// It has three hyperparameters for tuning decision tree performance. 
pipeline.Add(new FastTreeBinaryClassifier() {NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2});
```
### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known sentiment values) to tune the parameters of the model. It is implemented in the `Train()` API. To perform training we just call the method and provide the types for our data object `SentimentData` and  prediction object `SentimentPrediction`.
```CSharp
var model = pipeline.Train<SentimentData, SentimentPrediction>();
```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`sentiment-yelp-test.txt`). This dataset also contains known sentiments. `BinaryClassificationEvaluator` calculates the difference between known fares and values predicted by the model in various metrics.
```CSharp
    var testData = new TextLoader(TestDataPath).CreateFrom<SentimentData>();

    var evaluator = new BinaryClassificationEvaluator();
    var metrics = evaluator.Evaluate(model, testData);
```
>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size for performance purposes. You can use bigger labeled sentiment datasets available online to significantly improve the quality.*

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the sentiment for new reviews. 

```CSharp
var predictions = model.Predict(TestSentimentData.Sentiments);
```
Where `TestSentimentData.Sentiments` contains new user reviews that we want to analyze.

```CSharp
internal static readonly IEnumerable<SentimentData> Sentiments = new[]
{
    new SentimentData
    {
        SentimentText = "Contoso's 11 is a wonderful experience",
        Sentiment = 0
    },
    new SentimentData
    {
        SentimentText = "The acting in this movie is very bad",
        Sentiment = 0
    },
    new SentimentData
    {
        SentimentText = "Joe versus the Volcano Coffee Company is a great film.",
        Sentiment = 0
    }
};
```