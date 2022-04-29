# Object Detection - ASP.NET Core Web & WPF Desktop Sample

| ML.NET version | API type    | Status     | App Type    | Data type   | Scenario         | ML Task       | Algorithms                        |
|----------------|-------------|------------|-------------|-------------|------------------|---------------|-----------------------------------|
| v1.6.0         | Dynamic API | Up-to-date | End-End app | image files | Image classification | Deep Learning | ONNX: Custom Vision |

## Problem

Image classification is one of the main applicatinos of deep learning by being able to classify what is in the image. For deep learning scenarios, you can either use a pre-trained model or train your own model. This sample uses an image classification model exported from [Custom Vision](https://www.customvision.ai).

## How the sample works

This sample consists of a single console application that builds an ML.NET pipeline from an ONNX model downnloaded from Custom Vision and predicts on any images in the "test" folder.

## ONNX

The Open Neural Network eXchange i.e [ONNX](http://onnx.ai/) is an open format to represent deep learning models. With ONNX, developers can move models between state-of-the-art tools and choose the combination that is best for them. ONNX is developed and supported by a community of partners, including Microsoft.

## Model input and output

In order to parse the prediction output of the ONNX model, we need to understand the format (or shape) of the input and output tensors.  To do this, we'll start by using [Netron](https://netron.app/), a GUI visualizer for neural networks and machine learning models, to inspect the model.

Below is an example of what we'd see upon opening this sample's model with Netron:

![Output from inspecting the Tiny YOLOv2 model with Netron](./assets/onnx-input.png)

From the output above, we can see the ONNX model has the following input/output formats:

### Input: 'data' 3x300x300

The first thing to notice is that the **input tensor's name** is **'data'**.  We'll need this name later when we define **input** parameter of the estimation pipeline.

We can also see that the or **shape of the input tensor** is **3x300x300**.  This tells that the image passed into the model should be 300 high x 300 wide. The '3' indicates the image(s) should be in BGR format; the first 3 'channels' are blue, green, and red, respectively.

### Output: 'data' 1x4

We can see **output's name** is **'data'**.  Again, we'll make note of that for when we define the **output** parameter of the estimation pipeline.

We can also see that the **shape of the output tensor** is **1x4**.

The '4' portion of 1x4 means that the output is of four items. This represents the number of classes that was input when creating the model within Custom Vision. In our case our classes are:

- cloudy
- rain
- sunrise
- sunshine

## Solution

**The projects in this solution uses .NET 6. In order to run this sample, you must install the .NET 6.0. To do this either:**

1. Manually install the SDK by going to [.NET Core 3.0 download page](https://aka.ms/netcore3download) and download the latest **.NET Core Installer** in the **SDK** column.
2. Or, if you're using Visual Studio 2019, go to: _**Tools > Options > Environment > Preview Features**_ and check the box next to: _**Use previews of the .NET Core SDK**_

## Code Walkthrough

Create a class that defines the data schema to use while loading data into an `IDataView`. ML.NET supports the `Bitmap` type for images, so we'll specify `Bitmap` property decorated with the `ImageTypeAttribute` and pass in the height and width dimensions we got by [inspecting the model](#model-input-and-output), as shown below.

```csharp
public class ImageInputData
{
    [ImageType("300", "300")]
    public Bitmap Image { get; set; }
}
```

### ML.NET: Configure the model

The first step is to create an empty `DataView` to obtain the schema of the data to use when configuring the model.

```csharp
var dataView = _mlContext.Data.LoadFromEnumerable(new List<WeatherRecognitionInput>());
```

Next, we can use the input and output tensor names we got by [inspecting the model](#model-input-and-output) to define the **input** and **output** parameters of the ONNX Model. We can use this information to define the estimator pipeline. Usually, when dealing with deep neural networks, you must adapt the images to the format expected by the network. For this reason, the code below resizes and transforms the images (pixel values are normalized across all R,G,B channels).

```csharp
var pipeline = context.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "data", imageWidth: 300, imageHeight: 300, inputColumnName: nameof(WeatherRecognitionInput.Image))
                .Append(context.Transforms.ExtractPixels(outputColumnName: "data"))
                .Append(context.Transforms.ApplyOnnxModel(modelFile: "./model/model.onnx", outputColumnName: "model_output", inputColumnName: "data"));
```

Last, create the model by fitting the `DataView`.

```csharp
var model = pipeline.Fit(dataView);
```

## Create a PredictionEngine

After the model is configured, create a `PredictionEngine`, and then pass the image to the engine to classify images using the model.

The **Console** app uses the `CreatePredictionEngine` to make predictions. Internally, it is optimized so the object dependencies are cached and shared across Http requests with minimum overhead when creating those objects.

```csharp
var predictionEngine = context.Model.CreatePredictionEngine<WeatherRecognitionInput, WeatherRecognitionPrediction>(model);
```

## Classify ans image

When obtaining the prediction, we get a `float` array in the `PredictedLabels` property. This is the 125x13x13 output of the model [discussed earlier](#output-data-125x13x13). For each test image we get the max value of the predicted label and its corresponding index. We use that index to get the label based on the **labels.txt** file.

```csharp
var labels = File.ReadAllLines("./model/labels.txt");

var testFiles = Directory.GetFiles("./test");

Bitmap testImage;

foreach (var image in testFiles)
{
    using (var stream = new FileStream(image, FileMode.Open))
    {
        testImage = (Bitmap)Image.FromStream(stream);
    }

    var prediction = predictionEngine.Predict(new WeatherRecognitionInput { Image = testImage });

    var maxValue = prediction.PredictedLabels.Max();
    var maxIndex = prediction.PredictedLabels.ToList().IndexOf(maxValue);

    var predictedLabel = labels[maxIndex];

    Console.WriteLine($"Prediction for file {image}: {predictedLabel}");
}```
