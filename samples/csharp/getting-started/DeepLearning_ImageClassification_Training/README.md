# Image Classification Model Training - Preferred API (Based on native TensorFlow transfer learning)

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| Microsoft.ML.Dnn 0.16.0-preview | Dynamic API | Up-to-date | Console app | Image files | Image classification | Image classification with TensorFlow model retrain based on transfer learning  | InceptionV3 or ResNet |

## Problem 
Image classification is a common problem within the Deep Learning subject. This sample shows how to create your own custom image classifier by training your model based on the transfer learning approach which is basically retraining a pre-trained model (architecture such as InceptionV3 or ResNet) so you get a custom model trained on your own images.

In this sample app you create your own custom image classifier model by natively training a TensorFlow model from ML.NET API with your own images.

*Image classifier scenario – Train your own custom deep learning model with ML.NET* 
![](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2019/08/image-classifier-scenario.png)


## Dataset (Imageset)

> *Image set license*
>
> This sample's dataset is based on the 'flower_photos imageset' available from Tensorflow at [this URL](http://download.tensorflow.org/example_images/flower_photos.tgz). 
> All images in this archive are licensed under the Creative Commons By-Attribution License, available at:
https://creativecommons.org/licenses/by/2.0/
>
> The full license information is provided in the LICENSE.txt file which is included as part of the same image set downloaded as a .zip file.

The by default imageset being downloaded by the sample has 200 images evenly distributed across 5 flower classes:

    Images --> flower_photos_small_set -->       
               |
               daisy
               |
               dandelion
               |
               roses
               |
               sunflowers
               |
               tulips

The name of each sub-folder is important because that'll be the name of each class/label the model is going to use to classify the images. 

## ML Task - Image Classification

To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to classify a new image.

![](../shared_content/modelpipeline.png)

### 1. Build Model

Building the model includes the following steps:
* Loading the image files (file paths in this case) into an IDataView
* Image classification using the ImageClassification estimator (high level API)

Define the schema of data in a class type and refer that type while loading data. 
Here the data class type in this sample. 

```csharp
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
```

Download the imageset and load its information by using the LoadImagesFromDirectory() and LoadFromEnumerable().

```csharp
// 1. Download the image set and unzip
string finalImagesFolderName = DownloadImageSet(imagesDownloadFolderPath);
string fullImagesetFolderPath = Path.Combine(imagesDownloadFolderPath, finalImagesFolderName);

MLContext mlContext = new MLContext(seed: 1);

// 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: fullImagesetFolderPath, useFolderNameasLabel: true);
IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
IDataView shuffledFullImagesDataset = mlContext.Data.ShuffleRows(fullImagesDataset);
```

Once loaded into the IDataView, the rows are shuffled so the dataset is better balanced before spliting into the training/test datasets.

Now, the dataset is split in two datasets, one for training and the second for testing/validating the quality of the mode.

```csharp
// 3. Split the data 80:20 into train and test sets, train and evaluate.
TrainTestData trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
IDataView trainDataView = trainTestData.TrainSet;
IDataView testDataView = trainTestData.TestSet;
```

As the most important step, you define the model's training pipeline where you can see how easily you can train a new TensorFlow model which under the covers is based on transfer learning from a selected architecture (pre-trained model) such as *Inception v3* or *Resnet*.

```csharp
var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelAsKey", 
                                                                inputColumnName: "Label",
                                                                keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
            .Append(mlContext.Model.ImageClassification("ImagePath", "LabelAsKey",
                            arch: ImageClassificationEstimator.Architecture.InceptionV3,
                            epoch: 100,     
                            batchSize: 30,                                
                            metricsCallback: (metrics) => Console.WriteLine(metrics)));
```

The important line in the above code is the one using the `mlContext.Model.ImageClassification` classifier trainer which as you can see is a high level API where you just need to select the base pre-trained model to derive from, in this case [Inception v3](https://cloud.google.com/tpu/docs/inception-v3-advanced), but you can also select other pre-trained models such as [Resnet v2101](https://medium.com/@bakiiii/microsoft-presents-deep-residual-networks-d0ebd3fe5887). 

Those pre-trained models or architectures are the culmination of many ideas developed by multiple researchers over the years and you can easily take advantage of it now.

It is that simple, you don't even need to make image transformations (resize, normalizations, etc.). Depending on the selected architecture we need the required image transformations internaly so you simply need to use a single API.

### 2. Train model
In order to begin the training process you run `Fit` on the built pipeline:

```csharp 
// 4. Train/create the ML model
ITransformer trainedModel = pipeline.Fit(trainDataView);
```

### 3. Evaluate model

After the training, we evaluate the model's quality by using the test dataset. 

The `Evaluate` function needs an `IDataView` with the predictions generated from the test dataset by calling Transfor().

```csharp
// 5. Get the quality metrics (accuracy, etc.)
IDataView predictionsDataView = trainedModel.Transform(testDataset);

var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName:"LabelAsKey", predictedLabelColumnName: "PredictedLabel");
ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);
```

Finally, you save the model:
```csharp
// Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
mlContext.Model.Save(trainedModel, trainDataView.Schema, outputMlNetModelFilePath);
```

#### Run the project to train the model

You should proceed as follows in order to train a model your model:
1) Set `ImageClassification.Train` as starting project in Visual Studio
2) Press F5 in Visual Studio. After some seconds, the process will finish and you should have a new ML.NET model saved as the file `assets/outputs/imageClassifier.zip`

