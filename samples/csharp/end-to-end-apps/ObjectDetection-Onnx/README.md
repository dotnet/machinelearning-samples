# Object Detection - ASP.NET Core Web & WPF Desktop Sample

| ML.NET version | API type    | Status     | App Type    | Data type   | Scenario         | ML Task       | Algorithms            |
|----------------|-------------|------------|-------------|-------------|------------------|---------------|-----------------------|
| v1.3.1         | Dynamic API | Up-to-date | End-End app | image files | Object Detection | Deep Learning | Tiny Yolo2 ONNX model |

## Problem

Object detection is one of the classic problems in computer vision: Recognize what objects are inside a given image and also where they are in the image. For these cases, you can either use pre-trained models or train your own model to classify images specific to your custom domain.

## How the sample works

This sample consists of two separate apps:

- An ASP.NET Core Web App that allows the user to upload or select an image.  The WebApp then runs the image through an object detection model using ML.NET, and paints bounding boxes with labels indicating the objects detected.
- A WPF Core desktop app that renders a live-stream of the devices web cam, runs the video frames through an object detection model using ML.NET, and paints bounding boxes with labels indicating the objects detected in real-time.

The WebApp shows the images listed on the right, and each image may be selected to process. Once the image is processed, it is drawn in the middle of the screen with labeled bounding boxes around each detected object as shown below.

![Animated image showing object detection web sample](./docs/Screenshots/ObjectDetection.gif)

Alternatively you can try uploading your own images as shown below.

![Animated image showing object detection web sample](./docs/Screenshots/FileUpload.gif)

## Pre-trained model

