# Object Detection

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4           | Dynamic API | Up-to-date | Console app | image files | Object Detection | Deep Learning  | Tiny Yolo2 ONNX model |


For a detailed explanation of how to build this application, see the accompanying [tutorial](https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/object-detection-onnx) on the Microsoft Docs site. 

## Problem 
Object detection is one of the classical problems in computer vision: Recognize what the objects are inside a given image and also where they are in the image. For these cases, you can either use pre-trained models or train your own model to classify images specific to your custom domain. 

 
## DataSet
The dataset contains images which are located in the [assets](./ObjectDetectionConsoleApp/assets/images) folder. These images are taken from [wikimedia commons site](https://commons.wikimedia.org/wiki/Main_Page). Go to [Wikimediacommon.md](./ObjectDetectionConsoleApp/assets/images/wikimedia.md) to refer to the image urls and their licenses.

## Pre-trained model
There are multiple models which are pre-trained for identifying multiple objects in the images. here we are using the pretrained model, **Tiny Yolo2** in  **ONNX** format. This model is a real-time neural network for object detection that detects 20 different classes. It is made up of 9 convolutional layers and 6 max-pooling layers and is a smaller version of the more complex full [YOLOv2](https://pjreddie.com/darknet/yolov2/) network.

The Open Neural Network eXchange i.e [ONNX](http://onnx.ai/) is an open format to represent deep learning models. With ONNX, developers can move models between state-of-the-art tools and choose the combination that is best for them. ONNX is developed and supported by a community of partners.

The model is downloaded from the [ONNX Model Zoo](https://github.com/onnx/models/tree/master/tiny_yolov2) which is a is a collection of pre-trained, state-of-the-art models in the ONNX format.

The Tiny YOLO2 model was trained on the [Pascal VOC](http://host.robots.ox.ac.uk/pascal/VOC/) dataset. Below are the model's prerequisites. 

**Model input and output**

**Input**

Input image of the shape (3x416x416)  

**Output**

Output is a (1x125x13x13) array   

**Pre-processing steps**

Resize the input image to a (3x416x416) array of type float32.

**Post-processing steps**

The output is a (125x13x13) tensor where 13x13 is the number of grid cells that the image gets divided into. Each grid cell corresponds to 125 channels, made up of the 5 bounding boxes predicted by the grid cell and the 25 data elements that describe each bounding box (5x25=125). For more information on how to derive the final bounding boxes and their corresponding confidence scores, refer to this [post](http://machinethink.net/blog/object-detection-with-yolo/).


##  Solution
The console application project `ObjectDetection` can be used to to identify objects in the sample images based on the **Tiny Yolo2 ONNX** model. 

Again, note that this sample only uses/consumes a pre-trained ONNX model with ML.NET API. Therefore, it does **not** train any ML.NET model. Currently, ML.NET supports only for scoring/detecting with existing ONNX trained models. 

You need to follow next steps in order to execute the classification test:

1) **Set VS default startup project:** Set `ObjectDetection` as starting project in Visual Studio.
2)  **Run the training model console app:** Hit F5 in Visual Studio. At the end of the execution, the output will be similar to this screenshot:
![image](./docs/Output/Console_output.png)


##  Code Walkthrough
There is a single project in the solution named `ObjectDetection`, which is responsible for loading the model in Tiny Yolo2 ONNX format and then detects objects in the images.

### ML.NET: Model Scoring

Define the schema of data in a class type and refer that type while loading data using TextLoader. Here the class type is **ImageNetData**. 

```csharp
public class ImageNetData
{
    [LoadColumn(0)]
    public string ImagePath;

    [LoadColumn(1)]
    public string Label;

    public static IEnumerable<ImageNetData> ReadFromFile(string imageFolder)
    {
        return Directory
            .GetFiles(imageFolder)
            .Where(filePath => Path.GetExtension(filePath) != ".md")
            .Select(filePath => new ImageNetData { ImagePath = filePath, Label = Path.GetFileName(filePath) });
    }
}
```

### ML.NET: Configure the model

The first step is to create an empty dataview as we just need schema of data while configuring up model.

```csharp
var data = mlContext.Data.LoadFromTextFile<ImageNetData>(imagesLocation, hasHeader: true);
```

The image file used to load images has two columns: the first one is defined as `ImagePath` and the second one is the `Label` corresponding to the image. 

It is important to highlight that the `Label` in the `ImageNetData` class is not really used when scoring with the Tiny Yolo2 Onnx model. It is used when to print the labels on the console. 

The second step is to define the estimator pipeline. Usually, when dealing with deep neural networks, you must adapt the images to the format expected by the network. This is the reason images are resized and then transformed (mainly, pixel values are normalized across all R,G,B channels).

```csharp
var pipeline = mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: "", inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));
```

You also need to check the neural network, and check the names of the input / output nodes. In order to inspect the model, you can use tools like [Netron](https://github.com/lutzroeder/netron), which is automatically installed with [Visual Studio Tools for AI](https://visualstudio.microsoft.com/downloads/ai-tools-vs/). 
These names are used later in the definition of the estimation pipe: in the case of the inception network, the input tensor is named 'image' and the output is named 'grid'

Define the **input** and **output** parameters of the Tiny Yolo2 Onnx Model.

```csharp
public struct TinyYoloModelSettings
{
    // for checking TIny yolo2 Model input and  output  parameter names,
    //you can use tools like Netron, 
    // which is installed by Visual Studio AI Tools

    // input tensor name
    public const string ModelInput = "image";

    // output tensor name
    public const string ModelOutput = "grid";
}
```

![inspecting neural network with netron](./docs/Netron/netron.PNG)

Finally, we return the trained model after *fitting* the estimator pipeline. 

```csharp
  var model = pipeline.Fit(data);
  return model;
```
When obtaining the prediction, we get an array of floats in the property `PredictedLabels`. The array is a float array of size **21125**. This is the output of model i,e 125x13x13 as discussed earlier. This output is interpreted by `YoloOutputParser` class and returns a number of bounding boxes for each image. Again these boxes are filtered so that we retrieve only 5 bounding boxes which have better confidence(how much certain that a box contains the obejct) for each object of the image. On console we display the label value of each bounding box.

# Detect objects in the image:

After the model is configured, we need to pass the image to the model to detect objects. When obtaining the prediction, we get an array of floats in the property `PredictedLabels`. The array is a float array of size **21125**. This is the output of model i,e 125x13x13 as discussed earlier. This output is interpreted by `YoloOutputParser` class and returns a number of bounding boxes for each image. Again these boxes are filtered so that we retrieve only 5 bounding boxes which have better confidence(how much certain that a box contains the obejct) for each object of the image. 

```csharp
IEnumerable<float[]> probabilities = modelScorer.Score(imageDataView);

YoloOutputParser parser = new YoloOutputParser();

var boundingBoxes =
    probabilities
    .Select(probability => parser.ParseOutputs(probability))
    .Select(boxes => parser.FilterBoundingBoxes(boxes, 5, .5F));
```

**Note** The Tiny Yolo2 model is not having much accuracy compare to full YOLO2 model. As this is a sample program we are using Tiny version of Yolo model i.e Tiny_Yolo2


