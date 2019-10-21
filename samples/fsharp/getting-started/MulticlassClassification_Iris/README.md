# Iris Classification

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1 | Dynamic API | Up-to-date | Console app | .txt files | Iris flowers classification | Multi-class classification | Sdca Multi-class |

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
* Uploading data (`iris-train.txt` with `DataReader`)
* Create an Estimator and transform the data to one column so it can be used effectively by an ML algorithm (with `Concatenate`)
* Choosing a learning algorithm (`StochasticDualCoordinateAscent`). 


The initial code is similar to the following:
```fsharp
    // STEP 1: Common data loading configuration
    let trainingDataView = mlContext.Data.LoadFromTextFile<IrisData>(trainDataPath, hasHeader = true)
    let testDataView = mlContext.Data.LoadFromTextFile<IrisData>(testDataPath, hasHeader = true)

    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline = 
        EstimatorChain()
            .Append(mlContext.Transforms.Conversion.MapValueToKey("LabelKey","Label"))
            .Append(mlContext.Transforms.Concatenate("Features", "SepalLength",
                                                     "SepalWidth",
                                                     "PetalLength",
                                                     "PetalWidth"))
            .AppendCacheCheckpoint(mlContext)

    // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
    let trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName = "LabelKey", featureColumnName = "Features")
    let trainingPipeline = dataProcessPipeline.Append(trainer)
```
### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known iris types) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training we just call the method providing the training dataset (iris-train.txt file) in a DataView object.

```fsharp
    let trainedModel = trainingPipeline.Fit(trainingDataView)
```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`iris-test.txt`). This dataset also contains known iris types. `MulticlassClassification.Evaluate` calculates the difference between known types and values predicted by the model in various metrics.

```fsharp
    let metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score")

    Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metric
```
>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.
### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the probability that this flower belongs to each iris type. 

```fsharp
    // Create prediction engine related to the loaded trained model
    let predEngine = trainedModel.CreatePredictionEngine<IrisData, IrisPrediction>(mlContext)

    //Score sample 1
    let resultprediction1 = predEngine.Predict(DataStructures.SampleIrisData.Iris1)

    printfn "Actual: setosa.     Predicted probability: setosa:      %.4f" resultprediction1.Score.[0]
    printfn "                                           versicolor:  %.4f" resultprediction1.Score.[1]
    printfn "                                           virginica:   %.4f" resultprediction1.Score.[2]
    printfn ""
```
Where `SampleIrisData.Iris1` stores the information about the flower we'd like to predict the type for.
```fsharp
module SampleIrisData =
    let Iris1 = { IrisData.Empty with SepalLength = 5.1f; SepalWidth = 3.3f; PetalLength = 1.6f; PetalWidth= 0.2f}
    (...)
```
