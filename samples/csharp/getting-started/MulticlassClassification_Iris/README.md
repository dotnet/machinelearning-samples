# Iris Classification

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.1.0           | Dynamic API | Up-to-date | Console app | .txt files | Iris flowers classification | Multi-class classification | Sdca Multi-class |

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
var trainingDataView = mlContext.Data.LoadFromTextFile<IrisData>(TrainDataPath, hasHeader: true);
var testDataView = mlContext.Data.LoadFromTextFile<IrisData>(TestDataPath, hasHeader: true);

// STEP 2: Common data process configuration with pipeline data transformations
var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "KeyColumn", inputColumnName: nameof(IrisData.Label))
        .Append(mlContext.Transforms.Concatenate("Features", nameof(IrisData.SepalLength),
                                                            nameof(IrisData.SepalWidth),
                                                            nameof(IrisData.PetalLength),
                                                            nameof(IrisData.PetalWidth))
                                                            .AppendCacheCheckpoint(mlContext)); 
                                                            // Use in-memory cache for small/medium datasets to lower training time. 
                                                            // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets. 


// STEP 3: Set the training algorithm, then create and config the modelBuilder                         
var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "KeyColumn", featureColumnName: "Features")
            .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: nameof(IrisData.Label) , inputColumnName: "KeyColumn"));

var trainingPipeline = dataProcessPipeline.Append(trainer);
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known iris types) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training we just call the method providing the training dataset (iris-train.txt file) in a DataView object.

```CSharp
// STEP 4: Train the model fitting to the DataSet            

ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);

```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`iris-test.txt`). This dataset also contains known iris types. `MulticlassClassification.Evaluate` calculates the difference between known types and values predicted by the model in various metrics.

```CSharp
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score");

Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.
### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the probability that this flower belongs to each iris type. 

```CSharp

ITransformer trainedModel;
using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
{
    trainedModel = mlContext.Model.Load(stream);
}

// Create prediction engine related to the loaded trained model
var predEngine = trainedModel.CreatePredictionEngine<IrisData, IrisPrediction>(mlContext);

// During prediction we will get Score column with 3 float values.
// We need to find way to map each score to original label.
// In order to do what we need to get TrainingLabelValues from Score column.
// TrainingLabelValues on top of Score column represent original labels for i-th value in Score array.
// Let's look how we can convert key value for PredictedLabel to original labels.
// We need to read KeyValues for "PredictedLabel" column.
VBuffer<float> keys = default;
predEngine.OutputSchema["PredictedLabel"].GetKeyValues(ref keys);
var labelsArray = keys.DenseValues().ToArray();
// Since we apply MapValueToKey estimator with default parameters, key values
// depends on order of occurence in data file. Which is "Iris-setosa", "Iris-versicolor", "Iris-virginica"
// So if we have Score column equal to [0.2, 0.3, 0.5] that's mean what score for
// Iris-setosa is 0.2
// Iris-versicolor is 0.3
// Iris-virginica is 0.5.
//Add a dictionary to map the above float values to strings. 
Dictionary<float, string> IrisFlowers = new Dictionary<float, string>();
IrisFlowers.Add(0, "Setosa");
IrisFlowers.Add(1, "versicolor");
IrisFlowers.Add(2, "virginica");

Console.WriteLine("=====Predicting using model====");
//Score sample 1
var resultprediction1 = predEngine.Predict(SampleIrisData.Iris1);

Console.WriteLine($"Actual: setosa.     Predicted label and score: {IrisFlowers[labelsArray[0]]}:      {resultprediction1.Score[0]:0.####}");
Console.WriteLine($"                                           {IrisFlowers[labelsArray[1]]}:  {resultprediction1.Score[1]:0.####}"); Console.WriteLine($"                                           {IrisFlowers[labelsArray[2]]}:   {resultprediction1.Score[2]:0.####}");
Console.WriteLine();
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
