﻿using System;
using Microsoft.ML;
using Microsoft.ML.Trainers;

using MovieRecommendationConsoleApp.DataStructures;
using MovieRecommendation.DataStructures;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using Microsoft.ML.Core.Data;
using System.IO;

namespace MovieRecommendation
{
    class Program
    {
        // Using the ml-latest-small.zip as dataset from https://grouplens.org/datasets/movielens/. 
        private static string ModelsRelativePath = @"../../../../MLModels";
        public static string DatasetsRelativePath = @"../../../../Data";

        private static string TrainingDataRelativePath = $"{DatasetsRelativePath}/recommendation-ratings-train.csv";
        private static string TestDataRelativePath = $"{DatasetsRelativePath}/recommendation-ratings-test.csv";
        private static string MoviesDataLocation = $"{DatasetsRelativePath}/movies.csv";

        private static string TrainingDataLocation = GetAbsolutePath(TrainingDataRelativePath);
        private static string TestDataLocation = GetAbsolutePath(TestDataRelativePath);

        private static string ModelPath = GetAbsolutePath(ModelsRelativePath);

        private const float predictionuserId = 6;
        private const int predictionmovieId = 10;

        private static string userIdEncoded = nameof(userIdEncoded);
        private static string movieIdEncoded = nameof(movieIdEncoded);

        static void Main(string[] args)
        {
            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            MLContext mlcontext = new MLContext();

            //STEP 2: Read the training data which will be used to train the movie recommendation model    
            //The schema for training data is defined by type 'TInput' in ReadFromTextFile<TInput>() method.
            IDataView trainingDataView = mlcontext.Data.ReadFromTextFile<MovieRating>(TrainingDataLocation, hasHeader: true, separatorChar:',');

            //STEP 3: Transform your data by encoding the two features userId and movieID. These encoded features will be provided as input
            //        to our MatrixFactorizationTrainer.
            var dataProcessingPipeline = mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: userIdEncoded, inputColumnName: nameof(MovieRating.userId))
                           .Append(mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: movieIdEncoded, inputColumnName: nameof(MovieRating.movieId)));
            
            //Specify the options for MatrixFactorization trainer
            MatrixFactorizationTrainer.Options options = new MatrixFactorizationTrainer.Options();
            options.MatrixColumnIndexColumnName = userIdEncoded;
            options.MatrixRowIndexColumnName = movieIdEncoded;
            options.LabelColumnName = DefaultColumnNames.Label;
            options.NumIterations = 20;
            options.K = 100;

            //STEP 4: Create the training pipeline 
            var trainingPipeLine = dataProcessingPipeline.Append(mlcontext.Recommendation().Trainers.MatrixFactorization(options));

            //STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer model = trainingPipeLine.Fit(trainingDataView);

            //STEP 6: Evaluate the model performance 
            Console.WriteLine("=============== Evaluating the model ===============");
            IDataView testDataView = mlcontext.Data.ReadFromTextFile<MovieRating>(TestDataLocation, hasHeader: true, separatorChar: ','); 
            var prediction = model.Transform(testDataView);
            var metrics = mlcontext.Regression.Evaluate(prediction, label: DefaultColumnNames.Label, score: DefaultColumnNames.Score);
            Console.WriteLine("The model evaluation metrics rms:" + metrics.Rms);
            
            //STEP 7:  Try/test a single prediction by predicting a single movie rating for a specific user
            var predictionengine = model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(mlcontext);
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
            Console.WriteLine("For userId:" + predictionuserId + " movie rating prediction (1 - 5 stars) for movie:" + movieService.Get(predictionmovieId).movieTitle + " is:" + Math.Round(movieratingprediction.Score, 1));

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadLine();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
