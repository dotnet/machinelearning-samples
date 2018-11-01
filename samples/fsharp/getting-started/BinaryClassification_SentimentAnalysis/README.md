# Sentiment Analysis for User Reviews (F#)

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.6           | Dynamic API | Up-to date | Console app | .tsv files | Sentiment Analysis | Two-class  classification | Linear Classification |

------------------------------------

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict a sentiment (positive or negative) for customer reviews. In the world of machine learning, this type of prediction is known as **binary classification**.

## Problem
This problem is centered around predicting if a customer's review has positive or negative sentiment. We will use IMDB and Yelp comments that were processed by humans and each comment has been assigned a label: 
* 0 - negative
* 1 - positive

Using those datasets we will build a model that will analyze a string and predict a sentiment value of 0 or 1.

## ML task - Binary classification
The generalized problem of **binary classification** is to classify items into one of two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](../../../../../master/samples/csharp/getting-started/shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Define the data's schema maped to the datasets to read (`wikipedia-detox-250-line-data.tsv` and `wikipedia-detox-250-line-test.tsv`) with a DataReader

* Create an Estimator and transform the data to numeric vectors so it can be used effectively by an ML algorithm (with `TextTransform`)

* Choosing a trainer/learning algorithm (such as `LinearClassificationTrainer`) to train the model with. 

The initial code is similar to the following:

```fsharp
    //1. Create ML.NET context/environment
    use env = new LocalEnvironment()

    //2. Create DataReader with data schema mapped to file's columns
    let reader = 
        TextLoader(
            env, 
            TextLoader.Arguments(
                Separator = "tab", 
                HasHeader = true, 
                Column = 
                    [|
                        TextLoader.Column("Label", Nullable DataKind.Bool, 0)
                        TextLoader.Column("Text", Nullable DataKind.Text, 1)
                    |]
                )
            )

    //Load training data
    let trainingDataView = MultiFileSource(TrainDataPath) |> reader.Read

    printfn "=============== Create and Train the Model ==============="

    let pipeline = 
        env
        |> Pipeline.textTransform "Text" "Features"
        |> Pipeline.append (LinearClassificationTrainer(env, LinearClassificationTrainer.Arguments(), "Features", "Label"))
```
### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known sentiment values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. To perform training you need to call the Fit() method while providing the training dataset (wikipedia-detox-250-line-data.tsv file) in a DataView object.
```fsharp
    let model = 
        pipeline          
        |> Pipeline.fit trainingDataView
```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`). This dataset also contains known sentiments.

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```fsharp
    //5. Evaluate the model and show accuracy stats
    let testDataView = MultiFileSource(TestDataPath) |> reader.Read

    let predictions = model.Transform testDataView
    let binClassificationCtx = env |> BinaryClassificationContext
    let metrics = binClassificationCtx.Evaluate(predictions, "Label")

    printfn "Model quality metrics evaluation"
    printfn "------------------------------------------"
    printfn "Accuracy: %.2f%%" (metrics.Accuracy * 100.)
```
>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, you can try to improve it by providing larger training datasets and by choosing different training algorithms with different hyper-parameters for each algorithm.

>*Keep in mind that for this sample the quality is lower than it could be because the datasets were reduced in size so the training is quick. You should use bigger labeled sentiment datasets to significantly improve the quality of your models.*

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the sentiment for new reviews. 

```fsharp
    let predictionFunct = model.MakePredictionFunction<SentimentIssue, SentimentPrediction> env
    let sampleStatement = { Label = false; Text = "This is a very rude movie" }
    let resultprediction = predictionFunct.Predict sampleStatement
```
Where in `resultprediction.PredictionLabel` will be either 1 or 0 depending if it is a positive or negative predicted sentiment.
