# Image Classification - Scoring sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0           | Dynamic API | up-to-date | Console app | Images and text labels | Images classification | Azure Cognitive Services Custom Vision  | DeepLearning model |


## Problem
In out getting started sample we have used pretrained Tensor Flow inception model to do image classification. The problem is, this model can be used only for category of images that the model is trained on. If you want to train model on your own images then this model may not work best to predict custom images. 

## Solution:
The solution is we use a **model from Azure Cognitive Services Custom Vision**. we train this custom model with our own images and then TensorFlow model is created. Then we use this Tensor flow model to predict images. For more details refer to this blog post [Run with ML.NET C# code a TensorFlow model exported from Azure Cognitive Services Custom Vision](https://devblogs.microsoft.com/cesardelatorre/run-with-ml-net-c-code-a-tensorflow-model-exported-from-azure-cognitive-services-custom-vision/)