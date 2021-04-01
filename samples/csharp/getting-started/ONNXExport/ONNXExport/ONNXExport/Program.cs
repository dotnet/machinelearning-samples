using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ONNXExport
{
    class Program
    {
        private static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string TRAIN_DATA_FILEPATH = Path.Combine(currentDirectory, @"..\..\..\..\..\..\Regression_TaxiFarePrediction\TaxiFarePrediction\Data\taxi-fare-train.csv");
        private static string TEST_DATA_FILEPATH = Path.Combine(currentDirectory, @"..\..\..\..\..\..\Regression_TaxiFarePrediction\TaxiFarePrediction\Data\taxi-fare-test.csv");

        static void Main(string[] args)
        {
            var mlContext = new MLContext();

            // Load training data
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: TRAIN_DATA_FILEPATH,
                                            hasHeader: true,
                                            separatorChar: ',');

            // Load test data
            IDataView testDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: TEST_DATA_FILEPATH,
                                            hasHeader: true,
                                            separatorChar: ',');


            // Create data processing pipeline
            var dataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] { new InputOutputColumnPair("vendor_id", "vendor_id"), new InputOutputColumnPair("payment_type", "payment_type") })
                                      .Append(mlContext.Transforms.Concatenate("Features", new[] { "vendor_id", "payment_type", "rate_code", "passenger_count", "trip_time_in_secs", "trip_distance" }));

            // Set the training algorithm and append to pipeline
            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "fare_amount", featureColumnName: "Features");

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // Train ML.NET model on training data
            ITransformer model = trainingPipeline.Fit(trainingDataView);

            // You need a transformer and input data to convert an ML.NET model to an ONNX model
            // By default, the ONNX conversion will generate the ONNX file with the latest OpSet version
            using (var stream = File.Create("taxi-fare-model.onnx"))
                mlContext.Model.ConvertToOnnx(model, trainingDataView, stream);

            // Create the pipeline using the ONNX file
            var onnxModelPath = "taxi-fare-model.onnx";
            var onnxEstimator = mlContext.Transforms.ApplyOnnxModel(onnxModelPath);
            
            // Make sure to either use the 'using' clause or explicitly dispose the returned onnxTransformer to prevent memory leaks
            using var onnxTransformer = onnxEstimator.Fit(trainingDataView);

            // Inference on the test set with the ONNX model
            var output = model.Transform(testDataView);
            var onnxOutput = onnxTransformer.Transform(testDataView);

            //Get the outScores
            var outScores = mlContext.Data.CreateEnumerable<ScoreValue>(output, reuseRowObject: false);
            var onnxOutScores = mlContext.Data.CreateEnumerable<OnnxScoreValue>(onnxOutput, reuseRowObject: false);

            // Print
            PrintScore(outScores, 5);
            PrintScore(onnxOutScores, 5);
            //Expected same results for the above 4 methods
            //Score - 0.09044361
            //Score - 9.105377
            //Score - 11.049
            //Score - 3.061928
            //Score - 6.375817
        }

        // Define model input schema
        public class ModelInput
        {
            [ColumnName("vendor_id"), LoadColumn(0)]
            public string Vendor_id { get; set; }


            [ColumnName("rate_code"), LoadColumn(1)]
            public float Rate_code { get; set; }


            [ColumnName("passenger_count"), LoadColumn(2)]
            public float Passenger_count { get; set; }


            [ColumnName("trip_time_in_secs"), LoadColumn(3)]
            public float Trip_time_in_secs { get; set; }


            [ColumnName("trip_distance"), LoadColumn(4)]
            public float Trip_distance { get; set; }


            [ColumnName("payment_type"), LoadColumn(5)]
            public string Payment_type { get; set; }


            [ColumnName("fare_amount"), LoadColumn(6)]
            public float Fare_amount { get; set; }

        }

        // Define model output schema
        public class ModelOutput
        {
            public float Score { get; set; }
        }

        private class ScoreValue
        {
            public float Score { get; set; }
        }

        private class OnnxScoreValue
        {
            public VBuffer<float> Score { get; set; }
        }

        private static void PrintScore(IEnumerable<ScoreValue> values, int numRows)
        {
            foreach (var value in values.Take(numRows))
                Console.WriteLine("{0, -10} {1, -10}", "Score", value.Score);
        }

        private static void PrintScore(IEnumerable<OnnxScoreValue> values, int numRows)
        {
            foreach (var value in values.Take(numRows))
                Console.WriteLine("{0, -10} {1, -10}", "Score", value.Score.GetItemOrDefault(0));
        }
    }
}