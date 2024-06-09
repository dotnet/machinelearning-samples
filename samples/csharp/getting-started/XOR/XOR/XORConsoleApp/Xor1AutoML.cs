
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace XORApp
{

    public static class XOR1AutoML
    {
        public class XORData
        {
            [LoadColumn(0)]
            public float Input1;

            [LoadColumn(1)]
            public float Input2;
                        
            [LoadColumn(2)]
            public float Output;

            public XORData(float input1, float input2)
            {
                Input1 = input1;
                Input2 = input2;
            }

            //public XORData(float input1, float input2, float output)
            //{
            //    Input1 = input1;
            //    Input2 = input2;
            //    Output = output;
            //}
        }

        public class XORPrediction
        {
            public float Score;
        }

        public class SampleXORData
        {
            internal static readonly XORData XOR1 = new XORData(1.0F, 0.0F);
            internal static readonly XORData XOR2 = new XORData(0.0F, 0.0F);
            internal static readonly XORData XOR3 = new XORData(0.0F, 1.0F);
            internal static readonly XORData XOR4 = new XORData(1.1F, 1.0F);
        }
        
        /// <summary>
        /// Infer columns in the dataset with AutoML.
        /// </summary>
        private static ColumnInferenceResults InferColumns(MLContext mlContext,
            string TrainDataPath, string LabelColumnName)
        {
            ConsoleHelperAutoML.ConsoleWriteHeader("=============== Inferring columns in dataset ===============");
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(
                TrainDataPath, LabelColumnName, groupColumns: false);
            ConsoleHelperAutoML.Print(columnInference);
            return columnInference;
        }

        public static void TrainFromFile(MLContext mlContext, 
            string ModelPath, string TrainDataPath)
        {
            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<XORData>(
                TrainDataPath, hasHeader: true, separatorChar: ',');
            var testDataView = mlContext.Data.LoadFromTextFile<XORData>(
                TrainDataPath, hasHeader: true, separatorChar: ',');

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                    "Features", nameof(XORData.Input1), nameof(XORData.Input2))
                .AppendCacheCheckpoint(mlContext);
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            ConsoleHelper.PeekDataViewInConsole(mlContext,
                trainingDataView, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext,
                "Features", trainingDataView, dataProcessPipeline, 5);

            // Infer columns in the dataset with AutoML
            var columnInference = InferColumns(mlContext, TrainDataPath,
                LabelColumnName: "Output");
            // Run an AutoML experiment on the dataset
            var experimentResult = RunAutoMLExperiment(mlContext, 
                columnInference, trainingDataView);

            var directoryPath = Path.GetDirectoryName(ModelPath);
            if (!Directory.Exists(directoryPath))
            {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                di.Create();
            }
            mlContext.Model.Save(experimentResult.BestRun.Model, 
                trainingDataView.Schema, ModelPath);
        }

        private static ExperimentResult<RegressionMetrics> RunAutoMLExperiment(
            MLContext mlContext, ColumnInferenceResults columnInference, 
            IDataView TrainDataView)
        {
            // STEP 1: Display first few rows of the training data.
            ConsoleHelper.ShowDataViewInConsole(mlContext, TrainDataView);

            // STEP 2: Build a pre-featurizer for use in the AutoML experiment.
            // (Internally, AutoML uses one or more train/validation data splits to 
            // evaluate the models it produces. The pre-featurizer is fit only on the 
            // training data split to produce a trained transform. Then, the trained transform 
            // is applied to both the train and validation data splits.)
            /*
            IEstimator<ITransformer> preFeaturizer = 
                mlContext.Transforms.Conversion.MapValue(
                    "Output2",
                    new[] { new KeyValuePair<string, bool>("XOR", true) }, "Output");
            */

            // STEP 3: Customize column information returned by InferColumns API.
            ColumnInformation columnInformation = columnInference.ColumnInformation;
            //columnInformation.CategoricalColumnNames.Remove("x");
            //columnInformation.IgnoredColumnNames.Add("y");

            // STEP 4: Initialize a cancellation token source to stop the experiment.
            var cts = new CancellationTokenSource();

            // STEP 5: Initialize our user-defined progress handler that AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RegressionExperimentProgressHandler();

            // STEP 6: Create experiment settings
            var experimentSettings = CreateExperimentSettings(mlContext, cts);

            // STEP 7: Run AutoML regression experiment.
            var experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings);
            ConsoleHelper.ConsoleWriteHeader("=============== Running AutoML experiment ===============");
            Console.WriteLine($"Running AutoML regression experiment...");
            var stopwatch = Stopwatch.StartNew();
            // Cancel experiment after the user presses any key.
            CancelExperimentAfterAnyKeyPress(cts);
            ExperimentResult<RegressionMetrics> experimentResult = 
                experiment.Execute(
                    TrainDataView, columnInformation, 
                    progressHandler:progressHandler); // preFeaturizer, 
            Console.WriteLine($"{experimentResult.RunDetails.Count()} models were returned after {stopwatch.Elapsed.TotalSeconds:0.00} seconds{Environment.NewLine}");

            // Print top models found by AutoML.
            PrintTopModels(experimentResult);

            return experimentResult;
        }
        
        /// <summary>
        /// Create AutoML regression experiment settings.
        /// </summary>
        private static RegressionExperimentSettings CreateExperimentSettings(MLContext mlContext,
            CancellationTokenSource cts)
        {
            var experimentSettings = new RegressionExperimentSettings();
            experimentSettings.MaxExperimentTimeInSeconds = 3600;
            experimentSettings.CancellationToken = cts.Token;

            // Set the metric that AutoML will try to optimize over the course of the experiment.
            experimentSettings.OptimizingMetric = RegressionMetric.RootMeanSquaredError;

            // Set the cache directory to null.
            // This will cause all models produced by AutoML to be kept in memory 
            // instead of written to disk after each run, as AutoML is training.
            // (Please note: for an experiment on a large dataset, opting to keep all 
            // models trained by AutoML in memory could cause your system to run out 
            // of memory.)
            //experimentSettings.CacheDirectory = null; // CS1061

            // Don't use LbfgsPoissonRegression and OnlineGradientDescent trainers during this experiment.
            // (These trainers sometimes underperform on this dataset.)
            //experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression);
            //experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent);

            return experimentSettings;
        }
        
        private static void CancelExperimentAfterAnyKeyPress(CancellationTokenSource cts)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Press any key to stop the experiment run...");
                Console.ReadKey();
                cts.Cancel();
            });
        }
        
        /// <summary>
        /// Print top models from AutoML experiment.
        /// </summary>
        private static void PrintTopModels(ExperimentResult<RegressionMetrics> experimentResult)
        {
            // Get top few runs ranked by root mean squared error.
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.RootMeanSquaredError))
                .OrderBy(r => r.ValidationMetrics.RootMeanSquaredError).Take(3);

            Console.WriteLine("Top models ranked by root mean squared error --");
            ConsoleHelperAutoML.PrintRegressionMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelperAutoML.PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
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
