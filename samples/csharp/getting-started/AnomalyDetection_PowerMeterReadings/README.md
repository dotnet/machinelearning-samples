# Power Consumption Anomaly Detection

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4           | Dynamic API | Up-to-date | Console app | .csv files | Power Meter Anomaly Detection | Time Series- Anomaly Detection | SsaSpikeDetection |

In this sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to detect anomalies in time series data.

## Problem
This problem is focused on finding spikes in power consumption based on daily readings from a smart electric meter.

To solve this problem, we will build an ML model that takes as inputs: 
* date and time
* meter reading difference, normalized by the time span between readings (ConsumptionDiffNormalized)

and generate an alert if an anomaly is detected.

## ML task - Time Series
The goal is the identification of rare items, events or observations which raise suspicions by differing significantly from the majority of the time series data.

## Solution
To solve this problem, you build and train an ML model on existing training data, evaluate how good it is (analyzing the obtained metrics), and lastly you can consume/test the model to predict the demand given input data variables.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

However, in this example we will build and train the model to demonstrate the Time Series anomaly detection library since it detects on actual data and does not have an evaluate method.  We will then review the detected anomalies in the Prediction output column.

### 1. Build model
Building a model includes:

- Prepare and Load the data with LoadFromTextFile

- Choosing a time series Estimator and setting parameters 


The initial code is similar to the following:

`````csharp

// Create a common ML.NET context.
var ml = new MLContext();

[...]

// Create a class for the dataset
class MeterData
{
    [LoadColumn(0)]
    public string name { get; set; }
    [LoadColumn(1)]
    public DateTime time { get; set; }
    [LoadColumn(2)]
    public float ConsumptionDiffNormalized { get; set; }
}

[...]

// Load the data
[...]

var dataView = ml.Data.LoadFromTextFile<MeterData>(
                TrainingData,
                separatorChar: ',',
                hasHeader: true);

[...]

// Prepare the Prediction output column for the model
class SpikePrediction
{
    [VectorType(3)]
    public double[] Prediction { get; set; }
}

[...]

// Configure the Estimator
const int PValueSize = 30;
const int SeasonalitySize = 30;
const int TrainingSize = 90;
const int ConfidenceInterval = 98;

string outputColumnName = nameof(SpikePrediction.Prediction);
string inputColumnName = nameof(MeterData.ConsumptionDiffNormalized);  

var trainigPipeLine = mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName,
                inputColumnName,
                confidence: ConfidenceInterval,
                pvalueHistoryLength: PValueSize,
                trainingWindowSize: TrainingSize,
                seasonalityWindowSize: SeasonalitySize);

`````

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known anomaly values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`power-export_min.csv`) in a DataView object.

`````csharp    
ITransformer trainedModel = trainigPipeLine.Fit(dataView);
`````

### 3. View the anomalies
You can view the detected anomalies from the Time Series model by accessing the output column.

`````csharp    
var transformedData = model.Transform(dataView);
`````
