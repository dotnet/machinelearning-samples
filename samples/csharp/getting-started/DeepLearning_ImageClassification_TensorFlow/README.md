# Image Classification - Scoring sample

## Problem
Image classification is a common case in many business scenarios. For these cases, you can either use pre-trained models or train your own model to classify images specific to your custom domain. 

## Pre-trained model
There are multiple models are pre-trained for classifying images. In this case, we will use a model based on an Inception topology, and trained with images from Image.Net. This model can be downloaded from https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip, but it's also available at `/ src / ImageClassification / assets /inputs / inception / tensorflow_inception_graph.pb`.

##  Solution
The console application project `ImageClassification.Score` can be used to classify sample images based on the pre-trained Inception-v3 TensorFlow model. 

Again, note that this sample only uses/consumes a pre-trained TensorFlow model with ML.NET API. Therefore, it does **not** train any ML.NET model. Currently, TensorFlow is only supported in ML.NET for scoring/predicting with existing TensorFlow trained models. 

You need to follow next steps in order to execute the classification test:

1) **Set VS default startup project:** Set `ImageClassification.Score` as starting project in Visual Studio.
2)  **Run the training model console app:** Hit F5 in Visual Studio. At the end of the execution, the output will be similar to this screenshot:
![image](./docs/images/train_console.png)


##  Code Walkthrough
There is a single project in the solution named `ImageClassification.Score`, which is responsible for loading the model in TensorFlow format, and then classify images.

### ML.NET: Model Scoring
The `TextLoader.CreateReader()` is used to define the schema of the text file that will be used to load images in the ML.NET model.

```csharp
 var loader = new TextLoader(env,
    new TextLoader.Arguments
    {
        Column = new[] {
            new TextLoader.Column("ImagePath", DataKind.Text, 0)
        }
    });

var data = loader.Read(new MultiFileSource(dataLocation));
```

The image file used to load images has two columns: the first one is defined as `ImagePath` and the second one is the `Label` corresponding to the image. 

It is important to highlight that the label in this is not really used when scoring with the TensorFlow model. It is in this file only as a reference when testing the predictions so you can compare the actual label of each sample data with the predicted label provided by the TensorFlow model. That is why when loading the file with the 'TextLoader' above you just taking the ImagePath or name of the file but you are not taking the label.

```csv
broccoli.jpg	broccoli
bucket.png	bucket
canoe.jpg	canoe
snail.jpg	snail
teddy1.jpg	teddy bear
```
As you can observe, the file does not have a header row.

The second step is to define the estimator pipeline. Usually, when dealing with deep neural networks, you must adapt the images to the format expected by the network. This is the reason images are resized and then transformed (mainly, pixel values are normalized across all R,G,B channels).

```csharp
 var pipeline = new ImageLoaderEstimator(env, imagesFolder, ("ImagePath", "ImageReal"))
    .Append(new ImageResizerEstimator(env, "ImageReal", "ImageReal", ImageNetSettings.imageHeight, ImageNetSettings.imageWidth))
    .Append(new ImagePixelExtractorEstimator(env, new[] { new ImagePixelExtractorTransform.ColumnInfo("ImageReal", "input", interleave: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean) }))
    .Append(new TensorFlowEstimator(env, modelLocation, new[] { "input" }, new[] { "softmax2" }));

```
You also need to check the neural network, and check the names of the input / output nodes. In order to inspect the model, you can use tools like [Netron](https://github.com/lutzroeder/netron), which is automatically installed with [Visual Studio Tools for AI](https://visualstudio.microsoft.com/downloads/ai-tools-vs/). 
These names are used later in the definition of the estimation pipe: in the case of the inception network, the input tensor is named 'input' and the output is named 'softmax2'

![inspecting neural network with netron](./docs/images/netron.png)

Finally, we extract the prediction function after *fitting* the estimator pipeline. The prediction function receives as parameter an object of type `ImageNetData` (containing 2 properties: `ImagePath` and `Label`), and then returns and object of type `ImagePrediction`.  

```
 var modeld = pipeline.Fit(data);
 var predictionFunction = modeld.MakePredictionFunction<ImageNetData, ImageNetPrediction>(env);
```
When obtaining the prediction, we get an array of floats in the property `PredictedLabels`. Each position in the array is assigned to a label, so for example, if the model has 5 different labels, the array will be length = 5. Each position in the array represents the label's probability in that position; the sum of all array values (probabilities) is equal to one. Then, you need to select the biggest value (probability), and check which is the assigned label to that position.

### Citation
Training and prediction images 
> *Wikimedia Commons, the free media repository.* Retrieved 10:48, October 17, 2018 from https://commons.wikimedia.org/w/index.php?title=Main_Page&oldid=313158208.
