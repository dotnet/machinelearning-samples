# Spike Detection of Shampoo Sales

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.11         | Dynamic API | Up-to-date | WinForms app | .csv files | Spike Detection of Shampoo sales | Anomaly Detection | IID Spike Detection |

![Alt Text](https://github.com/briacht/machinelearning-samples/raw/master/samples/csharp/end-to-end-apps/AnomalyDetection-SalesSpike-WinForms/ShampooSalesSpikeDetection/images/shampoosales.gif)

## Overview
Shampoo Sales Spike Detection is a simple application which builds and consumes a time series anomaly detection model to detect spikes in shampoo sales.

This is an end-to-end sample which shows how you can use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) and anomaly detection in a WinForms application.

## App Features
* This is an end-to-end WinForms App that will:
    1. Prompt the user to input a dataset file for model training (in this case we have provided `shampoo-sales.csv` that you can use)
    2. Prompt the user to indicate if the data in the file is separated by commas or tabs
    3. Prompt the user to provide a confidence level and p-value (if left blank, it will default to confidence level: 95, p-value: 9, which works well for the shampoo sales dataset)
    4. Display the data in a table format so that the user can inspect the data columns
    5. Display the data as a time series line graph
    6. Detect and display the anomalies both in a textual format and as markers in the line graph
* Time Series Anomaly Detection
    * This application builds a time series anomaly detection model using the Shampoo Sales dataset.
    * The model training code uses the [IidSpikeDetector](https://docs.microsoft.com/dotnet/api/microsoft.ml.transforms.timeseries.iidspikedetector?view=ml-dotnet).

### Dataset
The `shampoo-sales.csv` dataset is from [DataMart](https://datamarket.com/data/set/22r0/sales-of-shampoo-over-a-three-year-period#!ds=22r0&display=line).

You can try out other datasets, but note that the WinForms app works only with datasets that have headers (you can specify the character separator at runtime).

## Problem
This problem is focused on finding spikes in shampoo sales over a 3 year period, which can then be helpful in analyzing trends or abnormal behavior in sales.

To solve this problem, we will build an ML model that takes as inputs:
* Date (Year 1 - 3 and Month)
* Number of shampoo sales

and will generate an alert if/where a spike in shampoo sales is detected.

## ML task - Time Series Anomaly Detection
Anomaly detection is the process of detecting outliers in data. The goal of time series anomaly detection is the identification of rare items, events, or observations which raise suspicions by differing significantly from the majority of the time series data.

## Solution
To solve this problem, you build and train an ML model on existing data to demonstrate time series anomaly detection. The Prediction output column will then provide the Alerts where the model predicted the anomalies to be in the dataset.

### 1. Build model

Building a model includes:

* Preparing and loading the data from (`shampoo-sales.csv`) to an IDataView.

* Creating an Estimator by choosing a trainer/learning algorithm (in this case `DetectIIDSpike`) and setting parameters (in this case confidence level and p-value).

The initial code is similar to the following:

```CSharp
// Create MLContext object
var mlcontext = new MLContext();

// STEP 1: Common data loading configuration
IDataView dataView = mlcontext.Data.LoadFromTextFile<AnomalyExample>(path: filePath, hasHeader:true, separatorChar: commaSeparatedRadio.Checked ? ',' : '\t');

// Step 2: Set up the training algorithm
string outputColumnName = nameof(AnomalyPrediction.Prediction);
string inputColumnName = nameof(AnomalyExample.numReported);

var trainingPipeline = mlcontext.Transforms.IidSpikeEstimator(outputColumnName, inputColumnName, confidenceLevel, pValue);
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known anomaly values) to tune the parameters of the model. It is implemented in the `Fit()` API.

To perform training, you just call the `Fit()` method while providing the training dataset (`shampoo-sales.csv` file) in a DataView object.
```CSharp
// STEP 3: Train the model by fitting the dataview
ITransformer trainedModel = trainingPipeline.Fit(dataView);
```

### 3. Consume model & view predictions
You use the trained model to predict the anomalies in the data and then view the detected anomalies from the model by accessing the output column.

In this case the Prediction returns back a vector containing three values:
* 0 = Alert (0 for no alert, 1 for an alert)
* 1 = Score (value where the anomaly is detected e.g. number of sales)
* 2 = P-value (value used to measure how likely an anomoly is to be true vs. background noise)

```CSharp
// Step 4: Apply data transformation to create predictions
IDataView transformedData = trainedModel.Transform(dataView);

var predictions = mlcontext.Data.CreateEnumerable<AnomalyPrediction>(transformedData, reuseRowObject: false);
```
