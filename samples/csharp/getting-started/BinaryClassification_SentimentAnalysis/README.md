# Sentiment Analysis for User Reviews

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7           | Dynamic API | README.md updated | Console app | .tsv files | Sentiment Analysis | Two-class  classification | Linear Classification |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict a sentiment (positive or negative) for customer reviews. In the world of machine learning, this type of prediction is known as **binary classification**.

## Problem
This problem is centered around predicting if a customer's review has positive or negative sentiment. We will use small wikipedia-detox-datasets (one dataset for training and a second dataset for model's accuracy evaluation) that were processed by humans and each comment has been assigned a sentiment label: 
* 0 - nice/positive
* 1 - toxic/negative

Using those datasets we will build a model that when predicting it will analyze a string and predict a sentiment value of 0 or 1.

## ML task - Binary classification
The generalized problem of **binary classification** is to classify items into one of two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Define the data's schema maped to the datasets to read (`wikipedia-detox-250-line-data.tsv` and `wikipedia-detox-250-line-test.tsv`) with a DataReader

* Create an Estimator and transform the data to numeric vectors so it can be used effectively by an ML algorithm (with `FeaturizeText`)

* Choosing a trainer/learning algorithm (such as `FastTree`) to train the model with. 

The initial code is similar to the following:

```CSharp
// STEP 1: Common data loading configuration
TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                        {
                                            Separator = "tab",
                                            HasHeader = true,
                                            Column = new[]
                                                        {
                                                        new TextLoader.Column("Label", DataKind.Bool, 0),
                                                        new TextLoader.Column("Text", DataKind.Text, 1)
                                                        }
                                        });
IDataView trainingDataView = textLoader.Read(TrainDataPath);
IDataView testDataView = textLoader.Read(TestDataPath);

// STEP 2: Common data process configuration with pipeline data transformations          
var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Text", "Features");

// STEP 3: Set the training algorithm, then create and config the modelBuilder                            
var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumn: "Label", featureColumn: "Features");
var trainingPipeline = dataProcessPipeline.Append(trainer);
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known sentiment values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training you need to call the `Fit()` method while providing the training dataset (`wikipedia-detox-250-line-data.tsv` file) in a DataView object.

```CSharp
ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);
```

Note that ML.NET works with data with a lazy-load approach, so in reality no data is really loaded in memory until you actually call the method .Fit().

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`). This dataset also contains known sentiments. 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```CSharp
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label", "Score");

ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);
```

If you are not satisfied with the quality of the model, you can try to improve it by providing larger training datasets and by choosing different training algorithms with different hyper-parameters for each algorithm.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size so the training is quick. You should use bigger labeled sentiment datasets to significantly improve the quality of your models.*

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict the sentiment for new sample text. 

```CSharp
// Create prediction engine related to the loaded trained model
var predFunction= trainedModel.MakePredictionFunction<SentimentIssue, SentimentPrediction>(mlContext);

//Score
var resultprediction = predFunction.Predict(sampleStatement);
```

Where in `resultprediction.PredictionLabel` will be either True or False depending if it is a negative or positive predicted sentiment.
