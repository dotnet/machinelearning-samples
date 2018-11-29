using System;
using Microsoft.ML;
using MulticlassClassification_HeartDisease.DataStructure;

namespace MulticlassClassification_HeartDisease
{
    public class Program
    {
        //private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../HeartPrediction/DataFiles";
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

            var textLoader = HeartTextLoaderFactory.CreateTextLoader(mlContext);

            var trainingDataView = textLoader.Read(TrainDataPath);

            var testDataView = textLoader.Read(TestDataPath);


            var dataProcess = mlContext.Transforms.Concatenate("Features",
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

            var modelBuilder =
                new Common.ModelBuilder<HeartData, MulticlassClassification_HeartDisease.DataStructure.HeartPrediction>(mlContext, dataProcess);

            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent();
            modelBuilder.AddTrainer(trainer);

            Console.WriteLine("=============== Training the model ===============");
            modelBuilder.Train(trainingDataView);
            Console.WriteLine();


            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var metrics = modelBuilder.EvaluateMultiClassClassificationModel(testDataView, "Label");
            Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);
            Console.WriteLine();

            Console.WriteLine("=============== Saving the model to a file ===============");
            modelBuilder.SaveModelAsFile(ModelPath);
            Console.WriteLine();

        }


        private static void TestPrediction(MLContext mlContext)
        {
            var modelScorer = new Common.ModelScorer<HeartData, MulticlassClassification_HeartDisease.DataStructure.HeartPrediction>(mlContext);
            modelScorer.LoadModelFromZipFile(ModelPath);
         

            foreach (var heartData in HeartSampleData.heartDatas)
            {
                var prediction = modelScorer.PredictSingle(heartData);

                Console.WriteLine($" 0: {prediction.Score[0]:0.###}");
                Console.WriteLine($" 1: {prediction.Score[1]:0.###}");
                Console.WriteLine($" 2: {prediction.Score[2]:0.###}");
                Console.WriteLine($" 3: {prediction.Score[3]:0.###}");
                Console.WriteLine($" 4: {prediction.Score[4]:0.###}");
                Console.WriteLine();

            }

        }
    }
}
