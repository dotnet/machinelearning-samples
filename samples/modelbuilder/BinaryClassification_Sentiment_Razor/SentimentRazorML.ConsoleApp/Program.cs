//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using SentimentRazorML.Model.DataModels;


namespace SentimentRazorML.ConsoleApp
{
    class Program
    {
        //Machine Learning model to load and use for predictions
        private const string MODEL_FILEPATH = @"MLModel.zip";

        static async Task Main(string[] args)
        {
            MLContext mlContext = new MLContext();

            // Training code used by ML.NET CLI and AutoML to generate the model
            //await ModelBuilder.CreateModel();

            ITransformer mlModel = mlContext.Model.Load(GetAbsolutePath(MODEL_FILEPATH), out DataViewSchema inputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            // Create sample data to do a single prediction with it 
            ModelInput sampleData = CreateSingleDataSample(mlContext);

            // Try a single prediction
            ModelOutput predictionResult = predEngine.Predict(sampleData);

            Console.WriteLine($"Single Prediction --> Actual value: {sampleData.Sentiment} | Predicted value: {predictionResult.Prediction}");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static ModelInput CreateSingleDataSample(MLContext mlContext)
        {
            // Here (ModelInput object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            ModelInput sampleForPrediction = new ModelInput
            {
                SentimentText = "Model Builder is cool!"
            };

            return sampleForPrediction;
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
