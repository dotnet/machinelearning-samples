# Iris Classification

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7           | Dynamic API | Up-to-date | Console app | .txt files | Iris flowers classification | Multi-class classification | Sdca Multi-class |

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

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 
* Uploading data (`iris-train.txt`) with `DataReader`)
* Create an Estimator and transform the data to one column so it can be used effectively by an ML algorithm (with `Concatenate`)
* Choosing a learning algorithm (`StochasticDualCoordinateAscent`). 


The initial code is similar to the following:
```CSharp
// Create MLContext to be shared across the model creation workflow objects 
// Set a random seed for repeatable/deterministic results across multiple trainings.
var mlContext = new MLContext(seed: 0);

// STEP 1: Common data loading configuration
var textLoader = IrisTextLoaderFactory.CreateTextLoader(mlContext);
var trainingDataView = textLoader.Read(TrainDataPath);
var testDataView = textLoader.Read(TestDataPath);

// STEP 2: Common data process configuration with pipeline data transformations
var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", "SepalLength",
                                                                       "SepalWidth",
                                                                       "PetalLength",
                                                                       "PetalWidth" );

// STEP 3: Set the training algorithm, then create and config the modelBuilder                            
var modelBuilder = new Common.ModelBuilder<IrisData, IrisPrediction>(mlContext, dataProcessPipeline);
// We apply our selected Trainer 
var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn: "Label", featureColumn: "Features");
modelBuilder.AddTrainer(trainer);
```
### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known iris types) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training we just call the method providing the training dataset (iris-train.txt file) in a DataView object.
```CSharp
// STEP 4: Train the model fitting to the DataSet            
modelBuilder.Train(trainingDataView);

[...]
public ITransformer Train(IDataView trainingData)
{
    TrainedModel = TrainingPipeline.Fit(trainingData);
    return TrainedModel;
}
```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`iris-test.txt`). This dataset also contains known iris types. `MulticlassClassification.Evaluate` calculates the difference between known types and values predicted by the model in various metrics.
```CSharp
var metrics = modelBuilder.EvaluateMultiClassClassificationModel(testDataView, "Label");
Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);
    
[...]
public MultiClassClassifierEvaluator.Result EvaluateMultiClassClassificationModel(IDataView testData, string label="Label", string score="Score")
{
    CheckTrained();
    var predictions = TrainedModel.Transform(testData);
    var metrics = _mlcontext.MulticlassClassification.Evaluate(predictions, label: label, score: score);
    return metrics;
}
```
>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.
### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the probability that this flower belongs to each iris type. 

```CSharp
var modelScorer = new Common.ModelScorer<IrisData, IrisPrediction>(mlContext);
modelScorer.LoadModelFromZipFile(ModelPath);

var prediction = modelScorer.PredictSingle(SampleIrisData.Iris1);
Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {prediction.Score[0]:0.####}");
Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");

[...]
public TPrediction PredictSingle(TObservation input)
{
    CheckTrainedModelIsLoaded();
    return PredictionFunction.Predict(input);
}
```
Where `TestIrisData.Iris1` stores the information about the flower we'd like to predict the type for.
```CSharp
internal class TestIrisData
{
    internal static readonly IrisData Iris1 = new IrisData()
    {
        SepalLength = 3.3f,
        SepalWidth = 1.6f,
        PetalLength = 0.2f,
        PetalWidth= 5.1f,
    }
    (...)
}
```
