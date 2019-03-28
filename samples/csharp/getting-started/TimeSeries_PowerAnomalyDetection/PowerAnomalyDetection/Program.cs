using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

namespace myApp
{
    class Program
    {
        class MeterData
        {
            [LoadColumn(0)]
            public string name { get; set; }
            [LoadColumn(1)]
            public DateTime time { get; set; }
            [LoadColumn(2)]
            public float ConsumptionDiffNormalized { get; set; }
        }
        
        class SpikePrediction
        {
            [VectorType(3)]
            public double[] Prediction { get; set; }
        }

        private static string DatasetsLocation = @"../../../Data";
        private static string TrainingData = $"{DatasetsLocation}/power-export_min.csv";

        public static IDataView LoadPowerDataMin(MLContext ml)
        {
            var dataView = ml.Data.LoadFromTextFile<MeterData>(
                TrainingData,
                separatorChar: ',',
                hasHeader: true);

            // take a peek to make sure data is loaded
            //var col = dataView.GetColumn<float>(ml, "ConsumptionDiffNormalized").ToArray(); 

            return dataView;
        }

        static void Main()
        {
            var ml = new MLContext();

            // load data
            var dataView = LoadPowerDataMin(ml);

            // transform options
            BuildTrainEvaluateModel(ml, dataView);  // using SsaSpikeEstimator

            Console.WriteLine("\nPress any key to exit");
            Console.Read();
        }


        public static void BuildTrainEvaluateModel(MLContext ml, IDataView dataView)
        {
            // Configure the Estimator
            const int PValueSize = 30;
            const int SeasonalitySize = 30;
            const int TrainingSize = 90;
            const int ConfidenceInterval = 98;

            string outputColumnName = nameof(SpikePrediction.Prediction);
            string inputColumnName = nameof(MeterData.ConsumptionDiffNormalized);  

            var estimator = ml.Transforms.SsaSpikeEstimator(
                outputColumnName, 
                inputColumnName, 
                confidence: ConfidenceInterval, 
                pvalueHistoryLength: PValueSize, 
                trainingWindowSize: TrainingSize, 
                seasonalityWindowSize: SeasonalitySize);

            var model = estimator.Fit(dataView);

            var transformedData = model.Transform(dataView);

            // Getting the data of the newly created column as an IEnumerable
            IEnumerable<SpikePrediction> predictionColumn = 
                ml.Data.CreateEnumerable<SpikePrediction>(transformedData, false);
            
            var colCDN = dataView.GetColumn<float>(ml, "ConsumptionDiffNormalized").ToArray();
            var colTime = dataView.GetColumn<DateTime>(ml, "time").ToArray();
            
            // Output the input data and predictions
            Console.WriteLine($"{outputColumnName} column obtained post-transformation.");
            Console.WriteLine("Date              \tReadingDiff\tAlert\tScore\tP-Value");

            int i = 0;
            foreach (var p in predictionColumn)
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
    }
}