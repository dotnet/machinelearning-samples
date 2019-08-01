# MNIST Classification

## Automated ML
Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model.

## Problem
The MNIST data set contains handwritten images of digits, ranging from 0 to 9.

The MNIST dataset we are using contains 65 columns of numbers. The first 64 columns in each row are integer values in the range from 0 to 16. These values are calculated by dividing 32 x 32 bitmaps into non-overlapping blocks of 4 x 4. The number of ON pixels is counted in each of these blocks, which generates an input matrix of 8 x 8. The last column in each row is the number that is represented by the values in the first 64 columns. These first 64 columns are our features and our ML model will use these features to classify the testing images. The last column in our training and validation datasets is the label - the actual number that we will predict using our ML model.

The ML model that we will build will return probabilities for a given image being each of the numbers from 0 to 9 as explained above.

## ML Task - Multiclass Classification
The generalized problem of **multiclass classification** is to classify items into one of three or more classes. (Classifying items into one of the two classes is called **binary classification**.)

## Step 1: Load the Data

Load the datasets required to train and test:

```fsharp
let load path =
	mlContext.Data.LoadFromTextFile(path=path,
		columns=
			[|
				TextLoader.Column("PixelValues", DataKind.Single, 0, 63)
				TextLoader.Column("Number", DataKind.Single, 64)
			|],
		hasHeader = false,
		separatorChar = ',')
let trainData = load trainDataPath
let testData = load testDataPath
```

## Step 2: Build a Machine Learning Model Using AutoML

Instantiate and run an AutoML experiment. In doing so, specify how long the experiment should run in seconds (`experimentTimeInSeconds`), and set a progress handler that will receive notifications after AutoML trains & evaluates each new model.

```fsharp
// Run an AutoML multiclass classification experiment
let experimentResult = mlContext.Auto().CreateMulticlassClassificationExperiment(experimentTimeInSeconds).Execute(trainData, "Number", progressHandler = progressHandler)
```

## 3. Evaluate Model

Grab the best model produced by the AutoML experiment

```fsharp
let model = experimentResult.BestRun.Model
```

and evaluate its quality on a test dataset that was not used in training (`optdigits-test.tsv`).

`Evaluate` compares the predicted values for the test dataset to the real values. It produces various metrics, such as accuracy:

```fsharp
let predictions = trainedModel.Transform(testDataView)
let metrics = mlContext.MulticlassClassification.Evaluate(predictions, scoreColumnName = "Score")
```

## Step 4: Make Predictions

Using the trained model, call the `Predict()` API to predict the number drawn by a new set of pixels:

```fharp
// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(loadedTrainedModel)

sampleData
|> Array.iter 
    (fun (n,dat) ->
        let p = predEngine.Predict dat
        printfn "Actual: %d     Predicted probability:       zero:  %.4f" n p.Score.[0]
        ["one:"; "two:"; "three:"; "four:"; "five:"; "six:"; "seven:"; "eight:"; "nine:"]
        |> List.iteri 
            (fun i w ->
                let i = i + 1
                printfn "                                           %-6s %.4f" w p.Score.[i]
            )
        printfn ""
    )
```

where `sampleData` stores the pixel values of the digit that want to predict using the ML model

```fsharp

let sampleData = 
    [|
        1, {
            Number = 0.f
            PixelValues = [|0.f;0.f;0.f;0.f;14.f;13.f;1.f;0.f;0.f;0.f;0.f;5.f;16.f;16.f;2.f;0.f;0.f;0.f;0.f;14.f;16.f;12.f;0.f;0.f;0.f;1.f;10.f;16.f;16.f;12.f;0.f;0.f;0.f;3.f;12.f;14.f;16.f;9.f;0.f;0.f;0.f;0.f;0.f;5.f;16.f;15.f;0.f;0.f;0.f;0.f;0.f;4.f;16.f;14.f;0.f;0.f;0.f;0.f;0.f;1.f;13.f;16.f;1.f;0.f|]
        //...
	|]
```
