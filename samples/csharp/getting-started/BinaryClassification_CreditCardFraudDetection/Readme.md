# Fraud detection in credit cards based on binary classification and existing PCA-transformed dataset

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0-preview           | Dynamic API | Up-to-date | Two console apps | .csv file | Fraud Detection | Two-class classification | FastTree Binary Classification |

In this introductory sample, you'll see how to use ML.NET to predict a credit card fraud. In the world of machine learning, this type of prediction is known as binary classification.

## API version: Dynamic and Estimators-based API
It is important to note that this sample uses the dynamic API with Estimators.

## Problem
This problem is centered around predicting if credit card transaction (with its related info/variables) is a fraud or no. 
 
The input dataset of the transactions contain only numerical input variables which are the result of previous PCA (Principal Component Analysis) transformations. Unfortunately, due to confidentiality issues, the original features and additional background information are not available, but the way you build the model doesn't change.  

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

- Preapre the data and split data for training and tests

- Load the data with TextLoader by speccifying the type name that holds data's schema to be mapped with datasets.

- Create an Estimator and transform the data with a Concatenate() and Normalize by Mean Variance. 

- Choosing a trainer/learning algorithm (FastTree) to train the model with.


The initial code is similar to the following:

`````csharp

    // Create a common ML.NET context.
    // Seed set to any number so you have a deterministic environment for repeateable results
    MLContext mlContext = new MLContext(seed:1);

[...]

// Prepare data and create Train/Test split datasets
    PrepDatasets(mlContext, fullDataSetFilePath, trainDataSetFilePath, testDataSetFilePath);

[...]

//Load the original single dataset
    IDataView originalFullData = mlContext.Data.LoadFromTextFile<TransactionObservation>(fullDataSetFilePath, separatorChar: er: true);
                 
    // Split the data 80:20 into train and test sets, train and evaluate.
    TrainTestData trainTestData = mlContext.Data.TrainTestSplit(originalFullData, testFraction: 0.2, seed: 1);
    IDataView trainData = trainTestData.TrainSet;
    IDataView testData = trainTestData.TestSet;

    
[...]

   //Get all the feature column names (All except the Label and the IdPreservationColumn)
    string[] featureColumnNames = trainDataView.Schema.AsQueryable()
        .Select(column => column.Name)                               // Get alll the column names
        .Where(name => name != nameof(TransactionObservation.Label)) // Do not include the Label column
        .Where(name => name != "IdPreservationColumn")               // Do not include the IdPreservationColumn/StratificationColumn
        .Where(name => name != "Time")                               // Do not include the Time column. Not needed as feature column
        .ToArray();

    // Create the data process pipeline
    IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { "Time" }))
                                            .Append(mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
                                                                                 outputColumnName: "FeaturesNormalizedByMeanVar"));

    // Set the training algorithm
    IEstimator<ITransformer> trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(TransactionObservation.Label),
                                            featureColumnName: "FeaturesNormalizedByMeanVar",
                                            numberOfLeaves: 20,
                                            numberOfTrees: 100,
                                            minimumExampleCountPerLeaf: 10,
                                            learningRate: 0.2);

`````

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fraud values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`trainData.csv`) in a DataView object.

`````csharp    
    ITransformer model = pipeline.Fit(_trainData);
`````

### 3. Evaluate model
We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against another dataset that was not used in training (`testData.csv`). 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

`````csharp
    EvaluateModel(mlContext, model, testDataView, trainerName);
`````

### 4. Consume model
After the model is trained, you can use the `Predict()` API to predict if a transaction is a fraud, using a IDataSet.

`````csharp
[...]

   ITransformer model;
   DataViewSchema inputSchema;
   using (var file = File.OpenRead(_modelfile))
   {
       model = mlContext.Model.Load(file, out inputSchema);
   }

   var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);

[...]

    mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                        .Where(x => x.Label == true)
                        .Take(numberOfPredictions)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData => 
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionEngine.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });
[...]

    mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                        .Where(x => x.Label == false)
                        .Take(numberOfPredictions)
                        .ToList()
                        .ForEach(testData =>
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionEngine.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });

`````
