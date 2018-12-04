using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using movierecommender.Models;
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
        private readonly MovieService _movieService;
        private readonly ProfileService _profileService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            ILogger<MoviesController> logger,
            IOptions<AppSettings> appSettings)
        {
            _movieService = new MovieService();
            _profileService = new ProfileService();
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
            MLContext ctx = new MLContext();

            //2. Load the MoviesRecommendation Model
            ITransformer loadedModel;
            using (FileStream stream = new FileStream(_movieService.GetModelPath(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = ctx.Model.Load(stream);
            }

            //3. Create a prediction function
            PredictionFunction<RatingData, RatingPrediction> predictionfunction = loadedModel.MakePredictionFunction<RatingData, RatingPrediction>(ctx);

            List<(int movieId, float normalizedScore)> ratings = new List<(int movieId, float normalizedScore)>();
            List<(int movieId, int movieRating)> MovieRatings = _profileService.GetProfileWatchedMovies(id);
            List<Movie> WatchedMovies = new List<Movie>();

            foreach ((int movieId, int movieRating) in MovieRatings)
            {
                WatchedMovies.Add(_movieService.Get(movieId));
            }

            // 3. Create an Rating Prediction Output Class
            RatingPrediction prediction = null;
            foreach (Movie movie in _movieService._trendingMovies)
            {
                //4. Call the Rating Prediction for each movie prediction
                 prediction = predictionfunction.Predict(new RatingData
                 {
                     userId = id.ToString(),
                     movieId = movie.MovieID.ToString()
                 });

                //5. Normalize the prediction scores for the "ratings" b/w 0 - 100
                float normalizedscore = Sigmoid(prediction.Score);

                //6. Add the score for recommendation of each movie in the trending movie list
                 ratings.Add((movie.MovieID, normalizedscore));
            }

            //5. Provide ratings to the view to be displayed
            ViewData["watchedmovies"] = WatchedMovies;
            ViewData["ratings"] = ratings;
            ViewData["trendingmovies"] = _movieService._trendingMovies;
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
            List<Profile> profiles = _profileService._profile;
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
            ViewData["trendingmovies"] = _movieService._trendingMovies;
            return View(activeprofile);
        }

        public class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }

        public class RatingData
        {
            [Column("0")]
            public string userId;

            [Column("1")]
            public string movieId;

            [Column("2")]
            [ColumnName("Label")]
            public float Label;
        }

        public class RatingPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool predictedLabel;

            public float Score;
        }
    }
}
