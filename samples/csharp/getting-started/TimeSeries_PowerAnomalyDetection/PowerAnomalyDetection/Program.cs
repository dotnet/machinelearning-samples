using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using PowerAnomalyDetection.DataStructures;

namespace myApp
{
    class Program
    {
        private static string DatasetsRelativePath = @"../../../Data";
        private static string TrainingDatarelativePath = $"{DatasetsRelativePath}/power-export_min.csv";

        private static string TrainingDataPath = GetAbsolutePath(TrainingDatarelativePath);

        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/PowerAnomalyDetectionModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main()
        {
            var mlContext = new MLContext(seed:0);

            // load data
            var dataView = mlContext.Data.LoadFromTextFile<MeterData>(
               TrainingDataPath,
               separatorChar: ',',
               hasHeader: true);

            // transform options
            BuildTrainModel(mlContext, dataView);  // using SsaSpikeEstimator

            DetectAnomalies(mlContext, dataView);

            Console.WriteLine("\nPress any key to exit");
            Console.Read();
        }


        public static void BuildTrainModel(MLContext mlContext, IDataView dataView)
        {
            // Configure the Estimator
            const int PValueSize = 30;
            const int SeasonalitySize = 30;
            const int TrainingSize = 90;
            const int ConfidenceInterval = 98;            

            string outputColumnName = nameof(SpikePrediction.Prediction);
            string inputColumnName = nameof(MeterData.ConsumptionDiffNormalized);

            var trainigPipeLine = mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName,
                inputColumnName,
                confidence: ConfidenceInterval,
                pvalueHistoryLength: PValueSize,
                trainingWindowSize: TrainingSize,
                seasonalityWindowSize: SeasonalitySize);

            ITransformer trainedModel = trainigPipeLine.Fit(dataView);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, dataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);
            Console.WriteLine("");
        }

        public static void DetectAnomalies(MLContext mlContext,IDataView dataView)
        {
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            var transformedData = trainedModel.Transform(dataView);

            // Getting the data of the newly created column as an IEnumerable
            IEnumerable<SpikePrediction> predictions =
                mlContext.Data.CreateEnumerable<SpikePrediction>(transformedData, false);
            
            var colCDN = dataView.GetColumn<float>("ConsumptionDiffNormalized").ToArray();
            var colTime = dataView.GetColumn<DateTime>("time").ToArray();

            // Output the input data and predictions
            Console.WriteLine("======Displaying anomalies in the Power meter data=========");
            Console.WriteLine("Date              \tReadingDiff\tAlert\tScore\tP-Value");

            int i = 0;
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine("{0}\t{1:0.0000}\t{2:0.00}\t{3:0.00}\t{4:0.00}", 
                    colTime[i], colCDN[i], 
                    p.Prediction[0], p.Prediction[1], p.Prediction[2]);
                Console.ResetColor();
                i++;
            }
        }

        public static string GetAbsolutePath(string relativeDatasetPath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativeDatasetPath);

            return fullPath;
        }
    }
}