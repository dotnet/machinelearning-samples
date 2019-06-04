using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Trainers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DatabaseIntegration
{
    public class Program
    {
        // The url for the dataset that will be downloaded
        public static string datasetUrl = "https://raw.githubusercontent.com/dotnet/machinelearning/244a8c2ac832657af282aa312d568211698790aa/test/data/adult.train";
        public static void Main()
        {
            var mlContext = new MLContext(seed: 1);

            ModelTrainerScorer modelTrainerScorer = new ModelTrainerScorer(datasetUrl);

            //Load data from SQLite Database
            (IDataView trainDataView, IDataView testDataView) = modelTrainerScorer.LoadData(mlContext);

            //Train Model
            (ITransformer model, string trainerName) = modelTrainerScorer.TrainModel(mlContext, trainDataView);

            //Evaluate Model
            modelTrainerScorer.EvaluateModel(mlContext, model, testDataView, trainerName);

            //Predict model
            modelTrainerScorer.PredictModel(mlContext, model, testDataView);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }
    }
}