There are multiple pre-trained models for identifying multiple objects in the images. Both the **WPF app** and the **Web app** use the pre-trained model, **Tiny Yolo2** in [**ONNX**](http://onnx.ai/) format. This model is a real-time neural network for object detection that detects 20 different classes. It is made up of 9 convolutional layers and 6 max-pooling layers and is a smaller version of the more complex full [YOLOv2](https://pjreddie.com/darknet/yolov2/) network.

The Open Neural Network eXchange i.e [ONNX](http://onnx.ai/) is an open format to represent deep learning models. With ONNX, developers can move models between state-of-the-art tools and choose the combination that is best for them. ONNX is developed and supported by a community of partners, including Microsoft.

The model is downloaded from the [ONNX Model Zoo](https://github.com/onnx/models/tree/master/tiny_yolov2) which is a is a collection of pre-trained, state-of-the-art models in the ONNX format.

The Tiny YOLO2 model was trained on the [Pascal VOC](http://host.robots.ox.ac.uk/pascal/VOC/) dataset. Below are the model's prerequisites.

### Model input and output

- **Input:** An image of the shape (3x416x416)  
- **Output:** An (1x125x13x13) array

### Pre-processing steps

Resize the input image to an (3x416x416) array of type `float32`.

### Post-processing steps

The output is a (125x13x13) tensor where 13x13 is the number of grid cells that the image gets divided into. Each grid cell corresponds to 125 channels, made up of the 5 bounding boxes predicted by the grid cell and the 25 data elements that describe each bounding box (5x25=125). For more information on how to derive the final bounding boxes and their corresponding confidence scores, refer to this [post](http://machinethink.net/blog/object-detection-with-yolo/).

## Solution

**The projects in this solution use .NET Core 3.0.  In order to run this sample, you must install the .NET Core SDK 3.0.  To do this either:**

1. Manually install the SDK by going to [.NET Core 3.0 download page](https://aka.ms/netcore3download) and download the latest **.NET Core Installer** in the **SDK** column.
2. Or, if you're using Visual Studio 2019, go to: _**Tools > Options > Environment > Preview Features**_ and check the box next to: _**Use previews of the .NET Core SDK**_

### The solution contains three projects

- [**OnnxObjectDetection**](./OnnxObjectDetection) is a .NET Standard library used by both the WPF app and the Web app.  It contains most of the logic for running images trough the model and parsing the resulting prediction.  This project also contains the Onnx model file.  With the exception of drawing the labels bounding boxes on the image/screen, all of the following code snippets are contained in this project.
- [**OnnxObjectDetectionWeb**](./OnnxObjectDetectionWeb) contains an ASP.NET Core Web App that that contains both **Razor UI pages** and an **API controller** to process and render images.
- [**OnnxObjectDetectionApp**](./OnnxObjectDetectionApp) contains an .NET CORE WPF Desktop App that uses [OpenCvSharp](https://github.com/shimat/opencvsharp) to capture the video from the devices webcam.

## Code Walkthrough

_This sample differs from the [getting-started object detection sample](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/DeepLearning_ObjectDetection_Onnx) in that here we load/process the images **in-memory** whereas the getting-started sample loads the images from a **file**._

Create a class that defines the data schema to use while loading data into an `IDataView`. ML.NET supports the `Bitmap` type for images, so we'll specify `Bitmap` property decorated with the `ImageTypeAttribute`, as shown below.

```csharp
public class ImageInputData
{
    [ImageType(416, 416)]
    public Bitmap Image { get; set; }
}
```

### ML.NET: Configure the model

The first step is to create an empty DataView as we just need schema of data while configuring up model.

```csharp
var dataView = _mlContext.Data.LoadFromEnumerable(new List<ImageInputData>());
```

The second step is to define the estimator pipeline. Usually when dealing with deep neural networks, you must adapt the images to the format expected by the network. For this reason, the code below resizes and transforms the images (pixel values are normalized across all R,G,B channels).

```csharp
var pipeline = _mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image))
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                .Append(_mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModelFilePath, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));
```

You also need to inspect the neural network to get the names of the input and output nodes. To do this, you can use tools like [Netron](https://github.com/lutzroeder/netron), which is automatically installed with [Visual Studio Tools for AI](https://visualstudio.microsoft.com/downloads/ai-tools-vs/).
These names are used later when we define the estimation pipeline: in the case of the Tiny Yolo2 network, the input tensor is named **'image'** and the output is named **'grid'**.

We'll use these to define the **input** and **output** parameters of the Tiny Yolo2 Onnx Model.

```csharp
public struct TinyYoloModelSettings
{
    // for checking TIny yolo2 Model input and output parameter names,
    // you can use tools like Netron, which is installed by Visual Studio AI Tools

    // input tensor name
    public const string ModelInput = "image";

    // output tensor name
    public const string ModelOutput = "grid";
}
```

![inspecting neural network with netron](./docs/Netron/netron.PNG)

Create the model by fitting the DataView.

```csharp
var model = pipeline.Fit(dataView);
```

## Load model and create PredictionEngine

After the model is configured, we need to save the model, load the saved model, create a `PredictionEngine`, and the pass the image to the engine to detect objects using the model.
This is one place that the **Web** app and the **WPF** app differ slightly.  The **Web** app uses a `PredicitonEnginePool` to efficiently manage and provide the service with a `PredictionEngine` to use to make predictions.

```csharp
public ObjectDetectionService(PredictionEnginePool<ImageInputData, ImageObjectPrediction> predictionEngine)
{
    this.predictionEngine = predictionEngine;
}
```

Whereas the **WPF** desktop app creates a single `PredictionEngine` and caches locally to be used for each frame prediction.

```csharp
public PredictionEngine<ImageInputData, ImageObjectPrediction> GetMlNetPredictionEngine()
{
    return _mlContext.Model.CreatePredictionEngine<ImageInputData, ImageObjectPrediction>(_mlModel);
}
```

## Detect objects in the image

When obtaining the prediction, we get an `float` array of size **21125** in the `PredictedLabels` property. This is the 125x13x13 output of the model discussed earlier. We then use the `YoloOutputParser` class to interpret and returns a number of bounding boxes for each image. Again these boxes are filtered so that we retrieve only 5 with high confidence.

```csharp
var labels = predictionEngine.Predict(imageInputData).PredictedLabels;
var boundingBoxes = yoloParser.ParseOutputs(labels);
var filteredBoxes = yoloParser.FilterBoundingBoxes(boundingBoxes, 5, 0.5f);
```

## Draw bounding boxes around detected objects in Image

The final step is to draw the bounding boxes around the objects.

The **Web** app draws the boxes directly onto the image using Paint API and returns the image to display it in the browser.

```csharp
var img = _objectDetectionService.DrawBoundingBox(imageFilePath);

using (MemoryStream m = new MemoryStream())
{
   img.Save(m, img.RawFormat);
   byte[] imageBytes = m.ToArray();

   // Convert byte[] to Base64 String
   base64String = Convert.ToBase64String(imageBytes);
   var result = new Result { imageString = base64String };
   return result;
}
```

Alternatively, the **WPF** app draws the bounding boxes on a [`Canvas`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.canvas?view=netcore-3.0) element that overlaps the streaming video playback.

```csharp
DrawOverlays(filteredBoxes, WebCamImage.ActualHeight, WebCamImage.ActualWidth);

WebCamCanvas.Children.Clear();

foreach (var box in filteredBoxes)
{
    var objBox = new Rectangle {/* ... */ };

    var objDescription = new TextBlock {/* ... */ };

    var objDescriptionBackground = new Rectangle {/* ... */ };

    WebCamCanvas.Children.Add(objDescriptionBackground);
    WebCamCanvas.Children.Add(objDescription);
    WebCamCanvas.Children.Add(objBox);
}
```

## Note on accuracy

Tiny YOLO2 is significantly less accurate than the full YOLO2 model, but the tiny version is sufficient for this sample app.
