
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace XORApp
{
    public static class XOR2Vector
    {
        public class XOR2Data
        {
            [LoadColumn(0,3)]
            [VectorType(4)]
            public float[] Input;

            [LoadColumn(4)]
            public float Output1;

            [LoadColumn(5)]
            public float Output2;

            // ML.NET does not yet support multi-target regression
            // (only via TensorFlow and Python)
            // https://github.com/dotnet/machinelearning/issues/2134
            //[LoadColumn(4,5)]
            //[VectorType(2)]
            //public float[] Output;

            public XOR2Data(float input1, float input2, float input3, float input4, float output1, float output2)
            {
                Input = new float[4];
                Input[0] = input1;
                Input[1] = input2;
                Input[2] = input3;
                Input[3] = input4;
                Output1 = output1;
                Output2 = output2;
            }
        }

        public class XOR2Prediction
        {
            public float Score1;
            public float Score2;
        }

        public class SampleXOR2Data
        {
            public const int iNbSamples = 16;

            internal static readonly XOR2Data XOR01 = new XOR2Data(1.0F, 0.0F, 1.0F, 0.0F, 1.0F, 1.0F);
            internal static readonly XOR2Data XOR02 = new XOR2Data(1.0F, 0.0F, 0.0F, 0.0F, 1.0F, 0.0F);
            internal static readonly XOR2Data XOR03 = new XOR2Data(1.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);
            internal static readonly XOR2Data XOR04 = new XOR2Data(1.0F, 0.0F, 1.0F, 1.0F, 1.0F, 0.0F);

            internal static readonly XOR2Data XOR05 = new XOR2Data(0.0F, 0.0F, 1.0F, 0.0F, 0.0F, 1.0F);
            internal static readonly XOR2Data XOR06 = new XOR2Data(0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 0.0F);
            internal static readonly XOR2Data XOR07 = new XOR2Data(0.0F, 0.0F, 0.0F, 1.0F, 0.0F, 1.0F);
            internal static readonly XOR2Data XOR08 = new XOR2Data(0.0F, 0.0F, 1.0F, 1.0F, 0.0F, 0.0F);

            internal static readonly XOR2Data XOR09 = new XOR2Data(0.0F, 1.0F, 1.0F, 0.0F, 1.0F, 1.0F);
            internal static readonly XOR2Data XOR10 = new XOR2Data(0.0F, 1.0F, 0.0F, 0.0F, 1.0F, 0.0F);
            internal static readonly XOR2Data XOR11 = new XOR2Data(0.0F, 1.0F, 0.0F, 1.0F, 1.0F, 1.0F);
            internal static readonly XOR2Data XOR12 = new XOR2Data(0.0F, 1.0F, 1.0F, 1.0F, 1.0F, 0.0F);

            internal static readonly XOR2Data XOR13 = new XOR2Data(1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 1.0F);
            internal static readonly XOR2Data XOR14 = new XOR2Data(1.0F, 1.0F, 0.0F, 0.0F, 0.0F, 0.0F);
            internal static readonly XOR2Data XOR15 = new XOR2Data(1.0F, 1.0F, 0.0F, 1.0F, 0.0F, 1.0F);
            internal static readonly XOR2Data XOR16 = new XOR2Data(1.0F, 1.0F, 1.0F, 1.0F, 0.0F, 0.0F);
        }

        public static List<XOR2Data> LoadData()
        {
            // STEP 1: Common data loading configuration
            // From: https://stackoverflow.com/questions/53472759/why-does-this-ml-net-code-fail-to-predict-the-correct-output
            var data = new List<XOR2Data>
            {
                SampleXOR2Data.XOR01, SampleXOR2Data.XOR02, SampleXOR2Data.XOR03, SampleXOR2Data.XOR04,
                SampleXOR2Data.XOR05, SampleXOR2Data.XOR06, SampleXOR2Data.XOR07, SampleXOR2Data.XOR08,
                SampleXOR2Data.XOR09, SampleXOR2Data.XOR10, SampleXOR2Data.XOR11, SampleXOR2Data.XOR12,
                SampleXOR2Data.XOR13, SampleXOR2Data.XOR14, SampleXOR2Data.XOR15, SampleXOR2Data.XOR16,
            };
            // minimal set: Repeat 3
            var largeSet = Enumerable.Repeat(data, 3).SelectMany(a => a).ToList();
            return largeSet;
        }
        
        public class XOR2CsvReader
        {
            public IEnumerable<XOR2Data> GetDataFromCsv(string dataLocation, int numMaxRecords)
            {
                IEnumerable<XOR2Data> records =
                    File.ReadAllLines(dataLocation)
                    .Skip(1)
                    .Select(x => x.Split('\t'))
                    .Select(x => new XOR2Data(
                        float.Parse(x[0]), float.Parse(x[1]), float.Parse(x[2]),
                        float.Parse(x[3]), float.Parse(x[4]), float.Parse(x[5]))
                    { }
                    )
                    .Take<XOR2Data>(numMaxRecords);

                return records;
            }
        }

        public static void Train(MLContext mlContext, 
            string ModelPath1Zip, string ModelPath2Zip, List<XOR2Data> largeSet)
        {
            var trainingDataView = mlContext.Data.LoadFromEnumerable<XOR2Data>(largeSet);
            var testDataView = mlContext.Data.LoadFromEnumerable<XOR2Data>(largeSet);

            // Check if the Repeat function works fine
            int iNumSample = 0;
            int iNumSample16 = 0;
            int iNbSuccess = 0;
            var data = LoadData();
            foreach (var d in largeSet)
            {
                bool b = d == data[iNumSample16];
                if (b) iNbSuccess++;
                iNumSample++;
                iNumSample16++;
                if (iNumSample16 >= 16) iNumSample16 = 0;
            } 

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(XOR2Data.Input), nameof(XOR2Data.Input),
                    nameof(XOR2Data.Input), nameof(XOR2Data.Input))
                .AppendCacheCheckpoint(mlContext);
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            const int iNbSamples = 17;
            ConsoleHelper.PeekDataViewInConsole(mlContext,
                trainingDataView, dataProcessPipeline, iNbSamples);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext,
                "Features", trainingDataView, dataProcessPipeline, iNbSamples);

            const string scoreColumnName = "Score";

            const string labelColumnName1 = "Output1";
            const string outputColumnName1 = "Score1";
            Regression(
                mlContext,
                dataProcessPipeline,
                trainingDataView, testDataView,
                labelColumnName1, outputColumnName1, scoreColumnName, ModelPath1Zip);

            const string labelColumnName2 = "Output2";
            const string outputColumnName2 = "Score2";
            Regression(
                mlContext,
                dataProcessPipeline,
                trainingDataView, testDataView,
                labelColumnName2, outputColumnName2, scoreColumnName, ModelPath2Zip);
        }

        private static void Regression(
            MLContext mlContext,
            EstimatorChain<ColumnConcatenatingTransformer> dataProcessPipeline,
            IDataView trainingDataView, IDataView testDataView,
            string labelColumnName, string outputColumnName, string scoreColumnName,
            string ModelPathZip)
        {
            // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
            var trainer = mlContext.Regression.Trainers.FastTree(
                labelColumnName: labelColumnName, featureColumnName: "Features",
                learningRate: 0.2); // min: 0.03, max: 0.9
            
            var trainingPipeline =
                dataProcessPipeline.Append(trainer)
                .Append(mlContext.Transforms.CopyColumns(
                    outputColumnName: outputColumnName, inputColumnName: scoreColumnName));
            
            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);
            
            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(
                predictions, 
                labelColumnName: labelColumnName, 
                scoreColumnName: scoreColumnName);
            ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            var directoryPath = Path.GetDirectoryName(ModelPathZip);
            if (!Directory.Exists(directoryPath))
            {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                di.Create();
            }
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPathZip);
            Console.WriteLine("The model is saved to {0}", Path.GetFullPath(ModelPathZip));
        }

        public static void TrainFromFile(MLContext mlContext,
            string ModelPath1Zip, string ModelPath2Zip,
            string TrainDataPath, string TrainDataPathR, 
            out int iRowCount, out List<XOR2Data> samples, bool bRepeat)
        {
            // STEP 1: Common data loading configuration

            int iRowCountMin = 16;
            IDataView trainingDataView, testDataView;
            if (bRepeat)
            {
                // Repeat the minimal file
                var samplesMin = new XOR2CsvReader().GetDataFromCsv(TrainDataPath, iRowCountMin).ToList();
                // Repeat 1: no repeat, same size, Repeat 2: one repeat, double size
                // minimal set: Repeat 3
                samples = Enumerable.Repeat(samplesMin, 3).SelectMany(a => a).ToList();
                iRowCount = samples.Count; // 48 = 16 * 3 lines
                trainingDataView = mlContext.Data.LoadFromEnumerable<XOR2Data>(samples);
                testDataView = mlContext.Data.LoadFromEnumerable<XOR2Data>(samples);
            } else
            {
                // File is yet repeated
                trainingDataView = mlContext.Data.LoadFromTextFile<XOR2Data>(
                    TrainDataPathR, hasHeader: true);
                testDataView = mlContext.Data.LoadFromTextFile<XOR2Data>(
                    TrainDataPathR, hasHeader: true);
                samples = new XOR2CsvReader().GetDataFromCsv(TrainDataPathR, 48).ToList();
                iRowCount = samples.Count;
            }

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features", 
                    nameof(XOR2Data.Input), nameof(XOR2Data.Input),
                    nameof(XOR2Data.Input), nameof(XOR2Data.Input))
                .AppendCacheCheckpoint(mlContext);
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            const int iNbSamples = 17;
            ConsoleHelper.PeekDataViewInConsole(mlContext,
                trainingDataView, dataProcessPipeline, iNbSamples);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext,
                "Features", trainingDataView, dataProcessPipeline, iNbSamples);

            const string scoreColumnName = "Score";

            const string labelColumnName1 = "Output1";
            const string outputColumnName1 = "Score1";
            Regression(
                mlContext,
                dataProcessPipeline,
                trainingDataView, testDataView,
                labelColumnName1, outputColumnName1, scoreColumnName, ModelPath1Zip);

            const string labelColumnName2 = "Output2";
            const string outputColumnName2 = "Score2";
            Regression(
                mlContext,
                dataProcessPipeline,
                trainingDataView, testDataView,
                labelColumnName2, outputColumnName2, scoreColumnName, ModelPath2Zip);
        }

        public static void TestSomePredictions(
            MLContext mlContext, string ModelPath1Zip, string ModelPath2Zip,
            int iRowCount, List<XOR2Data> samples)
        {

            // Test Classification Predictions with some hard-coded samples 
            ITransformer trainedModel1 = mlContext.Model.Load(ModelPath1Zip, out var modelInputSchema1);
            ITransformer trainedModel2 = mlContext.Model.Load(ModelPath2Zip, out var modelInputSchema2);

            // Create prediction engine related to the loaded trained model
            var predEngine1 = mlContext.Model.CreatePredictionEngine<XOR2Data, XOR2Prediction>(trainedModel1);
            var predEngine2 = mlContext.Model.CreatePredictionEngine<XOR2Data, XOR2Prediction>(trainedModel2);

            Console.WriteLine("=====Predicting using model====");

            var listData = samples;
            var resultPreda = new List<XOR2Prediction>();
            var resultPredb = new List<XOR2Prediction>();
            var expecteda = new List<Boolean>();
            var expectedb = new List<Boolean>();
            int iNbSamples = iRowCount;
            var targeta = new float[iNbSamples];
            var targetb = new float[iNbSamples];
            var successa = new bool[iNbSamples];
            var successb = new bool[iNbSamples];
            int iNumSample = 0;
            const float threshold = 0.05f;
            const string format = "0.00";
            int iNbSucces = 0;
            int iNbSuccesExpected = 0;
            Console.WriteLine("Threshold:" + threshold.ToString(format));
            foreach (var d in listData)
            {
                resultPreda.Add(predEngine1.Predict(d));
                resultPredb.Add(predEngine2.Predict(d));
                bool d1 = Convert.ToBoolean(d.Input[0]);
                bool d2 = Convert.ToBoolean(d.Input[1]);
                bool d3 = Convert.ToBoolean(d.Input[2]);
                bool d4 = Convert.ToBoolean(d.Input[3]);
                // ^ : XOR operator
                bool bXOR1 = d1 ^ d2;
                bool bXOR2 = d3 ^ d4;
                expecteda.Add(bXOR1);
                expectedb.Add(bXOR2);
                targeta[iNumSample] = 0; if (expecteda[iNumSample]) targeta[iNumSample] = 1;
                targetb[iNumSample] = 0; if (expectedb[iNumSample]) targetb[iNumSample] = 1;
                float dev1 = Math.Abs(resultPreda[iNumSample].Score1 - targeta[iNumSample]);
                float dev2 = Math.Abs(resultPredb[iNumSample].Score2 - targetb[iNumSample]);
                bool s1 = dev1 < threshold;
                bool s2 = dev2 < threshold;
                successa[iNumSample] = s1;
                successb[iNumSample] = s2;
                iNbSucces += (s1 ? 1 : 0);
                iNbSucces += (s2 ? 1 : 0);
                iNbSuccesExpected += 2;

                Console.WriteLine("Sample n°" + (iNumSample + 1).ToString("00") + " : " +
                    d.Input[0] + " XOR " + d.Input[1] + " : " +
                    resultPreda[iNumSample].Score1.ToString(format) +
                    ", target:" + targeta[iNumSample] +
                    ", success: " + successa[iNumSample] + ", " +
                    d.Input[2] + " XOR " + d.Input[3] + " : " +
                    resultPredb[iNumSample].Score2.ToString(format) +
                    ", target:" + targetb[iNumSample] +
                    ", success: " + successb[iNumSample] + ", " +
                    iNbSucces.ToString("00") + "/" +
                    iNbSuccesExpected.ToString("00"));

                iNumSample += 1;
            }
        }
    }
}
