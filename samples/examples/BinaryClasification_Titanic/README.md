# Titanic Passenger Survival Prediction

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict if a passenger from the Titanic survived or not. In the world of machine learning, this type of prediction is known as **binary classification**.

## Problem

This problem is centered around predicting if a passenger aboard the Titanic survived or not. We will use the data provided in the repo: [Real-World Machine Learning](https://github.com/brinkar/real-world-machine-learning/blob/master/data/titanic.csv) in which each passenger has been assigned a label:

* 0 - did not survive
* 1 - survived

Using those datasets we will build a model that will analyze a string and predict if a passenger survived.

## ML task - Binary classification

The generalized problem of **binary classification** is to classify items into one of two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution

To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict if passengers survived.

![Build -> Train -> Evaluate -> Consume](https://github.com/dotnet/machinelearning-samples/raw/master/samples/getting-started/shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: uploading data (`titanic-train.csv` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `CategoricalOneHotVectorizer`), and choosing a learning algorithm (`FastTreeBinaryClassifier`). All of those steps are stored in a `LearningPipeline`:

```CSharp
// LearningPipeline holds all steps of the learning process: data, transforms, learners.  
var pipeline = new LearningPipeline();

// The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
// all the column names and their types.
pipeline.Add(new TextLoader(TrainDataPath).CreateFrom<TitanicData>(useHeader: true, separator: ','));

// Transform any text feature to numeric values
pipeline.Add(new CategoricalOneHotVectorizer(
    "Sex",
    "Ticket",
    "Fare",
    "Cabin",
    "Embarked"));

// Put all features into a vector
pipeline.Add(new ColumnConcatenator(
    "Features",
    "Pclass",
    "Sex",
    "Age",
    "SibSp",
    "Parch",
    "Ticket",
    "Fare",
    "Cabin",
    "Embarked"));

// FastTreeBinaryClassifier is an algorithm that will be used to train the model.
// It has three hyperparameters for tuning decision tree performance.
pipeline.Add(new FastTreeBinaryClassifier() {NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2});
```

### 2. Train model

Training the model is a process of running the chosen algorithm on a training data (with known survival values) to tune the parameters of the model. It is implemented in the `Train()` API. To perform training we just call the method and provide the types for our data object `TitanicData` and  prediction object `TitanicPrediction`.

```CSharp
var model = pipeline.Train<TitanicData, TitanicPrediction>();
```

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`titanic-test.csv`). This dataset also contains known passenger data. `BinaryClassificationEvaluator` calculates the difference between known values and those predicted by the model in various metrics.

```CSharp
var testData = new TextLoader(TestDataPath).CreateFrom<TitanicData>(useHeader: true, separator: ',');
var evaluator = new BinaryClassificationEvaluator();
var metrics = evaluator.Evaluate(model, testData);
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

### 4. Consume model

After the model is trained, we can use the `Predict()` API to predict if a passenger survived.

```CSharp
var predictions = model.Predict(TestTitanicData.Passenger);
```

Where `TestTitanicData.Passenger` contains the passenger data we want to analyze.

```CSharp
internal static readonly TitanicData Passenger = new TitanicData()
    {
        Pclass = 3f,
        Name = "Braund, Mr. Owen Harris",
        Sex = "male",
        Age = 31,
        SibSp = 0,
        Parch = 0,
        Ticket = "335097",
        Fare = "7.75",
        Cabin = "",
        Embarked = "Q"
    };
```