### 4. Consume model

In the sample's solution there's a second project named *ImageClassifcation.Predict*. That console app is simply loading your custom trained ML.NET model and performing a few sample predictions the same way a hypothetical end-user app could do.

First thing to do is to copy/paste the generated `assets/outputs/imageClassifier.zip` file into the *inputs/MLNETModel* folder of the consumption project.

In regards the code, you first need to load the model created during model training app execution.

```csharp
MLContext mlContext = new MLContext(seed: 1);
ITransformer loadedModel = mlContext.Model.Load(imageClassifierModelZipFilePath, out var modelInputSchema);
```

Then, your create a predictor engine object and finally make a few sample predictions by using the first image of the folder `assets/inputs//images-for-predictions` which has two images that were not used for training the model:

```csharp
var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(loadedModel);

IEnumerable<ImageData> imagesToPredict = LoadImagesFromDirectory(imagesForPredictions, true);

//Predict the first image in the folder
ImageData imageToPredict = new ImageData
{
    ImagePath = imagesToPredict.First().ImagePath
};

var prediction = predictionEngine.Predict(imageToPredict);

var index = prediction.PredictedLabel;

// Obtain the original label names to map through the predicted label-index
VBuffer<ReadOnlyMemory<char>> keys = default;
predictionEngine.OutputSchema["LabelAsKey"].GetKeyValues(ref keys);
var originalLabels = keys.DenseValues().ToArray();

Console.WriteLine($"ImageFile : [{Path.GetFileName(imageToPredict.ImagePath)}], " +
                    $"Scores : [{string.Join(",", prediction.Score)}], " +
                    $"Predicted Label : {originalLabels[index]}");
```

The prediction engine receives as parameter an object of type `ImageData` (containing 2 properties: `ImagePath` and `Label`). Then returns and object of type `ImagePrediction`, which holds the `PredictedLabel` (which is an index) and `Score` (*probability* value between 0 and 1) properties.

Since the `PredictedLabel` is just the predicted label's index, you need to find out the predicted label's name from the original values that you can obtain with the `OutputSchema` API, then extract the label's name which is text.

#### Run the "end-user-app" project to try predictions

You should proceed as follows in order to train a model your model:

1) Set `ImageClassification.Predict` as starting project in Visual Studio
2) Press F5 in Visual Studio. After some seconds, the process will show you predictions by loading and using your custom `imageClassifier.zip` model.

# TensorFlow Transfer Learning background

This sample app is retraining a TensorFlow model for image classification. As a user, you could think it is pretty similar to this other sample [Image classifier using the TensorFlow Estimator featurizer](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/DeepLearning_TensorFlowEstimator). However, the internal implementation is very different under the covers. In that mentioned sample, it is using a 'model composition approach' where an initial TensorFlow model (i.e. InceptionV3 or ResNet) is only used to featurize the images and produce the binary information per image to be used by another ML.NET classifier trainer added on top (such as `LbfgsMaximumEntropy`). Therefore, even when that sample is using a TensorFlow model, you are training only with a ML.NET trainer, you don't retrain a new TensorFlow model but train an ML.NET model. That's why the output of that sample is only an ML.NET model (.zip file).

In contrast, this sample is natively retraining a new TensorFlow model based on a Transfer Learning approach but training a new TensorFlow model derived from the specified pre-trained model (Inception V3 or ResNet).

The important difference is that this approach is internally retraining with TensorFlow APIs and creating a new TensorFlow model (.pb). Then, the ML.NET .zip file model you use is just like a wrapper around the new retrained TensorFlow model. This is why you can also see a new .pb file generated after training:

![](https://user-images.githubusercontent.com/1712635/64131693-26fa7680-cd7f-11e9-8010-89c60b71fe11.png)

In the screenshot below you can see how you can see that retrained TensorFlow model (`custom_retrained_model_based_on_InceptionV3.meta.pb`) in **Netron**, since it is a native TensorFlow model:

![](https://user-images.githubusercontent.com/1712635/64131904-9d4ba880-cd80-11e9-96a3-c2f936f8c5e0.png)

**Benefits:** 

- **Reuse across multiple frameworks and platforms:**
    This ultimately means that since you natively trained a Tensorflow model, in addition to being able to run/consume that model with the ML.NET 'wrapper' model (.zip file), you could also take the .pb TensorFlow frozen model and run it on any other framework such as Python/Keras/TensorFlow, or a Java/Android app or any framework that supports TensorFlow.
- **Flexibility and performace:** Since ML.NET is internally retraining natively on Tensorflow layers, the ML.NET team will be able to optimize further and take multiple approaches like training on the last layer or training on multiple layers across the TensorFlow model and achive better quality levels.
