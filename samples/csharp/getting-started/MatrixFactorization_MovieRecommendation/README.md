# Movie Recommendation - Matrix Factorization problem sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| Microsoft.ML.Recommender Preview v0.16.0   | Dynamic API | Up-to-date | Console app | .csv files | Recommendation | Matrix Factorization | MatrixFactorizationTrainer|

In this sample, you can see how to use ML.NET to build a movie recommendation engine. 


## Problem
For this tutorial we will use the MovieLens dataset which comes with movie ratings, titles, genres and more.  In terms of an approach for building our movie recommendation engine we will use Factorization Machines which uses a collaborative filtering approach. 

‘Collaborative filtering’ operates under the underlying assumption that if a person A has the same opinion as a person B on an issue, A is more likely to have B’s opinion on a different issue than that of a randomly chosen person. 

With ML.NET we support the following three recommendation scenarios, depending upon your scenario you can pick either of the three from the list below. 

| Scenario | Algorithm | Link To Sample
| --- | --- | --- | 
| You have UserId, ProductId and Ratings available to you for what users bought and rated.| Matrix Factorization | This sample | 
| You only have UserId and ProductId's the user bought available to you but not ratings. This is common in datasets from online stores where you might only have access to purchase history for your customers. With this style of recommendation you can build a recommendation engine which recommends frequently bought items. | One Class Matrix Factorization | [Product Recommender](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/MatrixFactorization_ProductRecommendation) | 
| You want to use more attributes (features) beyond UserId, ProductId and Ratings like Product Description, Product Price etc. for your recommendation engine | Field Aware Factorization Machines | [Movie Recommender with Factorization Machines](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender_Model) | 


## DataSet
The original data comes from MovieLens Dataset:
http://files.grouplens.org/datasets/movielens/ml-latest-small.zip

## Algorithm - [Matrix Factorization (Recommendation)](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#recommendation)

The algorithm for this recommendation task is Matrix Factorization, which is a supervised machine learning algorithm performing collaborative filtering. 

## Solution

To solve this problem, you build and train an ML model on existing training data, evaluate how good it is (analyzing the obtained metrics), and lastly you can consume/test the model to predict the demand given input data variables.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Define the data's schema mapped to the datasets to read (`recommendation-ratings-train.csv` and `recommendation-ratings-test.csv`) with a Textloader

* Matrix Factorization requires the two features userId, movieId to be encoded

* Matrix Factorization trainer then takes these two encoded features (userId, movieId) as input 

Here's the code which will be used to build the model:
```CSharp
 
 //STEP 1: Create MLContext to be shared across the model creation workflow objects 
  MLContext mlcontext = new MLContext();

 //STEP 2: Read the training data which will be used to train the movie recommendation model    
 //The schema for training data is defined by type 'TInput' in LoadFromTextFile<TInput>() method.
 IDataView trainingDataView = mlcontext.Data.LoadFromTextFile<MovieRating>(TrainingDataLocation, hasHeader: true, ar:',');

//STEP 3: Transform your data by encoding the two features userId and movieID. These encoded features will be provided as 
//        to our MatrixFactorizationTrainer.
 var dataProcessingPipeline = mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: userIdEncoded, inputColumnName: eRating.userId))
                .Append(mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: movieIdEncoded, inputColumnName: nameofg.movieId)));
 
 //Specify the options for MatrixFactorization trainer
 MatrixFactorizationTrainer.Options options = new MatrixFactorizationTrainer.Options();
 options.MatrixColumnIndexColumnName = userIdEncoded;
 options.MatrixRowIndexColumnName = movieIdEncoded;
 options.LabelColumnName = "Label";
 options.NumberOfIterations = 20;
 options.ApproximationRank = 100;

//STEP 4: Create the training pipeline 
 var trainingPipeLine = dataProcessingPipeline.Append(mlcontext.Recommendation().Trainers.MatrixFactorization(options));

```


### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known movie and user ratings) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training you need to call the `Fit()` method while providing the training dataset (`recommendation-ratings-train.csv` file) in a DataView object.

```CSharp    
ITransformer model = trainingPipeLine.Fit(trainingDataView);
```
Note that ML.NET works with data with a lazy-load approach, so in reality no data is really loaded in memory until you actually call the method .Fit().

### 3. Evaluate model
We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`recommendation-ratings-test.csv`). 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```CSharp 
Console.WriteLine("=============== Evaluating the model ===============");
IDataView testDataView = mlcontext.Data.LoadFromTextFile<MovieRating>(TestDataLocation, hasHeader: true); 
var prediction = model.Transform(testDataView);
var metrics = mlcontext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");
```

### 4. Consume model
After the model is trained, you can use the `Predict()` API to predict the rating for a particular movie/user combination. 
```CSharp    
var predictionengine = mlcontext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(model);
var movieratingprediction = predictionengine.Predict(
                new MovieRating()
                {
                    //Example rating prediction for userId = 6, movieId = 10 (GoldenEye)
                    userId = predictionuserId,
                    movieId = predictionmovieId
                }
            );
 Console.WriteLine("For userId:" + predictionuserId + " movie rating prediction (1 - 5 stars) for movie:" +  
                   movieService.Get(predictionmovieId).movieTitle + " is:" + Math.Round(movieratingprediction.Score,1));
       
```
Please note this is one approach for performing movie recommendations with Matrix Factorization. There are other scenarios for recommendation as well which we will build samples for as well. 

#### Score in Matrix Factorization

The score produced by matrix factorization represents the likelihood of being a positive case. The larger the score value, the higher probability of being a positive case. However, the score doesn't carry any probability information. When making a prediction, you will have to compute multiple merchandises' scores and pick up the merchandise with the highest score.
