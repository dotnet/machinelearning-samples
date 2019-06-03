using Common;
using DatabaseIntegration.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Linq;

namespace DatabaseIntegration
{
    public class Program
    {  
        public static void Main()
        {
            var mlContext = new MLContext(seed: 1);

            //Dataset stored in SQL server
            TrainConsumeModelOnSqlServerData(mlContext);

            //Dataset stored in SQL lite 
            TrainConsumeModelOnSqlLiteData(mlContext);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }      

        public static void TrainConsumeModelOnSqlServerData(MLContext mlContext)
        {            
            SqlServerModel sqlServerModelHelper = new SqlServerModel();

            //Load data from SQL Server Database
            (IDataView trainDataView, IDataView testDataView) = sqlServerModelHelper.LoadData(mlContext);

            //Train Model
            (ITransformer model, string trainerName) = sqlServerModelHelper.TrainModel(mlContext, trainDataView);

            //Evaluate Model
            sqlServerModelHelper.EvaluateModel(mlContext, model, testDataView, trainerName);

            //Predict model
            sqlServerModelHelper.PredictModel(mlContext, model, testDataView);
        }

        public static void TrainConsumeModelOnSqlLiteData(MLContext mlContext)
        {
            SqlLiteModel sqlLiteModel = new SqlLiteModel();

            //Load data from SQL Lite Database
            (IDataView trainDataView, IDataView testDataView) = sqlLiteModel.LoadData(mlContext);

            //Train Model
            (ITransformer model, string trainerName) = sqlLiteModel.TrainModel(mlContext, trainDataView);

            //Evaluate Model
            sqlLiteModel.EvaluateModel(mlContext, model, testDataView, trainerName);

            //Predict model
            sqlLiteModel.PredictModel(mlContext, model, testDataView);
        }
    }
}