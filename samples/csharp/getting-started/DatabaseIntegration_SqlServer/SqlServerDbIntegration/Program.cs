using Microsoft.ML;
using System;

namespace SqlServerDbIntegration
{
    public class Program
    {  
        public static void Main()
        {
            var mlContext = new MLContext(seed: 1);

            ModelTrainerScorer modelTrainerScorer = new ModelTrainerScorer();

            //Load data from SQL Server Database
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