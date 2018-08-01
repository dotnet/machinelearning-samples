# Iris Classification
In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict the type of iris flower. In the world of machine learning, this type of prediction is known as **multiclass classification**.

## Problem
This problem is centered around predicting the type of an iris flower (setosa, versicolor, or virginica) based on the flower's parameters such as petal length, petal width, etc.

To solve this problem, we will build an ML model that takes as inputs 4 parameters: 
* petal length
* petal width
* sepal length
* sepal width

and predicts which iris type the flower belongs to:
* setosa
* versicolor
* virginica

To be precise, the model will return probabilities for the flower to belong to each type.

## ML task - Multiclass classification
The generalized problem of **multiclass classification** is to classify items into one of three or more classes. (Classifying items into one of the two classes is called **binary classification**).

Some other examples of multiclass classification are:
* handwriting digit recognition: predict which of 10 digits (0-9) an image contains.
* issues labeling: predict which category (UI, back end, documentation) an issue belongs to.
* disease stage prediction based on patient's test results.

The common feature for all those examples is that the parameter we want to predict can take one of a few (more that two) values. In other words, this value is represented by `enum`, not by `integer`, `float`/`double` or `boolean` types.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict an iris type.

![Build -> Train -> Evaluate -> Consume](https://github.com/dotnet/machinelearning-samples/raw/master/samples/getting-started/shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: uploading data (`iris-train.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `ColumnConcatenator`), and choosing a learning algorithm (`StochasticDualCoordinateAscentClassifier`). All of those steps are stored in a `LearningPipeline`:
```fsharp
// LearningPipeline holds all steps of the learning process: data, transforms, learners.
let pipeline = LearningPipeline()

// The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
// all the column names and their types.
pipeline.Add(TextLoader(TrainDataPath).CreateFrom<IrisData>())

//When ML model starts training, it looks for two columns: Label and //Features.
// Transforms
//              like in this example, no extra actions required.
// Label:   values that should be predicted. If you have a field named Label in your data type,
//          If you don’t have it, copy the column you want to predict with ColumnCopier transform:
//              new ColumnCopier(("FareAmount", "Label"))
// Features: all data used for prediction. At the end of all transforms you need to concatenate
//              all columns except the one you want to predict into Features column with
//              ColumnConcatenator transform:
pipeline.Add(ColumnConcatenator("Features",
                "SepalLength",
                "SepalWidth",
                "PetalLength",
                "PetalWidth"))
// StochasticDualCoordinateAscentClassifier is an algorithm that will be used to train the model.
pipeline.Add(StochasticDualCoordinateAscentClassifier())
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known iris types) to tune the parameters of the model. It is implemented in the `Train()` API. To perform training we just call the method and provide our data object  `IrisData` and  prediction object `IrisPrediction`.

```fsharp
let model = pipeline.Train<IrisData, IrisPrediction>()
```

### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`iris-test.txt`). This dataset also contains known iris types. `ClassificationEvaluator` calculates the difference between known types and values predicted by the model in various metrics.

```fsharp
let testData = TextLoader(TestDataPath).CreateFrom<IrisData>()

let evaluator = ClassificationEvaluator(OutputTopKAcc=3.0)
let metrics = evaluator.Evaluate(model, testData)
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

### 4. Consume model

After the model is trained, we can use the `Predict()` API to predict the probability that this flower belongs to each iris type. 

```fsharp
let prediction1 = model.Predict(TestIrisData.Iris1)
Console.WriteLine(sprintf "Actual: setosa.     Predicted probability: setosa:      %0.4f" prediction1.Score.[0])
Console.WriteLine(sprintf "                                           versicolor:  %0.4f" prediction1.Score.[1])
Console.WriteLine(sprintf "                                           virginica:   %0.4f" prediction1.Score.[2])
Console.WriteLine()
```

Where `TestIrisData.Iris1` stores the information about the flower we'd like to predict the type for.

```fsharp
module TestIrisData = 
    let Iris1 = IrisData(SepalLength = 5.1, SepalWidth = 3.3, PetalLength = 1.6, PetalWidth= 0.2)
    ...
```
