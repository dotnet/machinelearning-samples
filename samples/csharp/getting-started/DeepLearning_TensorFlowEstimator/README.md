# Image Classification Training (Model composition using TensorFlow Featurizer Estimator)

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4           | Dynamic API | Up-to-date | Console app | image files | Image classification | Featurization + Classification  | Deep neural network + LbfgsMaximumEntropy |
 
## Problem 
Image classification is a common problem which has been solved quite a while using Machine Learning techniques. In this sample, we will review an approach that mixes new techniques (deep learning) and old school (LbfgsMaximumEntropy) techniques.

In this model, we use the [Inception model](https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip) as a *featurizer* (the model is already stored in the [assets folder](./ImageClassification.Train/assets/inputs/inception/) ). This means that the model will process input images through the neural network, and then it will use the output of the tensor which precedes the classification. This tensor contains the *image features*, which allows to identify an image.

Finally, these image features will feed into an LbfgsMaximumEntropy algorithm/trainer which will learn how to classify different sets of image features.

## DataSet

The image set is downloaded 'on the fly' by the training console app.
The image set, after it is automatically unzziped, is composed by multiple image folders. Each sub-folder corresponds to a image class (flower types, in this case) you want to classify the future predictions and looks like this:
```
training-app-folder/assets/inputs/images/flower_photos
    daisy
    dandelion
    roses
    sunflowers
    tulips
```

The name of each sub-folder is important because it will be used as the label for the image classification.

> All images in this image set are licensed under the Creative Commons By-Attribution License, available at:
> https://creativecommons.org/licenses/by/2.0/
> When the image set is downloaded, a LICENSE.txt is also downloaded where you can see the full details of > the imageset license.

## ML Task - [Image Classification](https://en.wikipedia.org/wiki/Outline_of_object_recognition)
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to classify a new image.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 0. Image set download and preparation

That is boilerplate code that you can research which basically downloads a .zip file and uncompress it.
Once the image files are ready, you build/train the model with the following steps. 

### 1. Build Model
Building the model includes the following steps:
* Load the image paths and realted labels from the folders in the initial DataView
* Load the Images in-memory while transforming as needed by the TensorFlow pre-trained model used, such as InceptionV3. (resize and normalize pixel values, as required by the deep neural network used)
* Image *featurization* using the deep neural network model
* Image classification using LbfgsMaximumEntropy

Define the schema of data in a class type and refer that type while loading data using TextLoader. Here the class type is ImageNetData. 

```csharp
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
```

Load all the images into an initial DataView with the whole dataset by using the utility method LoadImagesFromDirectory():
```csharp
IEnumerable<ImageData> allImages = LoadImagesFromDirectory(folder: fullImagesetFolderPath,
                                                            useFolderNameasLabel: true);

```

Shuffle the images so the dataset will be better balanced by label classes before spliting in two datasets: Training and Test datasets.

```csharp
IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(imageSet);
IDataView shuffledFullImagesDataset = mlContext.Data.ShuffleRows(fullImagesDataset);

// Split the data 90:10 into train and test sets, train and evaluate.
TrainTestData trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.10);
IDataView trainDataView = trainTestData.TrainSet;
IDataView testDataView = trainTestData.TestSet;
```

The following step defines the training pipeline. Usually, when dealing with deep neural networks, you must adapt the images to the format expected by the network. This is the reason images are resized and then transformed.

```csharp
// 2. Load images in-memory while applying image transformations 
// Input and output column names have to coincide with the input and output tensor names of the TensorFlow model
// You can check out those tensor names by opening the Tensorflow .pb model with a visual tool like Netron: https://github.com/lutzroeder/netron
// TF .pb model --> input node --> INPUTS --> input --> id: "input" 
// TF .pb model --> Softmax node --> INPUTS --> logits --> id: "softmax2_pre_activation" (Inceptionv1) or "InceptionV3/Predictions/Reshape" (Inception v3)

var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelAsKey, inputColumnName: "Label")
                .Append(mlContext.Transforms.LoadImages(outputColumnName: "image_object", imageFolder: imagesFolder, inputColumnName: nameof(DataModels.ImageData.ImagePath)))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "image_object_resized", 
                                                            imageWidth: ImageSettingsForTFModel.imageWidth, imageHeight: ImageSettingsForTFModel.imageHeight, 
                                                            inputColumnName: "image_object"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName:"input", inputColumnName:"image_object_resized", 
                                                            interleavePixelColors:ImageSettingsForTFModel.channelsLast, 
                                                            offsetImage:ImageSettingsForTFModel.mean, 
                                                            scaleImage:ImageSettingsForTFModel.scale))  //for Inception v3 needs scaleImage: set to 1/255f. Not needed for InceptionV1. 
                .Append(mlContext.Model.LoadTensorFlowModel(inputTensorFlowModelFilePath).
                        ScoreTensorFlowModel(outputColumnNames: new[] { "InceptionV3/Predictions/Reshape" }, 
                                            inputColumnNames: new[] { "input" }, 
                                            addBatchDimensionInput: false));  // (For Inception v1 --> addBatchDimensionInput: true)  (For Inception v3 --> addBatchDimensionInput: false)
```

