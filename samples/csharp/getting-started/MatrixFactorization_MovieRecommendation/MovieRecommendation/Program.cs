using System;
using Microsoft.ML;
using Microsoft.ML.Trainers.Recommender;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;

using MovieRecommendationConsoleApp.DataStructures;
using MovieRecommendation.DataStructures;

namespace MovieRecommendation
{
    class Program
    {
        // Using the ml-latest-small.zip as dataset from https://grouplens.org/datasets/movielens/. 
        private static string ModelsLocation = @"../../../../MLModels";
        public static string DatasetsLocation = @"../../../../Data";
        private static string TrainingDataLocation = $"{DatasetsLocation}/recommendation-ratings-train.csv";
        private static string TestDataLocation = $"{DatasetsLocation}/recommendation-ratings-test.csv";
        private static string MoviesDataLocation = $"{DatasetsLocation}/movies.csv";
        private const float predictionuserId = 6;
        private const int predictionmovieId = 10;
  
        static void Main(string[] args)
        {
            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            var mlcontext = new MLContext();

            //STEP 2: Create a reader by defining the schema for reading the movie recommendation datasets
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

            //STEP 3: Read the training data which will be used to train the movie recommendation model
            IDataView trainingDataView = reader.Read(TrainingDataLocation);

            //STEP 4: Transform your data by encoding the two features userId and movieID. These encoded features will be provided as input
            //        to our MatrixFactorizationTrainer.
            var pipeline = mlcontext.Transforms.Categorical.MapValueToKey("userId", "userIdEncoded")
                                    .Append(mlcontext.Transforms.Categorical.MapValueToKey("movieId", "movieIdEncoded")
                                    .Append(new MatrixFactorizationTrainer(mlcontext, "Label", "userIdEncoded", "movieIdEncoded",
                                    advancedSettings: s => { s.NumIterations = 20; s.K = 100; })));

            //STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            var model = pipeline.Fit(trainingDataView);

            //STEP 6: Evaluate the model performance 
            Console.WriteLine("=============== Evaluating the model ===============");
            IDataView testDataView = reader.Read(TestDataLocation);
            var prediction = model.Transform(testDataView);
            var metrics = mlcontext.Regression.Evaluate(prediction, label: "Label", score: "Score");
            //Console.WriteLine("The model evaluation metrics rms:" + Math.Round(float.Parse(metrics.Rms.ToString()), 1));


            //STEP 7:  Try/test a single prediction by predicting a single movie rating for a specific user
            var predictionengine = model.MakePredictionFunction<MovieRating, MovieRatingPrediction>(mlcontext);
            /* Make a single movie rating prediction, the scores are for a particular user and will range from 1 - 5. 
               The higher the score the higher the likelyhood of a user liking a particular movie.
               You can recommend a movie to a user if say rating > 3.5.*/
            var movieratingprediction = predictionengine.Predict(
                new MovieRating()
                {
                    //Example rating prediction for userId = 6, movieId = 10 (GoldenEye)
                    userId = predictionuserId,
                    movieId = predictionmovieId
                }
            );

           Movie movieService = new Movie();
           Console.WriteLine("For userId:" + predictionuserId + " movie rating prediction (1 - 5 stars) for movie:" + movieService.Get(predictionmovieId).movieTitle + " is:" + Math.Round(movieratingprediction.Score,1));
        }

    }
}
