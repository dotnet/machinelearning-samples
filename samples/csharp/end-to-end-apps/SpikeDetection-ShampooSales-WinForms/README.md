# Anomaly Detection of Shampoo Sales

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0-preview         | Dynamic API | Up-to-date | WinForms app | .csv files | Spike and Change Point Detection of Shampoo Sales | Anomaly Detection | IID Spike Detection and IID Change point Detection |

![Alt Text](./ShampooSalesAnomalyDetection/images/shampoosales.gif)

## Overview
Shampoo Sales Anomaly Detection is a simple application which builds and consumes time series anomaly detection models to detect [spikes](#spike-detection) and [change points](#change-point-detection) in shampoo sales.

This is an end-to-end sample which shows how you can use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) and anomaly detection in a WinForms application.

Note: This app is written in .NET Framework, so you must manually restore the nuget packages before running the app.

## App Features
* WinForms App:
    1. Prompts the user to input a dataset file for anomaly detection (in this case we have provided `shampoo-sales.csv` that you can use)
    2. Prompts the user to indicate if the data in the file is separated by commas or tabs
    3. Prompts the user to indicate if they want to see spikes or change points in the data
    4. Displays the data in a table format so that the user can inspect the data columns
    5. Displays the data as a time series line graph
    6. Loads the trained spike detection and change point detection models
    7. Uses the trained models to detect and display the anomalies both in a textual format, as markers in the line graph, and as highlighted rows in the data table

* Time Series Anomaly Detection Console App
    1. Builds and trains a time series anomaly detection model using the Shampoo Sales dataset for both spike detection and change point detection.
    2. Uses confidence level and p-value as algorithm hyperparameters.
    * Uses [IidSpikeDetector](https://docs.microsoft.com/dotnet/api/microsoft.ml.transforms.timeseries.iidspikedetector?view=ml-dotnet) and [IidChangePointDetector](https://docs.microsoft.com/dotnet/api/microsoft.ml.transforms.timeseries.iidchangepointdetector?view=ml-dotnet).

### Dataset
The `shampoo-sales.csv` dataset is from [DataMart](https://datamarket.com/data/set/22r0/sales-of-shampoo-over-a-three-year-period#!ds=22r0&display=line).

## Problem
This problem is focused on finding spikes and change points in shampoo sales over a 3 year period, which can then be helpful in analyzing trends or abnormal behavior in sales.

To solve this problem, we will build an ML model that takes as inputs:
* Date (Year 1 - 3 and Month)
* Number of shampoo sales

and will generate an alert if/where a spike or change point in shampoo sales is detected.

## ML task - Time Series Anomaly Detection
Anomaly detection is the process of detecting outliers in the data. Anomaly detection in time series refers to detecting time stamps, or points on a given input time series, at which the time series behaves differently from what was expected. These deviations are typically indicative of some events of interest in the problem domain: a cyber-attack on user accounts, power outage, bursting RPS on a server, memory leak, etc.

An anomalous behavior can be either persistent over time or just a temporary burst. There are 2 types of anomalies in this context: spikes and change points.

### Spike Detection
Spikes are attributed to sudden yet temporary bursts in the values of the input time-series. In practice, they can happen due to a variety of reasons depending on the application: outages, cyber-attacks, viral web content, etc.

### Change Point Detection
Change points mark the beginning of more persistent deviations in the behavior of time-series from what was expected. In practice, these type of changes are usually triggered by some fundamental changes in the dynamics of the system. For example, in system telemetry monitoring, an introduction of a memory leak can cause a (slow) trend in the time series of memory usage after certain point in time.

## Solution
To solve this problem, in your console app you build and train two ML models on existing data (shampoo sales) to demonstrate time series anomaly detection. You then use the model in the WinForms app, where the Prediction output columns provide the Alerts where the models predicted the anomalies (spikes or change points in shampoo sales) to be in the dataset.

The process of building and training models is the same for spike detection and change point detection; the main difference is the algorithm that you use (`IidSpikeDetector` vs. `IidChangePointDetector`).

### 1. Build model

Building a model in the console app includes:

* Preparing and loading the data from (`shampoo-sales.csv`) to an IDataView.

* Creating an Estimator by choosing a trainer/learning algorithm (e.g. `IidSpikeDetector` or `IidChangePointDetector`) and setting parameters (in this case confidence level and p-value).

The initial code for Spike Detection is similar to the following:

```CSharp
// Create MLContext object
var mlcontext = new MLContext();

// STEP 1: Common data loading configuration
IDataView dataView = mlcontext.Data.LoadFromTextFile<AnomalyExample>(path: filePath, hasHeader:true, separatorChar: commaSeparatedRadio.Checked ? ',' : '\t');

// Step 2: Set up the training algorithm
string outputColumnName = nameof(AnomalyPrediction.Prediction);
string inputColumnName = nameof(AnomalyExample.numReported);

var trainingPipeLine = mlcontext.Transforms.DetectIidSpike(outputColumnName: nameof(ShampooSalesPrediction.Prediction), inputColumnName: nameof(ShampooSalesData.numSales),confidence: 95, pvalueHistoryLength: size / 4);
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known anomaly values) to tune the parameters of the model. It is implemented in the `Fit()` API.

To perform training in the console app, you just call the `Fit()` method while providing the training dataset (`shampoo-sales.csv` file) in a DataView object:
```CSharp
// STEP 3: Train the model by fitting the dataview
ITransformer trainedModel = trainingPipeline.Fit(dataView);
```

### 3. Consume model & view predictions
In the WinForms app, you load and use the trained model to predict anomalies in the data and then view the detected anomalies from the model by accessing the output column:

```CSharp
var mlcontext = new MLContext();

ITransformer trainedModel;

// Load model
using (FileStream stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
{
    trainedModel = mlcontext.Model.Load(stream,out var modelInputSchema);
}

// Apply data transformation to create predictions
IDataView transformedData = trainedModel.Transform(dataView);

var predictions = mlcontext.Data.CreateEnumerable<ShampooSalesPrediction>(transformedData, reuseRowObject: false);

```

Each Prediction in `predictions` returns back a vector containing three values:
* 0 = Alert (0 for no alert, 1 for an alert)
* 1 = Score (value where the anomaly is detected e.g. number of sales)
* 2 = P-value (value used to measure how likely an anomoly is to be true vs. background noise)
