# Spam Detection for Text Messages

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1   | Dynamic API | Up-to-date | Console app | .tsv files | Spam detection | Two-class classification | SDCA (linear learner) |

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

```fsharp
// Set up the MLContext, which is a catalog of components in ML.NET.
let mlContext = MLContext(seed = Nullable 1)

let data = 
	mlContext.Data.LoadFromTextFile(trainDataPath,
		columns = 
			[|
				TextLoader.Column("LabelText" , DataKind.String, 0)
				TextLoader.Column("Message" , DataKind.String, 1)
			|],
		hasHeader = false,
		separatorChar = '\t')

// Create the estimator which converts the text label to a bool then featurizes the text, and add a linear trainer.
printfn "=============== Training the model ==============="
let estimator = 
	EstimatorChain()
		.Append(mlContext.Transforms.Conversion.MapValue("Label", dict ["ham", false; "spam", true], "LabelText"))
		.Append(mlContext.Transforms.Text.FeaturizeText("Features", "Message"))
		.AppendCacheCheckpoint(mlContext)
		.Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression("Label", "Features"))
```

### 2. Evaluate model

For this dataset, we will use [cross-validation](https://en.wikipedia.org/wiki/Cross-validation_(statistics)) to evaluate our model. This will partition the data into 5 'folds', train 5 models (on each combination of 4 folds), and test them on the fold that wasn't used in training.

```fsharp
let cvResults = mlContext.BinaryClassification.CrossValidate(data, downcastPipeline estimator, numberOfFolds = 5);
let avgAuc = cvResults |> Seq.map (fun x -> x.Metrics.AreaUnderRocCurve) |> Seq.average
printfn "The AUC is %.15f" avgAuc
```

Note that usually we evaluate a model after training it. However, cross-validation includes the model training part so we don't need to do `Fit()` first. However, we will later train the model on the full dataset to take advantage of the additional data.

### 3. Train model
To train the model we will call the estimator's `Fit()` method while providing the full training data.

```fsharp
let model = estimator.Fit(data)
```

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict whether new text is spam. In this case, we change the threshold of the model to get better predictions. We do this because our data is skewed with most messages not being spam.

```fsharp
// The dataset we have is skewed, as there are many more non-spam messages than spam messages.
// While our model is relatively good at detecting the difference, this skewness leads it to always
// say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
// it is useful to look at the precision-recall curve to identify the best possible threshold.
let classify = classifyWithThreshold 0.15f

// Create a PredictionFunction from our model 
let predictor = mlContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(model);

printfn "=============== Predictions for below data==============="
// Test a few examples
[
	"That's a great idea. It should work."
	"free medicine winner! congratulations"
	"Yes we should meet over the weekend!"
	"you win pills and free entry vouchers"
] 
|> List.iter (classify predictor)
```
