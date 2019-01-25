using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using MulticlassClassification_HeartDisease.DataStructure;
using System;
using System.IO;

namespace MulticlassClassification_HeartDisease
{
    public class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../DataFiles";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/HeartTraining.csv";
        private static string TestDataPath = $"{BaseDatasetsLocation}/HeartTest.csv";

        private static string BaseModelsPath = @"../../../../HeartPrediction/MLModels";
        private static string ModelPath = $"{BaseModelsPath}/HeartClassification.zip";

        public static void Main(string[] args)
        {
            var mlContext = new MLContext();
            BuildTrainEvaluateAndSaveModel(mlContext);

            TestPrediction(mlContext);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {

            var trainingDataView = mlContext.Data.ReadFromTextFile<HeartData>(TrainDataPath, hasHeader: true, separatorChar: ',');

            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features",
                "Age",
                "Sex", 
                "Cp", 
                "TrestBps",
                "Chol",
                "Fbs",
                "RestEcg", 
                "Thalac",
                "Exang", 
                "OldPeak", 
                "Slope", 
                "Ca", 
                "Thal"
            );

            IEstimator<ITransformer> trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(DefaultColumnNames.Label,DefaultColumnNames.Features);

            trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent();
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingDataView);
            Console.WriteLine();


            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var metrics = mlContext.MulticlassClassification.Evaluate(trainingDataView, "Label", "score", "PredictedLabel", 0);


            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*    Metrics for {trainer.ToString()} multi-class classification model   ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better");
            Console.WriteLine($"************************************************************");
            Console.WriteLine();

            Console.WriteLine("=============== Saving the model to a file ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine();

        }


        private static void TestPrediction(MLContext mlContext)
        {
            ITransformer trainedModel;

            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }
            var predictionEngine = trainedModel.CreatePredictionEngine<HeartData, HeartPrediction>(mlContext);


            foreach (var heartData in HeartSampleData.heartDatas)
            {
                var prediction = predictionEngine.Predict(heartData);

                Console.WriteLine($" 0: {prediction.Score:0.###}");
                Console.WriteLine($" 1: {prediction.Score[1]:0.###}");
                Console.WriteLine($" 2: {prediction.Score[2]:0.###}");
                Console.WriteLine($" 3: {prediction.Score[3]:0.###}");
                Console.WriteLine();

            }

        }
    }
}
