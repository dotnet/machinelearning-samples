
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace XORApp
{
    public static class XOR3Vector
    {
        public class XOR3Data
        {
            [LoadColumn(0,5)]
            [VectorType(6)]
            public float[] Input;

            [LoadColumn(6)]
            public float Output1;

            [LoadColumn(7)]
            public float Output2;
            
            [LoadColumn(8)]
            public float Output3;

            // ML.NET does not yet support multi-target regression
            // (only via TensorFlow and Python)
            // https://github.com/dotnet/machinelearning/issues/2134
            //[LoadColumn(6,8)]
            //[VectorType(3)]
            //public float[] Output;

            public XOR3Data(
                float input1, float input2, 
                float input3, float input4,
                float input5, float input6,
                float output1, float output2, float output3)
            {
                Input = new float[6];
                Input[0] = input1;
                Input[1] = input2;
                Input[2] = input3;
                Input[3] = input4;
                Input[4] = input5;
                Input[5] = input6;
                Output1 = output1;
                Output2 = output2;
                Output3 = output3;
            } 
        }

        public class XOR3CsvReader
        {
            public IEnumerable<XOR3Data> GetDataFromCsv(string dataLocation, int numMaxRecords)
            {
                IEnumerable<XOR3Data> records =
                    File.ReadAllLines(dataLocation)
                    .Skip(1)
                    .Select(x => x.Split('\t'))
                    .Select(x => new XOR3Data(
                        float.Parse(x[0]), float.Parse(x[1]), float.Parse(x[2]),
                        float.Parse(x[3]), float.Parse(x[4]), float.Parse(x[5]),
                        float.Parse(x[6]), float.Parse(x[7]), float.Parse(x[8])){}
                    )
                    .Take<XOR3Data>(numMaxRecords);

                return records;
            }
        }

        public class XOR3Prediction
        {
            public float Score1;
            public float Score2;
            public float Score3;
        }

        public static void TrainFromFile(MLContext mlContext,
            string ModelPath1Zip, string ModelPath2Zip, string ModelPath3Zip, 
            string TrainDataPath, out int iRowCount, out List<XOR3Data> samples)
        {
            // STEP 1: Common data loading configuration
            int iRowCountMin = 64;
            var samplesMin = new XOR3CsvReader().GetDataFromCsv(TrainDataPath, iRowCountMin).ToList();
            // Repeat 1: no repeat, same size, Repeat 2: one repeat, double size
            // minimal set: Repeat 6
            samples = Enumerable.Repeat(samplesMin, 6).SelectMany(a => a).ToList();
            iRowCount = samples.Count; // 384 = 6 * 64 lines
            var trainingDataView = mlContext.Data.LoadFromEnumerable<XOR3Data>(samples);
            var testDataView = mlContext.Data.LoadFromEnumerable<XOR3Data>(samples);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(XOR3Data.Input), nameof(XOR3Data.Input),
                    nameof(XOR3Data.Input), nameof(XOR3Data.Input),
                    nameof(XOR3Data.Input), nameof(XOR3Data.Input))
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

            const string labelColumnName3 = "Output3";
            const string outputColumnName3 = "Score3";
            Regression(
                mlContext,
                dataProcessPipeline,
                trainingDataView, testDataView,
                labelColumnName3, outputColumnName3, scoreColumnName, ModelPath3Zip);
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

        public static void TestSomePredictions(
            MLContext mlContext, 
            string ModelPath1Zip, string ModelPath2Zip, string ModelPath3Zip, 
            int iRowCount, List<XOR3Data> samples)
        {

            // Test Classification Predictions with some hard-coded samples 
            ITransformer trainedModel1 = mlContext.Model.Load(ModelPath1Zip, out var modelInputSchema1);
            ITransformer trainedModel2 = mlContext.Model.Load(ModelPath2Zip, out var modelInputSchema2);
            ITransformer trainedModel3 = mlContext.Model.Load(ModelPath3Zip, out var modelInputSchema3);

            // Create prediction engine related to the loaded trained model
            var predEngine1 = mlContext.Model.CreatePredictionEngine<XOR3Data, XOR3Prediction>(trainedModel1);
            var predEngine2 = mlContext.Model.CreatePredictionEngine<XOR3Data, XOR3Prediction>(trainedModel2);
            var predEngine3 = mlContext.Model.CreatePredictionEngine<XOR3Data, XOR3Prediction>(trainedModel3);

            Console.WriteLine("=====Predicting using model====");

            var resultPreda = new List<XOR3Prediction>();
            var resultPredb = new List<XOR3Prediction>();
            var resultPredc = new List<XOR3Prediction>();
            var expecteda = new List<Boolean>();
            var expectedb = new List<Boolean>();
            var expectedc = new List<Boolean>();
            int iNbSamples = iRowCount; 
            var targeta = new float[iNbSamples];
            var targetb = new float[iNbSamples];
            var targetc = new float[iNbSamples];
            var successa = new bool[iNbSamples];
            var successb = new bool[iNbSamples];
            var successc = new bool[iNbSamples];
            int iNumSample = 0;
            const float threshold = 0.05f;
            const string format = "0.00";
            int iNbSucces = 0;
            int iNbSuccesExpected = 0;
            Console.WriteLine("Threshold:" + threshold.ToString(format));
            foreach (var d in samples) 
            {
                resultPreda.Add(predEngine1.Predict(d));
                resultPredb.Add(predEngine2.Predict(d));
                resultPredc.Add(predEngine3.Predict(d));
                bool d1 = Convert.ToBoolean(d.Input[0]);
                bool d2 = Convert.ToBoolean(d.Input[1]);
                bool d3 = Convert.ToBoolean(d.Input[2]);
                bool d4 = Convert.ToBoolean(d.Input[3]);
                bool d5 = Convert.ToBoolean(d.Input[4]);
                bool d6 = Convert.ToBoolean(d.Input[5]);
                // ^ : XOR operator
                bool bXOR1 = d1 ^ d2;
                bool bXOR2 = d3 ^ d4;
                bool bXOR3 = d5 ^ d6;
                expecteda.Add(bXOR1);
                expectedb.Add(bXOR2);
                expectedc.Add(bXOR3);
                targeta[iNumSample] = 0; if (expecteda[iNumSample]) targeta[iNumSample] = 1;
                targetb[iNumSample] = 0; if (expectedb[iNumSample]) targetb[iNumSample] = 1;
                targetc[iNumSample] = 0; if (expectedc[iNumSample]) targetc[iNumSample] = 1;
                float dev1 = Math.Abs(resultPreda[iNumSample].Score1 - targeta[iNumSample]);
                float dev2 = Math.Abs(resultPredb[iNumSample].Score2 - targetb[iNumSample]);
                float dev3 = Math.Abs(resultPredc[iNumSample].Score3 - targetc[iNumSample]);
                bool s1 = dev1 < threshold;
                bool s2 = dev2 < threshold;
                bool s3 = dev3 < threshold;
                successa[iNumSample] = s1;
                successb[iNumSample] = s2;
                successc[iNumSample] = s3;
                iNbSucces += (s1 ? 1 : 0);
                iNbSucces += (s2 ? 1 : 0);
                iNbSucces += (s3 ? 1 : 0);
                iNbSuccesExpected += 3;

                Console.WriteLine("Sample n°" + (iNumSample + 1).ToString("00") + " : " +
                    d.Input[0] + " XOR " + d.Input[1] + " : " +
                    resultPreda[iNumSample].Score1.ToString(format) +
                    ", target:" + targeta[iNumSample] +
                    ", success: " + successa[iNumSample] + ", " +
                    d.Input[2] + " XOR " + d.Input[3] + " : " +
                    resultPredb[iNumSample].Score2.ToString(format) +
                    ", target:" + targetb[iNumSample] +
                    ", success: " + successb[iNumSample] + ", " +
                    d.Input[4] + " XOR " + d.Input[5] + " : " +
                    resultPredc[iNumSample].Score3.ToString(format) +
                    ", target:" + targetc[iNumSample] +
                    ", success: " + successc[iNumSample] + ", " +
                    iNbSucces.ToString("00") + "/" +
                    iNbSuccesExpected.ToString("00"));

                iNumSample += 1;
            }
        }
    }
}
