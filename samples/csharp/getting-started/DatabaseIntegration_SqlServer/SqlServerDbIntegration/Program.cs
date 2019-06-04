using Microsoft.ML;
using System;

namespace SqlServerDbIntegration
{
    public class Program
    {  
        public static void Main()
        {
            var mlContext = new MLContext(seed: 1);

            SqlServerModel sqlServerModelHelper = new SqlServerModel();

            //Load data from SQL Server Database
            (IDataView trainDataView, IDataView testDataView) = sqlServerModelHelper.LoadData(mlContext);

            //Train Model
            (ITransformer model, string trainerName) = sqlServerModelHelper.TrainModel(mlContext, trainDataView);

            //Evaluate Model
            sqlServerModelHelper.EvaluateModel(mlContext, model, testDataView, trainerName);

            //Predict model
            sqlServerModelHelper.PredictModel(mlContext, model, testDataView);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }            
    }
}