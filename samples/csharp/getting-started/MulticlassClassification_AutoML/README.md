# MNIST Classification

## Automated ML
Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model.

## Problem
The MNIST data set contains handwritten images of digits, ranging from 0 to 9.

The MNIST dataset we are using contains 65 columns of numbers. The first 64 columns in each row are integer values in the range from 0 to 16. These values are calculated by dividing 32 x 32 bitmaps into non-overlapping blocks of 4 x 4. The number of ON pixels is counted in each of these blocks, which generates an input matrix of 8 x 8. The last column in each row is the number that is represented by the values in the first 64 columns. These first 64 columns are our features and our ML model will use these features to classify the testing images. The last column in our training and validation datasets is the label - the actual number that we will predict using our ML model.

The ML model that we will build will return probabilities for a given image being each of the numbers from 0 to 9 as explained above.

## ML Task - Multiclass Classification
The generalized problem of **multiclass classification** is to classify items into one of three or more classes. (Classifying items into one of the two classes is called **binary classification**.)

## Step 1: Load the Data

Load the datasets required to train and test:

```C#
var trainData = mlContext.Data.LoadFromTextFile(path: TrainDataPath,
        columns : new[] 
        {
            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
            new TextLoader.Column("Number", DataKind.Single, 64)
        },
        hasHeader : false,
        separatorChar : ','
        );

var testData = mlContext.Data.LoadFromTextFile(path: TestDataPath,
        columns: new[]
        {
            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
            new TextLoader.Column("Number", DataKind.Single, 64)
        },
        hasHeader: false,
        separatorChar: ','
        );
```

## Step 2: Build a Machine Learning Model Using AutoML

Instantiate and run an AutoML experiment. In doing so, specify how long the experiment should run in seconds (`ExperimentTime`), and set a progress handler that will receive notifications after AutoML trains & evaluates each new model.

```C#
// Run an AutoML multiclass classification experiment
ExperimentResult<MulticlassClassificationMetrics> experimentResult = mlContext.Auto()
    .CreateMulticlassClassificationExperiment(ExperimentTime)
    .Execute(trainData, "Number", progressHandler: new MulticlassExperimentProgressHandler());
```

## 3. Evaluate Model

Grab the best model produced by the AutoML experiment

```C#
ITransformer model = experimentResult.BestRun.Model;
```

and evaluate its quality on a test dataset that was not used in training (`optdigits-test.tsv`).

`Evaluate` compares the predicted values for the test dataset to the real values. It produces various metrics, such as accuracy:

```C#
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.MulticlassClassification.Evaluate(predictions, scoreColumnName: "Score");
```

## Step 4: Make Predictions

Using the trained model, call the `Predict()` API to predict the number drawn by a new set of pixels:

```C#
// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(trainedModel);

var resultprediction1 = predEngine.Predict(SampleMNISTData.MNIST1);

Console.WriteLine($"Actual: 7     Predicted probability:       zero:  {resultprediction1.Score[0]:0.####}");
Console.WriteLine($"                                           One :  {resultprediction1.Score[1]:0.####}");
Console.WriteLine($"                                           two:   {resultprediction1.Score[2]:0.####}");
Console.WriteLine($"                                           three: {resultprediction1.Score[3]:0.####}");
Console.WriteLine($"                                           four:  {resultprediction1.Score[4]:0.####}");
Console.WriteLine($"                                           five:  {resultprediction1.Score[5]:0.####}");
Console.WriteLine($"                                           six:   {resultprediction1.Score[6]:0.####}");
Console.WriteLine($"                                           seven: {resultprediction1.Score[7]:0.####}");
Console.WriteLine($"                                           eight: {resultprediction1.Score[8]:0.####}");
Console.WriteLine($"                                           nine:  {resultprediction1.Score[9]:F4}");
Console.WriteLine();

```

where `SampleMNISTData.MNIST1` stores the pixel values of the digit that want to predict using the ML model

```C#
class SampleMNISTData
{
	internal static readonly InputData MNIST1 = new InputData()
	{
		PixelValues = new float[] { 0, 0, 0, 0, 14, 13, 1, 0, 0, 0, 0, 5, 16, 16, 2, 0, 0, 0, 0, 14, 16, 12, 0, 0, 0, 1, 10, 16, 16, 12, 0, 0, 0, 3, 12, 14, 16, 9, 0, 0, 0, 0, 0, 5, 16, 15, 0, 0, 0, 0, 0, 4, 16, 14, 0, 0, 0, 0, 0, 1, 13, 16, 1, 0 }
	}; //num 1
    (...)
}
```
