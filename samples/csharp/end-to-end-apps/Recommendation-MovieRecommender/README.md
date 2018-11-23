# Movie Recommender 

| ML.NET version | API type          | Status                        | App Type    | Data sources | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
|v0.7| Dynamic API | Up-to-date | End-End app | .csv | Movie Recommendation | Recommendation | Field Aware Factorization Machines |

![Alt Text](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender/movierecommender/wwwroot/images/movierecommender.gif)

## Overview

MovieRecommender is a simple application which both builds and consumes a recommendation model. 

This is an end-end sample on how you can enhance your existing ASP.NET apps with recommendations. 

The sample takes insipiration from the popular Netflix application and even though this sample focuses on movie recommendations, learnings can be easily applied to any style of product recommendations. 

## Features
* Wep app 
    * This is an end-end ASP.NET app which presents three user profiles 'Ankit', 'Cesar', 'Gal'. It then provides these three users 
      recommendations using a ML.NET recommendation model.   

* Recommendation Model 
    * The application builds a recommendation model using the MovieLens dataset. The model training code shows 
      uses colloborative filtering based recommendation approach. 

## How does it work?

## Model Training 

Movie Recommender uses Colloborative Filtering for recommendations. 

The underlying assumption with Colloborative filtering is that if a person A (e.g. Gal) has the same opinion as a person B (e.g. Cesar) on an issue, A (Gal) is more likely to have B’s (Cesar) opinion on a different issue than that of a random person. 

For this sample we make use of the http://files.grouplens.org/datasets/movielens/ml-latest-small.zip dataset. 

The model training code can be found in the [MovieRecommender_Model](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender_Model).

Model training follows the following four steps for building the model. You can traverse the code and follow along. 

![Build -> Train -> Evaluate -> Consume](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/shared_content/modelpipeline.png)

## Model Consumption

The trained model is consumed in the [Controller](https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/end-to-end-apps/Recommendation-MovieRecommender/MovieRecommender/movierecommender/Controllers/MoviesController.cs#L60) using the following steps. 

### 1. Create the ML.NET environment and load the already trained model

```CSharp

   // 1. Create the ML.NET environment and load the MoviesRecommendation Model
   var ctx = new MLContext();
            
   ITransformer loadedModel;
   using (var stream = new FileStream(_movieService.GetModelPath(), FileMode.Open, FileAccess.Read, FileShare.Read))
   {
   loadedModel = ctx.Model.Load(stream);
   }
 ```
### 2. Create a prediction function to predict a set of movie recommendations 

```CSharp
   //3. Create a prediction function
   var predictionfunction = loadedModel.MakePredictionFunction<RatingData, RatingPrediction>(ctx);
            
   List<Tuple<int, float>> ratings = new List<Tuple<int, float>>();
   List<Tuple<int, int>> MovieRatings = _profileService.GetProfileWatchedMovies(id);
   List<Movie> WatchedMovies = new List<Movie>();

   foreach (Tuple<int, int> tuple in MovieRatings)
   {
   WatchedMovies.Add(_movieService.Get(tuple.Item1));
   }
   
   RatingPrediction prediction = null;
   
   foreach (var movie in _movieService._trendingMovies)
   {
   // Call the Rating Prediction for each movie prediction
      prediction = predictionfunction.Predict(new RatingData { userId = id.ToString(), movieId = movie.MovieID.ToString()});
              
   // Normalize the prediction scores for the "ratings" b/w 0 - 100
      var normalizedscore = Sigmoid(prediction.Score);

   // Add the score for recommendation of each movie in the trending movie list
      ratings.Add(Tuple.Create(movie.MovieID, normalizedscore));
   }
 ```

### 3. Provide rating predictions to the view to be displayed

   ViewData["watchedmovies"] = WatchedMovies;
   ViewData["ratings"] = ratings;
   ViewData["trendingmovies"] = _movieService._trendingMovies;
   return View(activeprofile);

## Alternate Approaches 


