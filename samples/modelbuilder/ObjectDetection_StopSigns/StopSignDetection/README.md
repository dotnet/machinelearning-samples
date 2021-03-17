---
page_type: sample
name: "ML.NET Model Builder Stop Sign Object Detection"
description: "Use ML.NET Model Builder to train a deep learning object detection model in Azure."
urlFragment: "mlnet-object-detection-model-builder"
languages:
- csharp
products:
- dotnet
- dotnet-core
- vs
---

# ML.NET Model Builder Stop Sign Object Detection

This sample relates to the [ML.NET Model Builder Object Detection tutorial](https://docs.microsoft.com/dotnet/machine-learning/tutorials/object-detection-model-builder).

## Contents

| Project                     | Description                                                 |
|-----------------------------|-------------------------------------------------------------|
| `StopSignDetection`         | Console app that consumes the trained model.                |
| `StopSignDetectionML.Model` | Class library that contains the model and consumption code. |

The test image `test-image1.jpeg` is from [pexels](https://www.pexels.com/photo/red-stop-sign-39080/).

## Prerequisites

- Visual Studio 2019 16.6.1 or later
- .NET workload installed with ML.NET Model Builder component checked
- ML.NET Model Builder Preview Feature enabled

You can see Model Builder set up instructions in this [tutorial](https://dotnet.microsoft.com/learn/ml-dotnet/get-started-tutorial/install).

## Running the sample

You can either:

1. Follow along with the tutorial and have Model Builder generate the code for you, or
2. Clone this sample which already contains the necessary code and run the `StopSignDetection` console app to consume a trained ML.NET object detection model.
