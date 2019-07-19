# Image Classification - Asp.Net core Web/service Sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.1.0           | Dynamic API | up-to-date | Console app | Images and text labels | Images classification | TensorFlow model  | DeepLearning model |


## Problem
The problem is how to run/score a TensorFlow model in a web app/service while using in-memory images. 

## Solution:
The model (`model.pb`) is trained using TensorFlow as disscussed in the blogpost [Run with ML.NET C# code a TensorFlow model exported from Azure Cognitive Services Custom Vision](https://devblogs.microsoft.com/cesardelatorre/run-with-ml-net-c-code-a-tensorflow-model-exported-from-azure-cognitive-services-custom-vision/).

see the below architecture that shows how to run/score TensorFlow model in ASP.NET Core Razor web app/service

![](docs/scenario-architecture.png)


The difference between [getting started sample](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow) and this end-to-end sample is that the images are loaded from  **file** in getting started sample where as the images are loaded from **in-memory** in this end-to-end sample.

**Note:**  this sample is trained using Custom images and it predicts the only specific images that are in [TestImages](./TestImages) Folder.
