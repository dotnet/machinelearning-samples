# Exporting an ML.NET model to ONNX

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.5.5           | Dynamic API | Up-to-date | Console app | .csv files | Price prediction | Regression  | Light GBM regression |

In this sample, you'll see how to use ML.NET to train a regression model and then convert that model to the ONNX format.

## Problem

The Open Neural Network Exchange i.e [ONNX](http://onnx.ai/) is an open format to represent deep learning models. With ONNX, developers can move models between state-of-the-art tools and choose the combination that is best for them. ONNX is developed and supported by a community of partners.

There may be times when you want to train a model with ML.NET and then convert to ONNX, for instance if you want to consume your model with WinML to take advantage of GPU inferencing in Windows applications.

Not all ML.NET models can be converted to ONNX; it is dependent on the trainers and transforms in the training pipeline. For a list of supported trainers see the tables in the ML.NET [Algorithms Doc](https://docs.microsoft.com/dotnet/machine-learning/how-to-choose-an-ml-net-algorithm) and for a list of supported transforms check out the [Data transforms Doc](https://docs.microsoft.com/dotnet/machine-learning/resources/transforms).

## Dataset

This sample uses the [NYC Taxi Fare dataset](https://github.com/dotnet/machinelearning-samples/blob/main/datasets/README.md#nyc-taxi-fare) for training.

## Solution

The console application project `ONNXExport` can be used to train an ML.NET model that predicts the price of taxi fare based on several features such as distance travelled and number of passengers, to export that model to ONNX, and then to consume the ONNX model and make predictions with it.

### NuGet Packages

To export an ML.NET model to ONNX, you must install the following NuGet packages in your project:

- Microsoft.ML.OnnxConverter

You must also install:

- Microsoft.ML for training the ML.NET model
- Microsoft.ML.ONNXRuntime and Microsoft.ML.OnnxTransformer for scoring the ONNX model

### Transforms and trainers

This pipeline contains the following transforms and trainers which are all ONNX exportable:

- OneHotEncoding transform
- Concatenate transform
- Light GBM trainer

### Code

After training an ML.NET model, you can use the following code to convert to ONNX:

```csharp
using (var stream = File.Create("taxi-fare-model.onnx"))
   mlContext.Model.ConvertToOnnx(model, trainingDataView, stream);
```

You need a transformer and input data to convert an ML.NET model to an ONNX model. By default, the ONNX conversion will generate the ONNX file with the latest OpSet version

After converting to ONNX, you can then consume the ONNX model with the following code:

```csharp
var onnxEstimator = mlContext.Transforms.ApplyOnnxModel(onnxModelPath);

using var onnxTransformer = onnxEstimator.Fit(trainingDataView);

var onnxOutput = onnxTransformer.Transform(testDataView);
```

You should get the same results when comparing the ML.NET model and ONNX model on the same sample input. If you run the project, you should get similar to the following output in the console:

```console
Predicted Scores with ML.NET model
Score      19.60645
Score      18.673796
Score      5.9175444
Score      4.8969507
Score      19.108932
Predicted Scores with ONNX model
Score      19.60645
Score      18.673796
Score      5.9175444
Score      4.8969507
Score      19.108932
```

## Performance

The default ONNX to ML.NET conversion is not optimal and produces extra graph outputs that are not needed for ML.NET usage. ONNX Runtime does reverse depth first search which results in a lot of conversion operations of native memory to managed memory from ONNX Runtime to ML.NET and execution of more than the necessary kernels. 

If you specify just the necessary graph outputs, it will only execute a subset of the graph. Thus, by eliminating all graph outputs except Score, you can improve inference performance.
