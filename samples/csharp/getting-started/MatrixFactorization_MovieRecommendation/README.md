# Movie Recommendation - Matrix Factorization problem sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7   | Dynamic API | Up-to date | Console app | .csv files | Recommendation | Matrix Factorization | MatrixFactorizationTrainer|

In this sample, you can see how to use ML.NET to build a movie recommendation engine. 


## Problem
For this tutorial we will use the MovieLens dataset which comes with movie ratings, titles, genres and more.  In terms of an approach for building our movie recommendation engine we will use Factorization Machines which uses a colloborative filtering approach. 

‘Collaborative filtering’ operates under the underlying assumption that if a person A has the same opinion as a person B on an issue, A is more likely to have B’s opinion on a different issue than that of a randomly chosen person.

## DataSet
The original data comes from MovieLens Dataset:
http://files.grouplens.org/datasets/movielens/ml-latest-small.zip

## ML task - [Matrix Factorization (Recommendation)](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#recommendation)

The ML Task for this sample is Matrix Factorization, which is a supervised machine learning task performing colloborative filtering. 

## Solution

To solve this problem, you build and train an ML model on existing training data, evaluate how good it is (analyzing the obtained metrics), and lastly you can consume/test the model to predict the demand given input data variables.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Define the data's schema maped to the datasets to read (`recommendation-ratings-train.csv` and `recommendation-ratings-test.csv`) with a DataReader

* Matrix Factorization requires the two features userId, movieId to be encoded

* Matrix Factorization trainer then takes these two encoded features (userId, movieId) as input 

Here's the code which will be used to build the model:
```CSharp
 
 var mlcontext = new MLContext();

 var reader = mlcontext.Data.TextReader(new TextLoader.Arguments()
            {
                Separator = ",",
                HasHeader = true,
                Column = new[]
                {
                    new TextLoader.Column("userId", DataKind.R4, 0),
                    new TextLoader.Column("movieId", DataKind.R4, 1),
                    new TextLoader.Column("Label", DataKind.R4, 2)
                }
            });

 IDataView trainingDataView = reader.Read
                    (new MultiFileSource(TrainingDataLocation));

var pipeline = mlcontext.Transforms.Categorical.MapValueToKey   
                               ("userId", "userIdEncoded")
             .Append(mlcontext.Transforms.Categorical.MapValueToKey                     ("movieId", "movieIdEncoded")
            .Append(new MatrixFactorizationTrainer(mlcontext, "Label",                    "userIdEncoded", "movieIdEncoded")));


```