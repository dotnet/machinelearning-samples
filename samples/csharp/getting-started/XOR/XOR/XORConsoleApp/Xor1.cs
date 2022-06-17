
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace XORApp
{

    public static class XOR1
    {

        public class XORData {

            [LoadColumn(0)]
            public float Input1;

            [LoadColumn(1)]
            public float Input2;
            
            [LoadColumn(2)]
            public float Output; // Label

            public XORData(float input1, float input2)
            {
                Input1 = input1;
                Input2 = input2;
            }

            public XORData(float input1, float input2, float output)
            {
                Input1 = input1;
                Input2 = input2;
                Output = output;
            }

            public bool Equals1(Object obj)
            {
                // Check for null and compare run-time types.
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    XORData p = (XORData)obj;
                    return (Input1 == p.Input1) &&
                           (Input2 == p.Input2) &&
                           (Output == p.Output);
                }
            }

            public static bool Equals2(XORData p, System.Data.DataRow dr)
            {
                var I1 = (string)dr[0];
                var I2 = (string)dr[1];
                var O = (string)dr[2];
                float Input1 = 0, Input2 = 0, Output = 0;
                float.TryParse(I1, out Input1);
                float.TryParse(I2, out Input2);
                float.TryParse(O, out Output);
                return (Input1 == p.Input1) &&
                       (Input2 == p.Input2) &&
                       (Output == p.Output);
            }

        }

        public class XORPrediction {
            public float Score;
        }

        public class SampleXORData {
            internal static readonly XORData XOR1 = new XORData(1.0F, 0.0F);
            internal static readonly XORData XOR2 = new XORData(0.0F, 0.0F);
            internal static readonly XORData XOR3 = new XORData(0.0F, 1.0F);
            internal static readonly XORData XOR4 = new XORData(1.1F, 1.0F);
        }

        public static List<XORData> LoadData()
        {
            // STEP 1: Common data loading configuration
            
            var data = new List<XORData>
            {
                new XORData(1.0F, 0.0F, 1.0F),
                new XORData(0.0F, 0.0F, 0.0F),
                new XORData(0.0F, 1.0F, 1.0F),
                new XORData(1.0F, 1.0F, 0.0F)
            };
            
            // This is not optimal, it causes an overconsumption of RAM,
            //  it would have been better to add more iterations instead
            // From: https://stackoverflow.com/questions/53472759/why-does-this-ml-net-code-fail-to-predict-the-correct-output
            // minimal set: Repeat 10
            var largeSet = Enumerable.Repeat(data, 10).SelectMany(a => a).ToList();
            return largeSet;
        }

        public static void Train(MLContext mlContext, string ModelPath, List<XORData> largeSet)
        {
            var trainingDataView = mlContext.Data.LoadFromEnumerable<XORData>(largeSet);
            var testDataView = mlContext.Data.LoadFromEnumerable<XORData>(largeSet);

            // Check if the Repeat function works fine
            int iNumSample = 0;
            int iNumSample4 = 0;
            int iNbSuccess = 0;
            var data = LoadData();
            foreach (var d in largeSet)
            {
                var d2 = data[iNumSample4];
                bool b = d.Equals1(d2);
                if (b) iNbSuccess++;
                iNumSample++;
                iNumSample4++;
                if (iNumSample4 >= 4) iNumSample4 = 0;
            }

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features", nameof(XORData.Input1), nameof(XORData.Input2))
                .AppendCacheCheckpoint(mlContext);
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            ConsoleHelper.PeekDataViewInConsole(mlContext, 
                trainingDataView, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, 
                "Features", trainingDataView, dataProcessPipeline, 5);

            // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
            var trainer = mlContext.Regression.Trainers.FastTree(
                labelColumnName: "Output", featureColumnName: "Features", learningRate: 0.1);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(
                predictions, labelColumnName: "Output", scoreColumnName: "Score");
            ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            var directoryPath = Path.GetDirectoryName(ModelPath);
            if (!Directory.Exists(directoryPath)) {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                di.Create();
            }
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);
            Console.WriteLine("The model is saved to {0}", Path.GetFullPath(ModelPath));
        }

        public static void TrainFromFile(MLContext mlContext, 
            string ModelPath, string TrainDataPath)
        {
            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<XORData>(
                TrainDataPath, hasHeader: true);
            var testDataView = mlContext.Data.LoadFromTextFile<XORData>(
                TrainDataPath, hasHeader: true);

            // Check if the XOR file matches with the RAM data
            var table = trainingDataView.ToDataTable();
            int iNumSample = 0;
            int iNbSuccess = 0;
            var data = LoadData();
            foreach (var d in data)
            {
                var d2 = table.Rows[iNumSample];
                bool b = XORData.Equals2(d, d2);
                if (b) iNbSuccess++;
                iNumSample++;
            }

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features", nameof(XORData.Input1), nameof(XORData.Input2))
                .AppendCacheCheckpoint(mlContext);
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            ConsoleHelper.PeekDataViewInConsole(mlContext,
                trainingDataView, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext,
                "Features", trainingDataView, dataProcessPipeline, 5);

            // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
            var trainer = mlContext.Regression.Trainers.FastTree(
                labelColumnName: "Output", featureColumnName: "Features", 
                learningRate: 0.1);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(
                predictions, labelColumnName: "Output", scoreColumnName: "Score");
            ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            var directoryPath = Path.GetDirectoryName(ModelPath);
            if (!Directory.Exists(directoryPath))
            {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                di.Create();
            }
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);
            Console.WriteLine("The model is saved to {0}", Path.GetFullPath(ModelPath));
        }

        public static void TestSomePredictions(MLContext mlContext, string ModelPath)
        {

            // Test Classification Predictions with some hard-coded samples 
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<XORData, XORPrediction>(trainedModel);

            Console.WriteLine("=====Predicting using model====");

            var resultprediction1 = predEngine.Predict(SampleXORData.XOR1);
            var resultprediction2 = predEngine.Predict(SampleXORData.XOR2);
            var resultprediction3 = predEngine.Predict(SampleXORData.XOR3);
            var resultprediction4 = predEngine.Predict(SampleXORData.XOR4);

            // ^ : XOR operator
            bool expectedResult1 = 
                Convert.ToBoolean(SampleXORData.XOR1.Input1) ^
                Convert.ToBoolean(SampleXORData.XOR1.Input2);
            bool expectedResult2 = 
                Convert.ToBoolean(SampleXORData.XOR2.Input1) ^
                Convert.ToBoolean(SampleXORData.XOR2.Input2);
            bool expectedResult3 = 
                Convert.ToBoolean(SampleXORData.XOR3.Input1) ^
                Convert.ToBoolean(SampleXORData.XOR3.Input2);
            bool expectedResult4 = 
                Convert.ToBoolean(SampleXORData.XOR4.Input1) ^
                Convert.ToBoolean(SampleXORData.XOR4.Input2);

            const float threshold = 0.2F;

            float target1 = 0; if (expectedResult1) target1 = 1;
            float target2 = 0; if (expectedResult2) target2 = 1;
            float target3 = 0; if (expectedResult3) target3 = 1;
            float target4 = 0; if (expectedResult4) target4 = 1;
            bool succes1 = Math.Abs(resultprediction1.Score - target1) < threshold;
            bool succes2 = Math.Abs(resultprediction2.Score - target2) < threshold;
            bool succes3 = Math.Abs(resultprediction3.Score - target3) < threshold;
            bool succes4 = Math.Abs(resultprediction4.Score - target4) < threshold;

            const string format = "0.00";
            Console.WriteLine(
                SampleXORData.XOR1.Input1 + " XOR " + SampleXORData.XOR1.Input2 + " : " +
                resultprediction1.Score.ToString(format) +
                ", target:" + target1 + ", success: " + succes1 + " (" + threshold.ToString(format) + ")");
            Console.WriteLine(
                SampleXORData.XOR2.Input1 + " XOR " + SampleXORData.XOR2.Input2 + " : " +
                resultprediction2.Score.ToString(format) +
                ", target:" + target2 + ", success: " + succes2 + " (" + threshold.ToString(format) + ")");
            Console.WriteLine(SampleXORData.XOR3.Input1 + " XOR " + SampleXORData.XOR3.Input2 + " : " +
                resultprediction3.Score.ToString(format) +
                ", target:" + target3 + ", success: " + succes3 + " (" + threshold.ToString(format) + ")");
            Console.WriteLine(SampleXORData.XOR4.Input1 + " XOR " + SampleXORData.XOR4.Input2 + " : " +
                resultprediction4.Score.ToString(format) +
                ", target:" + target4 + ", success: " + succes4 + " (" + threshold.ToString(format) + ")");
        }
    }
}
