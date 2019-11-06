# Movie Recommender 

| ML.NET version | API type          | Status                        | App Type    | Data sources | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
|v1.4     | Dynamic API | up-to-date | End-End app | .csv | Movie Recommendation | Recommendation | Field Aware Factorization Machines |

![Alt Text](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender/movierecommender/wwwroot/images/movierecommender.gif)

## Overview

MovieRecommender is a simple application which both builds and consumes a recommendation model. 

This is an end-end sample on how you can enhance your existing ASP.NET apps with recommendations. 

The sample takes insipiration from the popular Netflix application and even though this sample focuses on movie recommendations, learnings can be easily applied to any style of product recommendations. 

## Features
* Wep app 
    * This is an end-end ASP.NET app which presents three user profiles 'Ankit', 'Cesar', 'Amy'. It then provides these three users 
      recommendations using a ML.NET recommendation model.   

* Recommendation Model 
    * The application builds a recommendation model using the MovieLens dataset. The model training code shows 
      uses collaborative filtering based recommendation approach. 

## How does it work?

## Model Training 

Movie Recommender uses Collaborative Filtering for recommendations. 

The underlying assumption with Collaborative filtering is that if a person A (e.g. Amy) has the same opinion as a person B (e.g. Cesar) on an issue, A (Amy) is more likely to have B’s (Cesar) opinion on a different issue than that of a random person. 

For this sample we make use of the http://files.grouplens.org/datasets/movielens/ml-latest-small.zip dataset. 

The model training code can be found in the [MovieRecommender_Model](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender_Model).

Model training follows the following four steps for building the model. You can traverse the code and follow along. 

![Build -> Train -> Evaluate -> Consume](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/shared_content/modelpipeline.png)

## Model Consumption

The trained model is consumed in the [Controller](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender/movierecommender/Controllers/MoviesController.cs#L60) using the following steps. 

### 1. Create the ML.NET environment and load the already trained model

```CSharp

   // 1. Create the ML.NET environment and load the already trained model
   MLContext mlContext = new MLContext();
            
   ITransformer trainedModel;
   using (var stream = new FileStream(_movieService.GetModelPath(), FileMode.Open, FileAccess.Read, FileShare.Read))
   {
    trainedModel = mlContext.Model.Load(stream);
   }
 ```
### 2. Create a prediction function to predict a set of movie recommendations 

```CSharp
   //2. Create a prediction function
   var predictionEngine = mlContext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(trainedModel);
            
   List<(int movieId, float normalizedScore)> ratings = new List<(int movieId, float normalizedScore)>();
   var MovieRatings = _profileService.GetProfileWatchedMovies(id);
   List<Movie> WatchedMovies = new List<Movie>();

   foreach ((int movieId, int movieRating) in MovieRatings)
   {
     WatchedMovies.Add(_movieService.Get(movieId));
   }
   
   MovieRatingPrediction prediction = null;
   
   foreach (var movie in _movieService.GetTrendingMovies)
   {
       //Call the Rating Prediction for each movie prediction
        prediction = predictionEngine.Predict(new MovieRating
        {
            userId = id.ToString(),
            movieId = movie.MovieID.ToString()
        });
       //Normalize the prediction scores for the "ratings" b/w 0 - 100
       float normalizedscore = Sigmoid(prediction.Score);
       //Add the score for recommendation of each movie in the trending movie list
        ratings.Add((movie.MovieID, normalizedscore));
   }
 ```

### 3. Provide rating predictions to the view to be displayed

```CSharp
  ViewData["watchedmovies"] = WatchedMovies;
  ViewData["ratings"] = ratings;
  ViewData["trendingmovies"] = _movieService.GetTrendingMovies;
  return View(activeprofile);
 ```

## Alternate Approaches 
This sample shows one of many recommendation approaches that can be used with ML.NET. Depending upon your specific scenario you can choose any of the following approaches which best fit your usecase. 

| Scenario | Algorithm | Link To Sample
| --- | --- | --- | 
| You want to use attributes (features) like UserId, ProductId, Ratings, Product Description, Product Price etc. for your  recommendation engine. In such a scenario Field Aware Factorization Machine is a generalized approach you can use to build your recommendation engine | Field Aware Factorization Machines | This sample | 
| You have  UserId, ProductId and Ratings available to you for what users bought and rated. For this scenario you should use the Matrix Factorization approach | Matrix Factorization | [Matrix Factorization - Recommendation](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/MatrixFactorization_MovieRecommendation)| 
| You only have UserId and ProductId's the user bought available to you but not ratings. This is  common in datasets from online stores where you might only have access to purchase history for your customers. With this style of recommendation you can build a recommendation engine which recommends frequently bought items. | One Class Matrix Factorization | [Product Recommender](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/MatrixFactorization_ProductRecommendation) | 



