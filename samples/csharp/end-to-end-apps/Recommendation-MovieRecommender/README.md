# Movie Recommender 

| ML.NET version | API type          | Status                        | App Type    | Data sources | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
|v0.7| Dynamic API | Up-to-date | End-End app | .csv | Movie Recommendation | Recommendation | Field Aware Factorization Machines |

## Overview
MovieRecommender is a simple application which both builds and consumes a recommendation model. 

This is an end-end sample on how you can enhance your existing ASP.NET apps with recommendations. 

The sample takes insipiration from the popular Netflix application. Even though this sample focuses on movie recommendations, learnings can be easily applied to any style of product recommendations. 

## Features
* Wep app 
    * This is an end-end ASP.NET app which presents three user profiles 'Ankit', 'Cesar', 'Gal'. It then provides these three users 
      recommendations using a ML.NET recommendation model.   

* Recommendation Model 
    * The application builds a recommendation model using the MovieLens dataset. The model training code shows 
      uses colloborative filtering based recommendation approach. 

## How does it work?

## Model Training 
There are multiple ways to build recommendation models for your application. Choose the best example for the task based upon your scenario. With ML.NET we support the following three recommendation scenarios, depending upon your scenario you can pick either of the three from the list below.

With ML.NET we support the following three recommendation scenarios, depending upon your scenario you can pick either of the three from the list below. 

| Scenario | Algorithm | Link To Sample
| --- | --- | --- | 
| You want to use more attributes (features) beyond UserId, ProductId and Ratings like Product Description, Product Price etc. for your recommendation engine | Field Aware Factorization Machines | This sample | 
| You have  UserId, ProductId and Ratings available to you for what users bought and rated.| Matrix Factorization | <a href="samples/csharp/getting-started/MatrixFactorization_MovieRecommendation">Matrix Factorization based Recommendation</a>| 
| You only have UserId and ProductId's the user bought available to you but not ratings. This is  common in datasets from online stores where you might only have access to purchase history for your customers. With this style of recommendation you can build a recommendation engine which recommends frequently bought items. | One Class Matrix Factorization | Coming Soon | 

For this sample we make use of the http://files.grouplens.org/datasets/movielens/ml-latest-small.zip dataset. 

## Model Consumption
The model is consumed in the MoviesController Recommend method using the following piece of code. 

```CSharp

            // 1. Create the local environment
            var ctx = new MLContext();
            
            //2. Load the MoviesRecommendation Model
            ITransformer loadedModel;
            using (var stream = new FileStream(_movieService.GetModelPath(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = ctx.Model.Load(stream);
            }

            //3. Create a prediction function
            var predictionfunction = loadedModel.MakePredictionFunction<RatingData, RatingPrediction>(ctx);
            
            List<Tuple<int, float>> ratings = new List<Tuple<int, float>>();
            List<Tuple<int, int>> MovieRatings = _profileService.GetProfileWatchedMovies(id);
            List<Movie> WatchedMovies = new List<Movie>();

            foreach (Tuple<int, int> tuple in MovieRatings)
            {
                WatchedMovies.Add(_movieService.Get(tuple.Item1));
            }

            // 3. Create an Rating Prediction Output Class
            RatingPrediction prediction = null;
            foreach (var movie in _movieService._trendingMovies)
            {
            //4. Call the Rating Prediction for each movie prediction
             prediction = predictionfunction.Predict(new RatingData { userId = id.ToString(), movieId = movie.MovieID.ToString()});
              
            //5. Normalize the prediction scores for the "ratings" b/w 0 - 100
             var normalizedscore = Sigmoid(prediction.Score);

            //6. Add the score for recommendation of each movie in the trending movie list
             ratings.Add(Tuple.Create(movie.MovieID, normalizedscore));
            }