Finally, add the ML.NET classification trainer (LbfgsMaximumEntropy) to finalize the training pipeline:
```csharp
// Set the training algorithm and convert back the key to the categorical values (original labels)                            
var trainer = mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelAsKey, featureColumnName: "InceptionV3/Predictions/Reshape");  //"softmax2_pre_activation" for Inception v1
var trainingPipeline = dataProcessPipeline.Append(trainer)
                                            .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"));
```

### 2. Train model
In order to begin the training execute `Fit` on the built pipeline:
```csharp 
  ITransformer model = trainingPipeline.Fit(trainingDataView);
```


### 3. Evaluate model
After the training, we evaluate the model using the training data. The `Evaluate` function needs a `IDataView` as parameter which containers all the predictions using the test dataset split, so we apply `Transform` to the model, and then take the `AsDynamic` value.

```csharp
// Make bulk predictions and calculate quality metrics
ConsoleWriteHeader("Create Predictions and Evaluate the model quality");
IDataView predictionsDataView = model.Transform(testDataView);
           
// Show the performance metrics for the multi-class classification            
var classificationContext = mlContext.MulticlassClassification;
var metrics = classificationContext.Evaluate(predictionsDataView, labelColumnName: LabelAsKey, predictedLabelColumnName: "PredictedLabel");
ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);
```

Finally, we save the model:
```csharp
mlContext.Model.Save(model, predictionsDataView.Schema, outputMlNetModelFilePath);
```

#### Run the app to train the model

You should proceed as follows in order to train a model your model:
1) Set `ImageClassification.Train` as starting project in Visual Studio
2) Press F5 in Visual Studio. The training process will start and will take more or less time depending on how many images you are training with. 
3) After the training process has finished, in order to update the consumption app with the new trained model, you must copy/paste the generated ML.NET model file (assets/inputs/imageClassifier.zip) and paste it into the consumption app project (assets/inputs/MLNETModel) which simulates and end-user app which is only running the model for making predictions.



### 4. Model consumption code

First, you need to load the model created during Model training
```csharp
ITransformer loadedModel = mlContext.Model.Load(modelLocation,out var modelInputSchema);
```

Then, you create a predictor engine, and make a sample prediction:
```csharp
var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(loadedModel);

IEnumerable<ImageData> imagesToPredict = LoadImagesFromDirectory(imagesFolder, true);

//Predict the first image in the folder
//
ImageData imageToPredict = new ImageData
{
    ImagePath = imagesToPredict.First().ImagePath
};

var prediction = predictionEngine.Predict(imageToPredict);

Console.WriteLine("");
Console.WriteLine($"ImageFile : [{Path.GetFileName(imageToPredict.ImagePath)}], " +
                    $"Scores : [{string.Join(",", prediction.Score)}], " +
                    $"Predicted Label : {prediction.PredictedLabelValue}");

```
The prediction engine receives as parameter an object of type `ImageData` (containing 2 properties: `ImagePath` and `Label`). Then returns and object of type `ImagePrediction`, which holds the `PredictedLabel` and `Score` (*probability* value between 0 and 1) properties.

#### Model testing: making classifications
1) Copy the model produced by the training model (located at [ImageClassification.Train](./ImageClassification.Train/)/[assets](./ImageClassification.Train/assets/)/[outputs](./ImageClassification.Train/assets/outputs/)/[imageClassifier.zip](./ImageClassification.Train/assets/outputs/imageClassifier.zip) ) to the prediction project (at [ImageClassification.Predict](./ImageClassification.Predict/)/[assets](./ImageClassification.Predict/assets/)/[inputs](./ImageClassification.Predict/assets/inputs/)/[MLNETModel](./ImageClassification.Predict/assets/inputs/MLNETModel)/[imageClassifier.zip](./ImageClassification.Predict/assets/inputs/imageClassifier.zip) ).
2) Set VS default startup project: Set `ImageClassification.Predict` as starting project in Visual Studio. 
3) Press F5 in Visual Studio. After some seconds, the process will finish and show the predictions.
