# Product Recommendation - Matrix Factorization problem sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| 0.8.0-preview-27128-6   | Dynamic API | Needs update to v0.8 | Console app | .txt files | Recommendation | Matrix Factorization | MatrixFactorizationTrainer (One Class)|

In this sample, you can see how to use ML.NET to build a product recommendation scenario.

The style of recommendation in this sample is based upon the co-purchase scenario or products frequently 
bought together which means it will recommend customers a set of products based upon their order purchase
history. 

![Alt Text](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/MatrixFactorization_ProductRecommendation/ProductRecommender/Data/frequentlyboughttogether.png)

In this example, the highlighted products are being recommended based upon a frequently bought together learning model. 


## Problem
For this tutorial we will use the Amazon product co-purchasing network dataset.  

In terms of an approach for building our product recommender we will use One-Class Factorization Machines which uses a colloborative filtering approach. 


The difference between one-class and other Factorization Machines approach we covered is that in this dataset we only have information on order purchase history.

We do not have ratings or other details like product description etc. available to us. 

Matrix Factorization relies on ‘Collaborative filtering’ which operates under the underlying assumption that if a person A has the same opinion as a person B on an issue, A is more likely to have B’s opinion on a different issue than that of a randomly chosen person.

## DataSet
The original data comes from SNAP:
https://snap.stanford.edu/data/amazon0302.html


## ML task - [Matrix Factorization (Recommendation)](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#recommendation)

The ML Task for this sample is Matrix Factorization, which is a supervised machine learning task performing colloborative filtering. 

## Solution

To solve this problem, you build and train an ML model on existing training data, evaluate how good it is (analyzing the obtained metrics), and lastly you can consume/test the model to predict the demand given input data variables.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Download and copy the dataset Amazon0302.txt file from https://snap.stanford.edu/data/amazon0302.html. 

* Replace the column names with only these instead:  ProductID	ProductID_Copurchased

* Given in the reader we already provide a KeyRange and product ID's are already encoded all we need to do is
  call the MatrixFactorizationTrainer with a few extra parameters. 

Here's the code which will be used to build the model:
```CSharp
 
    //STEP 1: Create MLContext to be shared across the model creation workflow objects 
    var ctx = new MLContext();

    //STEP 2: Create a reader by defining the schema for reading the product co-purchase dataset
    //        Do remember to replace amazon0302.txt with dataset from 
              https://snap.stanford.edu/data/amazon0302.html
    var reader = ctx.Data.TextReader(new TextLoader.Arguments()
    {
        Separator = "tab",
        HasHeader = true,
        Column = new[]
        {
                new TextLoader.Column("Label", DataKind.R4, 0),
                new TextLoader.Column("ProductID", DataKind.U4, new [] { new TextLoader.Range(0) }, new KeyRange(0, 262110)),
                new TextLoader.Column("CoPurchaseProductID", DataKind.U4, new [] { new TextLoader.Range(1) }, new KeyRange(0, 262110))
            }
        });

        //STEP 3: Read the training data which will be used to train the movie recommendation model
        var traindata = reader.Read(new MultiFileSource(TrainingDataLocation));


        //STEP 4: Your data is already encoded so all you need to do is call the MatrixFactorization Trainer with a few extra hyperparameters:
        //        LossFunction, Alpa, Lambda and a few others like K and C as shown below. 
        var est = ctx.Recommendation().Trainers.MatrixFactorization("ProductID", "CoPurchaseProductID",  
                                     labelColumn: "Label",
                                     advancedSettings: s =>
                                     {
                                         s.LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass;
                                         s.Alpha = 0.01;
                                         s.Lambda = 0.025;
                                         // For better results use the following parameters
                                         //s.K = 100;
                                         //s.C = 0.00001;
                                     });
```

### 2. Train Model 

Once the estimator has been defined, you can train the estimator on the training data available to us. 

This will return a trained model. 

```CSharp

    //STEP 5: Train the model fitting to the DataSet
    //Please add Amazon0302.txt dataset from https://snap.stanford.edu/data/amazon0302.html to Data folder if FileNotFoundException is thrown.
    var model = est.Fit(traindata);

```

### 3. Evaluate Model 

We will perform predictions for this model by creating a prediction engine/function as shown below.

The prediction engine creation takes in as input the following two classes. 

```CSharp
    public class Copurchase_prediction
    {
        public float Score { get; set; }
    }

    public class ProductEntry
    {
        [KeyType(Contiguous = true, Count = 262111, Min = 0)]
        public uint ProductID { get; set; }

        [KeyType(Contiguous = true, Count = 262111, Min = 0)]
        public uint CoPurchaseProductID { get; set; }
        }
```

Once the prediction engine has been created you can predict scores of two products being co-purchased. 

```CSharp
    //STEP 6: Create prediction engine and predict the score for Product 63 being co-purchased with Product 3.
    //        The higher the score the higher the probability for this particular productID being co-purchased 
    var predictionengine = model.MakePredictionFunction<ProductEntry, Copurchase_prediction>(ctx);
    var prediction = predictionengine.Predict(
                             new ProductEntry()
                             {
                             ProductID = 3,
                             CoPurchaseProductID = 63
                             });
```
