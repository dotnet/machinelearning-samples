# Fraud detection in credit cards (based on anomaly/outlier detection)

| ML.NET version | API type    | Status     | App Type         | Data type | Scenario        | ML Task           | Algorithms     |
|----------------|-------------|------------|------------------|-----------|-----------------|-------------------|----------------|
| v1.4         | Dynamic API | Up-to-date | Two console apps | .csv file | Fraud Detection | Anomaly Detection | Randomized PCA |

In this introductory sample, you'll see how to use ML.NET to predict a credit card fraud. In the world of machine learning, this type of prediction is known as anomaly (or outlier) detection.
  

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
  

## ML Task - [Anomaly Detection](https://en.wikipedia.org/wiki/Anomaly_detection)

Anomaly (or outlier) detection is the identification of rare items, events or observations which raise suspicions by differing significantly from the majority of the data. Typically the anomalous items will translate to some kind of problem such as bank fraud, a structural defect, medical problems or errors in a text. 

If you would like to learn how to detect fraud using binary classification, visit the [Binary Classification Credit Card Fraud Detection sample](../BinaryClassification_CreditCardFraudDetection).  

## Solution

To solve this problem, first you need to build a machine learning model. Then you train the model on existing training data, evaluate how good its accuracy is, and lastly you consume the model (deploying the built model in a different app) to predict a fraud for a sample credit card transaction.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)


### 1. Build model

Building a model includes:

- Prepare the data and split data for training and tests.

- Load the data with TextLoader by specifying the type name that holds data's schema to be mapped with datasets.

- Create an Estimator and transform the data with a `Concatenate()` and Normalize by LP Norm. 

- Choosing a trainer/learning algorithm Randomized PCA to train the model with.


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

    // Get all the feature column names (All except the Label and the IdPreservationColumn)
    string[] featureColumnNames = trainDataView.Schema.AsQueryable()
        .Select(column => column.Name)                               // Get alll the column names
        .Where(name => name != nameof(TransactionObservation.Label)) // Do not include the Label column
        .Where(name => name != "IdPreservationColumn")               // Do not include the IdPreservationColumn/StratificationColumn
        .Where(name => name != nameof(TransactionObservation.Time))  // Do not include the Time column. Not needed as feature column
        .ToArray();

    // Create the data process pipeline
    IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { nameof(TransactionObservation.Time) }))
                                            .Append(mlContext.Transforms.NormalizeLpNorm(outputColumnName: "NormalizedFeatures",
                                                                                          inputColumnName: "Features"));


    // In Anomaly Detection, the learner assumes all training examples have label 0, as it only learns from normal examples.
    // If any of the training examples has label 1, it is recommended to use a Filter transform to filter them out before training:
    IDataView normalTrainDataView = mlContext.Data.FilterRowsByColumn(trainDataView, columnName: nameof(TransactionObservation.Label), lowerBound: 0, upperBound: 1);

[...]

    var options = new RandomizedPcaTrainer.Options
    {
        FeatureColumnName = "NormalizedFeatures",   // The name of the feature column. The column data must be a known-sized vector of Single.
        ExampleWeightColumnName = null,             // The name of the example weight column (optional). To use the weight column, the column data must be of type Single.
        Rank = 28,                                  // The number of components in the PCA.
        Oversampling = 20,                          // Oversampling parameter for randomized PCA training.
        EnsureZeroMean = true,                      // If enabled, data is centered to be zero mean.
        Seed = 1                                    // The seed for random number generation.
    };


    // Create an anomaly detector. Its underlying algorithm is randomized PCA.
    IEstimator<ITransformer> trainer = mlContext.AnomalyDetection.Trainers.RandomizedPca(options: options);

    EstimatorChain<ITransformer> trainingPipeline = dataProcessPipeline.Append(trainer);

`````


### 2. Train model

Training the model is a process of running the chosen algorithm on a training data to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`trainData.csv`) in a DataView object.

`````csharp    
    TransformerChain<ITransformer> model = trainingPipeline.Fit(normalTrainDataView);
`````


### 3. Evaluate model

We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against another dataset that was not used in training (`testData.csv`). 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as AUC, you can explore.

`````csharp
    EvaluateModel(mlContext, model, testDataView);
`````


### 4. Consume model
  
After the model is trained, you can use the `Predict()` API to predict if a transaction is a fraud, using a IDataSet.

`````csharp
[...]

    IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(_dasetFile, separatorChar: ',', hasHeader: true);

    ITransformer model = mlContext.Model.Load(_modelfile, out var inputSchema);

    var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);

[...]

    mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                  .Where(x => x.Label > 0)
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
                  .Where(x => x.Label < 1)
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
