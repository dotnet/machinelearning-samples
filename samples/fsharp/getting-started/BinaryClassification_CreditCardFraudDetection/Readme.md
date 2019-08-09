# Fraud detection in credit cards

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1 | Dynamic API | Up-to-date | Two console apps | .csv file | Fraud Detection | Two-class classification | FastTree Binary Classification |

In this introductory sample, you'll see how to use ML.NET to predict a credit card fraud. In the world of machine learning, this type of prediction is known as binary classification.

## Problem
This problem is centered around predicting if credit card transaction (with its related info/variables) is a fraud or no. 
 
The input information of the transactions contain only numerical input variables which are the result of PCA transformations. Unfortunately, due to confidentiality issues, the original features and additional background information are not available, but the way you build the model doesn't change.  

Features V1, V2, ... V28 are the principal components obtained with PCA, the only features which have not been transformed with PCA are 'Time' and 'Amount'. 

The feature 'Time' contains the seconds elapsed between each transaction and the first transaction in the dataset. The feature 'Amount' is the transaction Amount, this feature can be used for example-dependant cost-sensitive learning. Feature 'Class' is the response variable and it takes value 1 in case of fraud and 0 otherwise.

The dataset is highly unbalanced, the positive class (frauds) account for 0.172% of all transactions.

Using those datasets you build a model that when predicting it will analyze a transaction's input variables and predict a fraud value of false or true.

## DataSet

The training and testing data is based on a public [dataset available at Kaggle](https://www.kaggle.com/mlg-ulb/creditcardfraud) originally from Worldline and the Machine Learning Group (http://mlg.ulb.ac.be) of ULB (Université Libre de Bruxelles), collected and analysed during a research collaboration. 

The datasets contains transactions made by credit cards in September 2013 by european cardholders. This dataset presents transactions that occurred in two days, where we have 492 frauds out of 284,807 transactions.

By: Andrea Dal Pozzolo, Olivier Caelen, Reid A. Johnson and Gianluca Bontempi. Calibrating Probability with Undersampling for Unbalanced Classification. In Symposium on Computational Intelligence and Data Mining (CIDM), IEEE, 2015

More details on current and past projects on related topics are available on http://mlg.ulb.ac.be/BruFence and http://mlg.ulb.ac.be/ARTML

## ML Task - [Binary Classification](https://en.wikipedia.org/wiki/Binary_classification)

Binary or binomial classification is the task of classifying the elements of a given set into two groups (predicting which group each one belongs to) on the basis of a classification rule. Contexts requiring a decision as to whether or not an item has some qualitative property, some specified characteristic
  
## Solution

To solve this problem, first you need to build a machine learning model. Then you train the model on existing training data, evaluate how good its accuracy is, and lastly you consume the model (deploying the built model in a different app) to predict a fraud for a sample credit card transaction.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)


### 1. Build model
Building a model includes:

- Define the data's schema maped to the datasets to read with a DataReader

- Split data for training and tests

- Create an Estimator and transform the data with a ConcatEstimator() and Normalize by Mean Variance. 

- Choosing a trainer/learning algorithm (FastTree) to train the model with.


The initial code is similar to the following:

`````fsharp

    // Create a common ML.NET context.
    // Seed set to any number so you have a deterministic environment for repeateable results
    let seed = Nullable 1
    let mlContext = MLContext seed
[...]
    let classification = BinaryClassificationCatalog mlContext
 
[...]

    let trainData, testData = 
        printfn "Reading train and test data"
        let trainData = mlContext.Data.LoadFromTextFile<TransactionObservation>(trainFile, separatorChar = ',', hasHeader = true)
        let testData = mlContext.Data.LoadFromTextFile<TransactionObservation>(testFile, separatorChar = ',', hasHeader = true)
        trainData, testData

[...]

    let featureColumnNames = 
        trainData.Schema
        |> Seq.map (fun column -> column.Name)
        |> Seq.filter (fun name -> name <> "Time")
        |> Seq.filter (fun name -> name <> "Label")
        |> Seq.filter (fun name -> name <> "IdPreservationColumn")
        |> Seq.toArray

    let pipeline = 
        EstimatorChain()
        |> fun x -> x.Append(mlContext.Transforms.Concatenate("Features", featureColumnNames))
        |> fun x -> x.Append(mlContext.Transforms.DropColumns [|"Time"|])
        |> fun x -> 
            x.Append (
                mlContext.Transforms.NormalizeMeanVariance (
                    "FeaturesNormalizedByMeanVar", 
                    "Features"
                    )
                )
        |> fun x -> 
            x.Append (
                mlContext.BinaryClassification.Trainers.FastTree(
                    "Label", 
                    "FeaturesNormalizedByMeanVar", 
                    numberOfLeaves = 20, 
                    numberOfTrees = 100, 
                    minimumExampleCountPerLeaf = 10, 
                    learningRate = 0.2
                    )
                )

`````

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fraud values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`trainData.csv`) in a DataView object.

`````fsharp    
    let model = pipeline.Fit trainData
`````

### 3. Evaluate model
We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against another dataset that was not used in training (`testData.csv`). 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

`````fsharp
    let metrics = mlContext.BinaryClassification.Evaluate(model.Transform (testData), "Label")   
`````

### 4. Consume model
After the model is trained, you can use the `Predict()` API to predict if a transaction is a fraud, using a IDataSet.

`````fsharp
    printfn "Making predictions"
    mlContext.Data.CreateEnumerable<TransactionObservation>(testData, reuseRowObject = false)
    |> Seq.filter (fun x -> x.Label = true)
    // use 5 observations from the test data
    |> Seq.take 5
    |> Seq.iter (fun testData -> 
        let prediction = predictionEngine.Predict testData
        printfn "%A" prediction
        printfn "------"
        )
`````
