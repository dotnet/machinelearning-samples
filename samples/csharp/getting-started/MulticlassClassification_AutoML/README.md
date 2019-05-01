# Sentiment Analysis for User Reviews

| AutoML version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.3.0-preview          | Dynamic API | up-to-date | Console app | .tsv files | Sentiment Analysis | Two-class  classification | Linear Classification |

In this introductory sample, you'll see how to use automated ML to classify handwritten digits from 0 to 9 using the MNIST dataset. This is a **multiclass classification** problem that we will solve using SDCA (Stochastic Dual Coordinate Ascent) algorithm.

## Automated ML
AutoML eliminates the task of selecting different algorithms and hyperparameters. With AutoML, you just bring in your dataset and specify a few parameters. AutoML will do the rest i.e. data preprocessing, learning algorithm selection and hyperparameter selection to generate a high quality machine learning model that you can use for predictions.


## Problem
The MNIST data set contains handwritten images of digits, ranging from 0 to 9.

The MNIST dataset we are using contains 65 columns of numbers. The first 64 columns in each row are integer values in the range from 0 to 16. These values are calculated by dividing 32 x 32 bitmaps into non-overlapping blocks of 4 x 4. The number of ON pixels is counted in each of these blocks, which generates an input matrix of 8 x 8. The last column in each row is the number that is represented by the values in the first 64 columns. These first 64 columns are our features and our ML model will use these features to classify the testing images. The last column in our training and validation datasets is the label - the actual number that we will predict using our ML model.

the ML model that we will build will return probabilities for a given image of being one of the numbers from 0 to 9 as explained above.

## ML task - Multiclass classification
The generalized problem of **multiclass classification** is to classify items into one of three or more classes. (Classifying items into one of the two classes is called **binary classification**).

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Building a Machine Learning Model using AutoML

The general steps in building a model using AutoML are

1) Create necessary global variable to define the experiment

2) Define the data's schema mapped to the datasets to load the test and train data

3) Create a Machine Learning Experiment (currently Binary Classification, Multiclass Classification or Regression) by configuring set of parameters

4) Execute the experiment (Generates several models using the configuration settings you specified in Step 2)

5) Fetch the best model

6) Test and Deploy

### Step 1: Define Experiment Variables

Before the main method, create the global variable
```C#
private static uint ExperimentTime = 60;
```

### Step 2: Data loading
```C#
// STEP 1: Load the data
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

### Step 3: Building a Machine Learning Model using AutoML

Create an AutoML experiment by specifying experiment settings. We have already determined this sentiment analysis problem to be a Binary Classification problem. Next, we should specify how long the experiment should run and set a progress handler that will receive notifications as and when new models are trained.

```C#
// Progress handler be will invoked after each model it produces and evaluates.
var progressHandler = new MulticlassExperimentProgressHandler();

// Run an AutoML multiclass classification experiment
ExperimentResult<MulticlassClassificationMetrics> experimentResult = mlContext.Auto()
    .CreateMulticlassClassificationExperiment(ExperimentTime)
    .Execute(trainData, "Number", progressHandler: progressHandler);

```

### 4. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`). This dataset also contains known sentiments. 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```CSharp
RunDetail<MulticlassClassificationMetrics> bestRun = experimentResult.BestRun;
ITransformer trainedModel = bestRun.Model;

var predictions = trainedModel.Transform(testData);
var metrics = mlContext.MulticlassClassification.Evaluate(data:predictions, labelColumnName: "Number", scoreColumnName: "Score");

ConsoleHelper.PrintMulticlassClassificationMetrics(bestRun.TrainerName, metrics);
```

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict the sentiment for new sample text. 

```CSharp
// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(trainedModel);

//InputData data1 = SampleMNISTData.MNIST1;
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
Console.WriteLine($"                                           nine:  {resultprediction1.Score[9]:0.####}");
Console.WriteLine();

```

Where `SampleMNISTData.MNIST1` stores the pixel values of the digit that want to predict using the ML model.

```CSharp
class SampleMNISTData
{
	internal static readonly InputData MNIST1 = new InputData()
	{
		PixelValues = new float[] { 0, 0, 0, 0, 14, 13, 1, 0, 0, 0, 0, 5, 16, 16, 2, 0, 0, 0, 0, 14, 16, 12, 0, 0, 0, 1, 10, 16, 16, 12, 0, 0, 0, 3, 12, 14, 16, 9, 0, 0, 0, 0, 0, 5, 16, 15, 0, 0, 0, 0, 0, 4, 16, 14, 0, 0, 0, 0, 0, 1, 13, 16, 1, 0 }
	}; //num 1
    (...)
}
```
