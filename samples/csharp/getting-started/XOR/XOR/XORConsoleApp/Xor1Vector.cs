
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace XORApp
{

    public static class XOR1Vector
    {
        public class XORData
        {
            [LoadColumn(0,1)]
            [VectorType(2)]
            public float[] Input;
            
            [LoadColumn(2)]
            public float Output;

            public XORData(float input1, float input2)
            {
                Input = new float[2];
                Input[0] = input1;
                Input[1] = input2;
            }
            public XORData(float input1, float input2, float output)
            {
                Input = new float[2];
                Input[0] = input1;
                Input[1] = input2;
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
                    return (Input[0] == p.Input[0]) && 
                           (Input[1] == p.Input[1]) && 
                           (Output == p.Output);
                }
            }

        }

        public class XORPrediction {
            //public float[] Score;
            public float Score;
        }

        public class SampleXORData {
            internal static readonly XORData XOR1 = new XORData(1.0F, 0.0F);
            internal static readonly XORData XOR2 = new XORData(0.0F, 0.0F);
            internal static readonly XORData XOR3 = new XORData(0.0F, 1.0F);
            internal static readonly XORData XOR4 = new XORData(1.0F, 1.0F);
        }

        public static List<XORData> LoadData()
        {
            // STEP 1: Common data loading configuration

            // Récup depuis : https://stackoverflow.com/questions/53472759/why-does-this-ml-net-code-fail-to-predict-the-correct-output
            var data = new List<XORData>
            {
                new XORData(1.0F, 0.0F, 1.0F),
                new XORData(0.0F, 0.0F, 0.0F),
                new XORData(0.0F, 1.0F, 1.0F),
                new XORData(1.0F, 1.0F, 0.0F)
            };
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
                    "Features", nameof(XORData.Input), nameof(XORData.Input))
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

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features", nameof(XORData.Input), nameof(XORData.Input))
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
                Convert.ToBoolean(SampleXORData.XOR1.Input[0]) ^
                Convert.ToBoolean(SampleXORData.XOR1.Input[1]);
            bool expectedResult2 = 
                Convert.ToBoolean(SampleXORData.XOR2.Input[0]) ^
                Convert.ToBoolean(SampleXORData.XOR2.Input[1]);
            bool expectedResult3 = 
                Convert.ToBoolean(SampleXORData.XOR3.Input[0]) ^
                Convert.ToBoolean(SampleXORData.XOR3.Input[1]);
            bool expectedResult4 = 
                Convert.ToBoolean(SampleXORData.XOR4.Input[0]) ^
                Convert.ToBoolean(SampleXORData.XOR4.Input[1]);

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
                SampleXORData.XOR1.Input[0] + " XOR " + SampleXORData.XOR1.Input[1] + " : " +
                resultprediction1.Score.ToString(format) +
                ", target:" + target1 + ", success: " + succes1 + " (" + threshold.ToString(format) + ")");
            Console.WriteLine(
                SampleXORData.XOR2.Input[0] + " XOR " + SampleXORData.XOR2.Input[1] + " : " +
                resultprediction2.Score.ToString(format) +
                ", target:" + target2 + ", success: " + succes2 + " (" + threshold.ToString(format) + ")");
            Console.WriteLine(SampleXORData.XOR3.Input[0] + " XOR " + SampleXORData.XOR3.Input[1] + " : " +
                resultprediction3.Score.ToString(format) +
                ", target:" + target3 + ", success: " + succes3 + " (" + threshold.ToString(format) + ")");
            Console.WriteLine(SampleXORData.XOR4.Input[0] + " XOR " + SampleXORData.XOR4.Input[1] + " : " +
                resultprediction4.Score.ToString(format) +
                ", target:" + target4 + ", success: " + succes4 + " (" + threshold.ToString(format) + ")");
        }
    }
}
