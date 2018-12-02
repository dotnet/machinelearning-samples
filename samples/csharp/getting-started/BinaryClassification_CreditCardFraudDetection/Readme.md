# Fraud detection in credit cards based on binary classification and PCA

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7           | Dynamic API | Updated to 0.7 | Two console apps | .csv file | Fraud Detection | Two-class classification | FastTree Binary Classification |

In this introductory sample, you'll see how to use ML.NET to predict a credit card fraud. In the world of machine learning, this type of prediction is known as binary classification.

## API version: Dynamic and Estimators-based API
It is important to note that this sample uses the dynamic API with Estimators.

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

![Build -> Train -> Evaluate -> Consume](https://raw.githubusercontent.com/dotnet/machinelearning-samples/features/samples-new-api/samples/csharp/getting-started/shared_content/modelpipeline.png)


### 1. Build model
Building a model includes:

- Define the data's schema maped to the datasets to read with a DataReader

- Split data for training and tests

- Create an Estimator and transform the data with a ConcatEstimator() and Normalize by Mean Variance. 

- Choosing a trainer/learning algorithm (FastTree) to train the model with.


The initial code is similar to the following:

`````csharp

    // Create a common ML.NET context.
    // Seed set to any number so you have a deterministic environment for repeateable results
    MLContext mlContext = new MLContext(seed:1);

[...]
    TextLoader.Column[] columns = new[] {
           // A boolean column depicting the 'label'.
           new TextLoader.Column("Label", DataKind.BL, 30),
           // 29 Features V1..V28 + Amount
           new TextLoader.Column("V1", DataKind.R4, 1 ),
           new TextLoader.Column("V2", DataKind.R4, 2 ),
           new TextLoader.Column("V3", DataKind.R4, 3 ),
           new TextLoader.Column("V4", DataKind.R4, 4 ),
           new TextLoader.Column("V5", DataKind.R4, 5 ),
           new TextLoader.Column("V6", DataKind.R4, 6 ),
           new TextLoader.Column("V7", DataKind.R4, 7 ),
           new TextLoader.Column("V8", DataKind.R4, 8 ),
           new TextLoader.Column("V9", DataKind.R4, 9 ),
           new TextLoader.Column("V10", DataKind.R4, 10 ),
           new TextLoader.Column("V11", DataKind.R4, 11 ),
           new TextLoader.Column("V12", DataKind.R4, 12 ),
           new TextLoader.Column("V13", DataKind.R4, 13 ),
           new TextLoader.Column("V14", DataKind.R4, 14 ),
           new TextLoader.Column("V15", DataKind.R4, 15 ),
           new TextLoader.Column("V16", DataKind.R4, 16 ),
           new TextLoader.Column("V17", DataKind.R4, 17 ),
           new TextLoader.Column("V18", DataKind.R4, 18 ),
           new TextLoader.Column("V19", DataKind.R4, 19 ),
           new TextLoader.Column("V20", DataKind.R4, 20 ),
           new TextLoader.Column("V21", DataKind.R4, 21 ),
           new TextLoader.Column("V22", DataKind.R4, 22 ),
           new TextLoader.Column("V23", DataKind.R4, 23 ),
           new TextLoader.Column("V24", DataKind.R4, 24 ),
           new TextLoader.Column("V25", DataKind.R4, 25 ),
           new TextLoader.Column("V26", DataKind.R4, 26 ),
           new TextLoader.Column("V27", DataKind.R4, 27 ),
           new TextLoader.Column("V28", DataKind.R4, 28 ),
           new TextLoader.Column("Amount", DataKind.R4, 29 )
       };

   TextLoader.Arguments txtLoaderArgs = new TextLoader.Arguments
                                               {
                                                   Column = columns,
                                                   // First line of the file is a header, not a data row.
                                                   HasHeader = true,
                                                   Separator = ","
                                               };


[...]
    var classification = new BinaryClassificationContext(env);

    (trainData, testData) = classification.TrainTestSplit(data, testFraction: 0.2);

[...]

    //Get all the column names for the Features (All except the Label and the StratificationColumn)
    var featureColumnNames = _trainData.Schema.GetColumns()
        .Select(tuple => tuple.column.Name) // Get the column names
        .Where(name => name != "Label") // Do not include the Label column
        .Where(name => name != "StratificationColumn") //Do not include the StratificationColumn
        .ToArray();

    var pipeline = _mlContext.Transforms.Concatenate("Features", featureColumnNames)
                    .Append(_mlContext.Transforms.Normalize(inputName: "Features", outputName: "FeaturesNormalizedByMeanVar", mode: NormalizerMode.MeanVariance))                       
                    .Append(_mlContext.BinaryClassification.Trainers.FastTree(labelColumn: "Label", 
                                                                              featureColumn: "Features",
                                                                              numLeaves: 20,
                                                                              numTrees: 100,
                                                                              minDatapointsInLeaves: 10,
                                                                              learningRate: 0.2));

`````

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known fraud values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object.

To perform training you need to call the `Fit()` method while providing the training dataset (`trainData.csv`) in a DataView object.

`````csharp    
    var model = pipeline.Fit(_trainData);
`````

### 3. Evaluate model
We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against another dataset that was not used in training (`testData.csv`). 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

`````csharp
    var metrics = _context.Evaluate(model.Transform(_testData), "Label");
`````

### 4. Consume model
After the model is trained, you can use the `Predict()` API to predict if a transaction is a fraud, using a IDataSet.

`````csharp
[...]

   ITransformer model;
   using (var file = File.OpenRead(_modelfile))
   {
       model = mlContext.Model.Load(file);
   }

   var predictionFunc = model.MakePredictionFunction<TransactionObservation, TransactionFraudPrediction>(mlContext);

[...]

    dataTest.AsEnumerable<TransactionObservation>(mlContext, reuseRowObject: false)
                        .Where(x => x.Label == true)
                        .Take(numberOfTransactions)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData => 
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionFunc.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });
[...]

    dataTest.AsEnumerable<TransactionObservation>(mlContext, reuseRowObject: false)
                        .Where(x => x.Label == false)
                        .Take(numberOfTransactions)
                        .ToList()
                        .ForEach(testData =>
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionFunc.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });

`````
