using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using MulticlassClassification_HeartDisease.DataStructure;
using System;
using System.IO;

using Common;

namespace MulticlassClassification_HeartDisease
{
    public class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/HeartTraining.csv";
        private static string TestDataPath = $"{BaseDatasetsLocation}/HeartTest.csv";

        private static string BaseModelsPath = @"../../../../MLModels";
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

            var trainingDataView = mlContext.Data.ReadFromTextFile<HeartDataImport>(TrainDataPath, hasHeader: true, separatorChar: ',');
            var testDataView = mlContext.Data.ReadFromTextFile<HeartDataImport>(TestDataPath, hasHeader: true, separatorChar: ',');

            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", "Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca", "Thal")
                                        .AppendCacheCheckpoint(mlContext);

            // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole<HeartData>(mlContext, trainingDataView, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 5);

            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn: DefaultColumnNames.Label, featureColumn: DefaultColumnNames.Features);
            var trainingPipeline = dataProcessPipeline.Append(trainer);        
            
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingDataView);
            Console.WriteLine("=============== Finish the train model.===============");

            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score", "PredictedLabel", 0);

            Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

            Console.WriteLine("=============== Saving the model to a file ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("=============== Model Saved ============= ");
            Console.ReadKey();
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

                Console.WriteLine($" 0: {prediction.Score[0]:0.###}");
                Console.WriteLine($" 1: {prediction.Score[1]:0.###}");
                Console.WriteLine($" 2: {prediction.Score[2]:0.###}");
                Console.WriteLine($" 3: {prediction.Score[3]:0.###}");
                Console.WriteLine($" 4: {prediction.Score[4]:0.###}");
                Console.WriteLine();
                Console.ReadKey();
            }

        }
    }


}
