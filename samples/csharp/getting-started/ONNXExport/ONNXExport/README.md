# Exporting an ML.NET model to ONNX

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.5.5           | Dynamic API | Up-to-date | Console app | .csv files | Price prediction | Regression  | Tiny Yolo2 ONNX model |

In this sample, you'll see how to use ML.NET to train a regression model and then convert that model to the ONNX format.

## Problem 
The Open Neural Network eXchange i.e [ONNX](http://onnx.ai/) is an open format to represent deep learning models. With ONNX, developers can move models between state-of-the-art tools and choose the combination that is best for them. ONNX is developed and supported by a community of partners.

There may be times when you want to train a model with ML.NET and then convert to ONNX, for instance if you want to consume your model with WinML to take advantage of GPU inferencing in Windows applications.

Not all ML.NET models can be converted to ONNX; it is dependent on the trainers and transforms in the training pipeline. For a list of supported trainers see the tables in the ML.NET [Algorithms Doc](https://docs.microsoft.com/dotnet/machine-learning/how-to-choose-an-ml-net-algorithm) and for a list of supported transforms check out the [Data transforms Doc](https://docs.microsoft.com/dotnet/machine-learning/resources/transforms).

 
## Dataset
This sample uses the [NYC Taxi Fare dataset](https://github.com/dotnet/machinelearning-samples/blob/main/datasets/README.md#nyc-taxi-fare) for training.


##  Solution
The console application project `ONNXExport` can be used to train an ML.NET model that X, export that model to ONNX, and then consume the ONNX model and make predictions with it. 

### NuGet Packages
To export an ML.NET model to ONNX, you must install the following NuGet packages in your project:

- Microsoft.ML.OnnxConverter
- Microsoft.ML.OnnxTransformer

You must also install:

- Microsoft.ML for training the ML.NET model
- Microsoft.ML.ONNXRuntime for scoring the ONNX model

### Transforms and trainers

This pipeline contains the following transforms and trainers which are all ONNX exportable:

- OneHotEncoding transform
- Concatenate transform
- Light GBM trainer