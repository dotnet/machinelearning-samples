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

![Build -> Train -> Evaluate -> Consume](../../../../../master/samples/csharp/getting-started/shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: uploading data (`iris-train.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `ColumnConcatenator`), and choosing a learning algorithm (`StochasticDualCoordinateAscentClassifier`). All of those steps are stored in a `LearningPipeline`:
```CSharp
            var env = new LocalEnvironment();
            string dataPath = "iris-data.txt";
            var reader = new TextLoader(env,
                            new TextLoader.Arguments()
                            {
                                Separator = ",",
                                HasHeader = true,
                                Column = new[]
                                {
                                     new TextLoader.Column("SepalLength", DataKind.R4, 0),
                                     new TextLoader.Column("SepalWidth", DataKind.R4, 1),
                                     new TextLoader.Column("PetalLength", DataKind.R4, 2),
                                     new TextLoader.Column("PetalWidth", DataKind.R4, 3),
                                     new TextLoader.Column("Label", DataKind.Text, 4)
                                }
                            });
            IDataView trainingDataView = reader.Read(new MultiFileSource(dataPath));

            // Transform your data and add a learner
            // Assign numeric values to text in the "Label" column, because only
            // numbers can be processed during model training.
            // Add a learning algorithm to the pipeline. e.g.(What type of iris is this?)
            // Convert the Label back into original text (after converting to number in step 3)
            var pipeline = new TermEstimator(env, "Label", "Label")
                   .Append(new ConcatEstimator(env, "Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth"))
                   .Append(new SdcaMultiClassTrainer(env, new SdcaMultiClassTrainer.Arguments()))
                   .Append(new KeyToValueEstimator(env, "PredictedLabel"));  
```
### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known iris types) to tune the parameters of the model. It is implemented in the `Train()` API. To perform training we just call the method and provide our data object  `IrisData` and  prediction object `IrisPrediction`.
```CSharp
            var model = pipeline.Fit(trainingDataView);
```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`iris-test.txt`). This dataset also contains known iris types. `ClassificationEvaluator` calculates the difference between known types and values predicted by the model in various metrics.
```CSharp
    var testData = new TextLoader(TestDataPath).CreateFrom<IrisData>();

    var evaluator = new ClassificationEvaluator {OutputTopKAcc = 3};
    var metrics = evaluator.Evaluate(model, testData);
```
>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.
### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the probability that this flower belongs to each iris type. 

```CSharp
             var prediction = model.MakePredictionFunction<IrisData, IrisPrediction>(env).Predict(
                new IrisData()
                {
                    SepalLength = 3.3f,
                    SepalWidth = 1.6f,
                    PetalLength = 0.2f,
                    PetalWidth = 5.1f,
                });

            Console.WriteLine($"Predicted flower type is: {prediction.PredictedLabels}");
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

