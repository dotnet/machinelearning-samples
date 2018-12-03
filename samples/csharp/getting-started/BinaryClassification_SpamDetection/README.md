# Spam Detection for Text Messages

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7           | Dynamic API | Might need to update project structure to match template | Console app | .tsv files | Spam detection | Two-class classification | SDCA (linear learner), also showing the CustomMapping estimator, which enables adding custom code to an ML.NET pipeline |

In this sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict whether a text message is spam. In the world of machine learning, this type of prediction is known as **binary classification**.

## Problem

Our goal here is to predict whether a text message is spam (an irrelevant/unwanted message). We will use the [SMS Spam Collection Data Set](https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection) from UCI, which contains close to 6000 messages that have been classified as being "spam" or "ham" (not spam). We will use this dataset to train a model that can take in new message and predict whether they are spam or not.

This is an example of binary classification, as we are classifying the text messages into one of two categories.

## Solution
To solve this problem, first we will build an estimator to define the ML pipeline we want to use. Then we will train this estimator on existing data, evaluate how good it is, and lastly we'll consume the model to predict whether a few examples messages are spam.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build estimator

To build the estimator we will:

* Define how to read the spam dataset that will be downloaded from https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection. 

* Apply several data transformations:

    * Convert the label ("spam" or "ham") to a boolean ("true" represents spam) so we can use it with a binary classifier. 
    * Featurize the text message into a numeric vector so a machine learning trainer can use it

* Add a trainer (such as `StochasticDualCoordinateAscent`).

The initial code is similar to the following:

```CSharp
// Set up the MLContext, which is a catalog of components in ML.NET.
var mlContext = new MLContext();

// Create the reader and define which columns from the file should be read.
var reader = new TextLoader(mlContext, new TextLoader.Arguments()
{
    Separator = "tab",
    HasHeader = true,
    Column = new[]
        {
            new TextLoader.Column("Label", DataKind.Text, 0),
            new TextLoader.Column("Message", DataKind.Text, 1)
        }
});

var data = reader.Read(new MultiFileSource(TrainDataPath));

// Create the estimator which converts the text label to boolean, featurizes the text, and adds a linear trainer.
var estimator = mlContext.Transforms.CustomMapping<MyInput, MyOutput>(MyLambda.MyAction, "MyLambda")
    .Append(mlContext.Transforms.Text.FeaturizeText("Message", "Features"))
    .Append(mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent());
```

### 2. Evaluate model

For this dataset, we will use [cross-validation](https://en.wikipedia.org/wiki/Cross-validation_(statistics)) to evaluate our model. This will partition the data into 5 'folds', train 5 models (on each combination of 4 folds), and test them on the fold that wasn't used in training.

```CSharp
var cvResults = mlContext.BinaryClassification.CrossValidate(data, estimator, numFolds: 5);
var aucs = cvResults.Select(r => r.metrics.Auc);
Console.WriteLine("The AUC is {0}", aucs.Average());
```

Note that usually we evaluate a model after training it. However, cross-validation includes the model training part so we don't need to do `Fit()` first. However, we will later train the model on the full dataset to take advantage of the additional data.

### 3. Train model
To train the model we will call the estimator's `Fit()` method while providing the full training data.

```CSharp
var model = estimator.Fit(data);
```

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict whether new text is spam. In this case, we change the threshold of the model to get better predictions. We do this because our data is skewed with most messages not being spam.

```CSharp
// The dataset we have is skewed, as there are many more non-spam messages than spam messages.
// While our model is relatively good at detecting the difference, this skewness leads it to always
// say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
// it is useful to look at the precision-recall curve to identify the best possible threshold.
var inPipe = new TransformerChain<ITransformer>(model.Take(model.Count() - 1).ToArray());
var lastTransformer = new BinaryPredictionTransformer<IPredictorProducing<float>>(mlContext, model.LastTransformer.Model, inPipe.GetOutputSchema(data.Schema), model.LastTransformer.FeatureColumn, threshold: 0.15f, thresholdColumn: DefaultColumnNames.Probability);

ITransformer[] parts = model.ToArray();
parts[parts.Length - 1] = lastTransformer;
var newModel = new TransformerChain<ITransformer>(parts);

// Create a PredictionFunction from our model 
var predictor = newModel.MakePredictionFunction<SpamInput, SpamPrediction>(mlContext);

var input = new SpamInput { Message = "free medicine winner! congratulations" };
Console.WriteLine("The message '{0}' is {1}", input.Message, predictor.Predict(input).isSpam ? "spam" : "not spam");

```
