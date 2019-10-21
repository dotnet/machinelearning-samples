using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using movierecommender.Models;
using movierecommender.Services;
using MovieRecommender.DataStructures;
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
        private readonly PredictionEnginePool<MovieRating, MovieRatingPrediction> _model;

        public MoviesController(PredictionEnginePool<MovieRating, MovieRatingPrediction> model,
            ILogger<MoviesController> logger,
            IOptions<AppSettings> appSettings,
            IMovieService movieService,
            IProfileService profileService)
        {
            _movieService = movieService;
            _profileService = profileService;
            _logger = logger;
            _appSettings = appSettings.Value;
            _model = model;
        }

        public ActionResult Choose()
        {
            return View(_movieService.GetSomeSuggestions());
        }

        public ActionResult Recommend(int id)
        {
            var activeprofile = _profileService.GetProfileByID(id);

            // 1. Create the ML.NET environment and load the already trained model
            MLContext mlContext = new MLContext();

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
                // Call the Rating Prediction for each movie prediction
                 prediction = _model.Predict(new MovieRating
                 {
                     userId = id.ToString(),
                     movieId = movie.MovieID.ToString()
                 });

                // Normalize the prediction scores for the "ratings" b/w 0 - 100
                float normalizedscore = Sigmoid(prediction.Score);

                // Add the score for recommendation of each movie in the trending movie list
                 ratings.Add((movie.MovieID, normalizedscore));
            }

            //3. Provide rating predictions to the view to be displayed
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
            var profiles = _profileService.GetProfiles;
            return View(profiles);
        }

        public ActionResult Watched(int id)
        {
            var activeprofile = _profileService.GetProfileByID(id);
            var MovieRatings = _profileService.GetProfileWatchedMovies(id);
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
    }
}
