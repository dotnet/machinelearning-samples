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

```VB
' LearningPipeline holds all steps of the learning process: data, transforms, learners.  
' The TextLoader loads a dataset. The schema of the dataset Is specified by passing a class containing
' all the column names And their types.
' Transform any text feature to numeric values
' Put all features into a vector
' FastTreeBinaryClassifier Is an algorithm that will be used to train the model.
' It has three hyperparameters for tuning decision tree performance. 
Dim pipeline As New LearningPipeline From {
    New TextLoader(TrainDataPath).CreateFrom(Of TitanicData)(useHeader:=True, separator:=","c),
    New CategoricalOneHotVectorizer("Sex", "Ticket", "Fare", "Cabin", "Embarked"),
    New ColumnConcatenator("Features", "Pclass", "Sex", "Age", "SibSp", "Parch", "Ticket", "Fare", "Cabin", "Embarked"),
    New FastTreeBinaryClassifier With {
        .NumLeaves = 5,
        .NumTrees = 5,
        .MinDocumentsInLeafs = 2
    }
}
```

### 2. Train model

Training the model is a process of running the chosen algorithm on a training data (with known survival values) to tune the parameters of the model. It is implemented in the `Train()` API. To perform training we just call the method and provide the types for our data object `TitanicData` and  prediction object `TitanicPrediction`.

```VB
Dim model = pipeline.Train(Of TitanicData, TitanicPrediction)()
```

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`titanic-test.csv`). This dataset also contains known passenger data. `BinaryClassificationEvaluator` calculates the difference between known values and those predicted by the model in various metrics.

```VB
Dim testData = New TextLoader(TestDataPath).CreateFrom(Of TitanicData)(useHeader:=True, separator:=","c)
Dim evaluator = New BinaryClassificationEvaluator()
Dim metrics = evaluator.Evaluate(model, testData)
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

### 4. Consume model

After the model is trained, we can use the `Predict()` API to predict if a passenger survived.

```VB
Dim prediction = model.Predict(TestTitanicData.Passenger)
```

Where `TestTitanicData.Passenger` contains the passenger data we want to analyze.

```VB
Friend Class TestTitanicData
    Friend Shared ReadOnly Passenger As TitanicData = New TitanicData() With {
        .Pclass = 2,
        .Name = "Shelley, Mrs. William (Imanita Parrish Hall)",
        .Sex = "female",
        .Age = 25,
        .SibSp = 0,
        .Parch = 1,
        .Ticket = "230433",
        .Fare = "26",
        .Cabin = "",
        .Embarked = "S"
    }
End Class
```
