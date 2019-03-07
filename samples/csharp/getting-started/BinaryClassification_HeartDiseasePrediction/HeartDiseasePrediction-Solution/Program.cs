using System;
using System.IO;
using HeartDiseasePredictionConsoleApp.DataStructure;
using Microsoft.ML;
using Microsoft.ML.Data;


namespace HeartDiseasePredictionConsoleApp
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

            var trainingDataView = mlContext.Data.LoadFromTextFile<HeartDataImport>(TrainDataPath, hasHeader: true, separatorChar: ';');
            var testDataView = mlContext.Data.LoadFromTextFile<HeartDataImport>(TestDataPath, hasHeader: true, separatorChar: ';');

            var pipeline = mlContext.Transforms.Concatenate("Features", "Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca", "Thal")
                .Append(mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: DefaultColumnNames.Label, featureColumnName: DefaultColumnNames.Features));

            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = pipeline.Fit(trainingDataView);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("=============== Finish the train model. Push Enter ===============");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, label: DefaultColumnNames.Label, score: DefaultColumnNames.Score);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*       Metrics for {trainedModel.ToString()} binary classification model      ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"*       Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"*       Auc:      {metrics.Auc:P2}");
            Console.WriteLine($"*       Auprc:  {metrics.Auprc:P2}");
            Console.WriteLine($"*       F1Score:  {metrics.F1Score:P2}");
            Console.WriteLine($"*       LogLoss:  {metrics.LogLoss:#.##}");
            Console.WriteLine($"*       LogLossReduction:  {metrics.LogLossReduction:#.##}");
            Console.WriteLine($"*       PositivePrecision:  {metrics.PositivePrecision:#.##}");
            Console.WriteLine($"*       PositiveRecall:  {metrics.PositiveRecall:#.##}");
            Console.WriteLine($"*       NegativePrecision:  {metrics.NegativePrecision:#.##}");
            Console.WriteLine($"*       NegativeRecall:  {metrics.NegativeRecall:P2}");
            Console.WriteLine($"************************************************************");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("=============== Saving the model to a file ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("=============== Model Saved ============= ");
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

                Console.WriteLine($"=============== Single Prediction  ===============");
                Console.WriteLine($"Age: {heartData.Age} ");
                Console.WriteLine($"Sex: {heartData.Sex} ");
                Console.WriteLine($"Cp: {heartData.Cp} ");
                Console.WriteLine($"TrestBps: {heartData.TrestBps} ");
                Console.WriteLine($"Chol: {heartData.Chol} ");
                Console.WriteLine($"Fbs: {heartData.Fbs} ");
                Console.WriteLine($"RestEcg: {heartData.RestEcg} ");
                Console.WriteLine($"Thalac: {heartData.Thalac} ");
                Console.WriteLine($"Exang: {heartData.Exang} ");
                Console.WriteLine($"OldPeak: {heartData.OldPeak} ");
                Console.WriteLine($"Slope: {heartData.Slope} ");
                Console.WriteLine($"Ca: {heartData.Ca} ");
                Console.WriteLine($"Thal: {heartData.Thal} ");
                Console.WriteLine($"Prediction Value: {prediction.Prediction} ");
                Console.WriteLine($"Prediction: {(prediction.Prediction ? "A disease could be present" : "Not present disease" )} ");
                Console.WriteLine($"Probability: {prediction.Probability} ");
                Console.WriteLine($"==================================================");
                Console.WriteLine("");
                Console.WriteLine("");
            }

        }
    }
}
