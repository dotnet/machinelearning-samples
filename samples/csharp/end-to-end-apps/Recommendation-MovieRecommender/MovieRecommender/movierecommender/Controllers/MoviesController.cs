using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using movierecommender.Models;
using movierecommender.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace movierecommender.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IProfileService _profileService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            ILogger<MoviesController> logger,
            IOptions<AppSettings> appSettings,
            IMovieService movieService,
            IProfileService profileService)
        {
            _movieService = movieService;
            _profileService = profileService;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public ActionResult Choose()
        {
            return View(_movieService.GetSomeSuggestions());
        }

        public ActionResult Recommend(int id)
        {
            Profile activeprofile = _profileService.GetProfileByID(id);

            // 1. Create the local environment
            MLContext mlContext = new MLContext();

            //2. Load the MoviesRecommendation Model
            ITransformer trainedModel;
            using (FileStream stream = new FileStream(_movieService.GetModelPath(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            //3. Create a prediction function
            PredictionEngine<MovieRating, MovieRatingPrediction> predictionEngine = trainedModel.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(mlContext);

            List<(int movieId, float normalizedScore)> ratings = new List<(int movieId, float normalizedScore)>();
            List<(int movieId, int movieRating)> MovieRatings = _profileService.GetProfileWatchedMovies(id);
            List<Movie> WatchedMovies = new List<Movie>();

            foreach ((int movieId, int movieRating) in MovieRatings)
            {
                WatchedMovies.Add(_movieService.Get(movieId));
            }

            // 3. Create an Rating Prediction Output Class
            MovieRatingPrediction prediction = null;
            foreach (Movie movie in _movieService.GetTrendingMovies)
            {
                //4. Call the Rating Prediction for each movie prediction
                 prediction = predictionEngine.Predict(new MovieRating
                 {
                     userId = id,
                     movieId = movie.MovieID
                 });

                //5. Normalize the prediction scores for the "ratings" b/w 0 - 100
                float normalizedscore = Sigmoid(prediction.Score);

                //6. Add the score for recommendation of each movie in the trending movie list
                 ratings.Add((movie.MovieID, normalizedscore));
            }

            //5. Provide ratings to the view to be displayed
            ViewData["watchedmovies"] = WatchedMovies;
            ViewData["ratings"] = ratings;
            ViewData["trendingmovies"] = _movieService.GetTrendingMovies;
            return View(activeprofile);
        }

        public float Sigmoid(float x)
        {
            return (float) (100/(1 + Math.Exp(-x)));
        }

        public ActionResult Watch()
        {
            return View();
        }

        public ActionResult Profiles()
        {
            List<Profile> profiles = _profileService.GetProfiles;
            return View(profiles);
        }

        public ActionResult Watched(int id)
        {
            Profile activeprofile = _profileService.GetProfileByID(id);
            List<(int movieId, int movieRating)> MovieRatings = _profileService.GetProfileWatchedMovies(id);
            List<Movie> WatchedMovies = new List<Movie>();

            foreach ((int movieId, float normalizedScore) in MovieRatings)
            {
                WatchedMovies.Add(_movieService.Get(movieId));
            }

            ViewData["watchedmovies"] = WatchedMovies;
            ViewData["trendingmovies"] = _movieService.GetTrendingMovies;
            return View(activeprofile);
        }

        public class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }

        public class MovieRating
        {
            public float userId;

            public float movieId;

            public float Label;
        }

        public class MovieRatingPrediction
        {
            public float Label;

            public float Score;
        }
    }
}
