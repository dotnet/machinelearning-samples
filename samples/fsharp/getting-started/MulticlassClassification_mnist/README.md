# MNIST Classification

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1           | Dynamic API | Up-to-date | Console app | .csv files | MNIST classification | Multi-class classification | Sdca Multi-class |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to classify handwritten digits from 0 to 9 using the MNIST dataset. This is a **multiclass classification** problem that we will solve using SDCA (Stochastic Dual Coordinate Ascent) algorithm.

## Problem

The MNIST data set contains handwritten images of digits, ranging from 0 to 9.

The MNIST dataset we are using contains 65 columns of numbers. The first 64 columns in each row are integer values in the range from 0 to 16. These values are calculated by dividing 32 x 32 bitmaps into non-overlapping blocks of 4 x 4. The number of ON pixels is counted in each of these blocks, which generates an input matrix of 8 x 8. The last column in each row is the number that is represented by the values in the first 64 columns. These first 64 columns are our features and our ML model will use these features to classifiy the testing images. The last column in our training and validation datasets is the label - the actual number that we will predict using our ML model.

the ML model that we will build will return probabilities for a given image of being one of the numbers from 0 to 9 as explained above.

## ML task - Multiclass classification
The generalized problem of **multiclass classification** is to classify items into one of three or more classes. (Classifying items into one of the two classes is called **binary classification**).

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a number the given image represents.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 
* Uploading data (`optdigits-train.csv`) with (`DataReader`)
* Create an Estimator and transform the data in the first 64 columns to one column so it can be used effectively by an ML algorithm (with `Concatenate`)
* Choosing a learning algorithm (`StochasticDualCoordinateAscent`). 


The initial code is similar to the following:
```fsharp
// STEP 1: Common data loading configuration
let trainData = mlContext.Data.LoadFromTextFile<Input>(trainDataPath, separatorChar=',', hasHeader=false)
let testData = mlContext.Data.LoadFromTextFile<Input>(testDataPath, separatorChar=',', hasHeader=false)

// STEP 2: Common data process configuration with pipeline data transformations
// Use in-memory cache for small/medium datasets to lower training time. Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.
let dataProcessPipeline = 
    EstimatorChain() 
        .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", "Number"))
        .Append(mlContext.Transforms.Concatenate("Features", "PixelValues"))
        .AppendCacheCheckpoint(mlContext)

// STEP 3: Set the training algorithm, then create and config the modelBuilder
let trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features")
let trainingPipeline = 
    dataProcessPipeline
        .Append(trainer)
        .Append(mlContext.Transforms.Conversion.MapKeyToValue("Number", "Label"))
```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data to tune the parameters of the model. Our training data consists of pixel values and the digit they represent. It is implemented in the `Fit()` method from the Estimator object. 

To perform training we just call the method providing the training dataset (optdigits-train.csv file) in a DataView object.

```fsharp
// STEP 4: Train the model fitting to the DataSet
printfn "=============== Training the model ==============="
let trainedModel = trainingPipeline.Fit(trainData)
```
### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`optdigits-val.csv`). `MulticlassClassification.Evaluate` calculates the difference between known types and values predicted by the model in various metrics.

```fsharp
let predictions = trainedModel.Transform(testData)
let metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Number", "Score")

Common.ConsoleHelper.printMultiClassClassificationMetrics (trainer.ToString()) metrics
```

>*To learn more on how to understand the metrics, check out the Machine Learning glossary from the [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) or use any available materials on data science and machine learning*.

If you are not satisfied with the quality of the model, there are a variety of ways to improve it, which will be covered in the *examples* category.

### 4. Consume model
After the model is trained, we can use the `Predict()` API to predict the probability of being correct digit.

```fsharp
let loadedTrainedModel, modelInputSchema = mlContext.Model.Load modelPath

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<Input, Output>(loadedTrainedModel)
            
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

Where `sampleData` stores the pixel values of the digit that want to predict using the ML model.

```fsharp
let sampleData = 
    [|
        7, {
            Number = 0.f
            PixelValues = [|0.f;0.f;0.f;0.f;14.f;13.f;1.f;0.f;0.f;0.f;0.f;5.f;16.f;16.f;2.f;0.f;0.f;0.f;0.f;14.f;16.f;12.f;0.f;0.f;0.f;1.f;10.f;16.f;16.f;12.f;0.f;0.f;0.f;3.f;12.f;14.f;16.f;9.f;0.f;0.f;0.f;0.f;0.f;5.f;16.f;15.f;0.f;0.f;0.f;0.f;0.f;4.f;16.f;14.f;0.f;0.f;0.f;0.f;0.f;1.f;13.f;16.f;1.f;0.f|]
        }
		//...
	|]
```
