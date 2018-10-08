using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Training;
using Microsoft.ML;
using Microsoft.ML.StaticPipe;
using Microsoft.ML.Trainers;

namespace BinaryClassification_SentimentAnalysis
{

    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "wikipedia-detox-250-line-data.tsv");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "wikipedia-detox-250-line-test.tsv");
        private static string ModelPath => Path.Combine(AppPath, "SentimentModel.zip");


        static void Main(string[] args)
        {
            //1. Create ML.NET context/environment
            var env = new LocalEnvironment();

            //2. Create DataReader with data schema mapped to file's columns
            var reader = TextLoader.CreateReader(env, ctx => (label: ctx.LoadBool(0),
                                                              text: ctx.LoadText(1)));

            //3. Create an estimator to use afterwards for creating/traing the model.

            var bctx = new BinaryClassificationContext(env);

            var est = reader.MakeNewEstimator().Append(row =>
            {
                var featurizedText = row.text.FeaturizeText();  //Convert text to numeric vectors
                var prediction = bctx.Trainers.Sdca(row.label, featurizedText);  //Specify SDCA trainer based on the label and featurized text columns
                return (row.label, prediction);  //Return label and prediction columns. "prediction" holds predictedLabel, score and probability
            });

            //Another way to create an Estimator, with the same behaviour, by chaining appends
            //var est = reader.MakeNewEstimator().Append(row => (label: row.label,
            //                                                  featurizedtext: row.text.FeaturizeText()))  //Convert text to numeric vectors                                  
            //                                   .Append(row => (label: row.label,
            //                                                  prediction: bctx.Trainers.Sdca(row.label, row.featurizedtext)));  //Specify SDCA trainer based on the label and featurized text columns


            //4. Build and train the model

            //Load training data
            var traindata = reader.Read(new MultiFileSource(TrainDataPath));

            Console.WriteLine("=============== Create and Train the Model ===============");
            var model = est.Fit(traindata);
            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();


            //5. Evaluate the model
            
            //Load test data
            var testdata = reader.Read(new MultiFileSource(TestDataPath));

            Console.WriteLine("=============== Evaluating Model's accuracy with Test data===============");
            var predictions = model.Transform(testdata);
            var metrics = bctx.Evaluate(predictions, row => row.label, row => row.prediction);

            Console.WriteLine();
            Console.WriteLine("PredictionModel quality metrics evaluation");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of Model's evaluation ===============");
            Console.WriteLine();

            //6. Test Sentiment Prediction with one sample text 
            var predictionFunct = model.AsDynamic.MakePredictionFunction<SentimentIssue, SentimentPrediction>(env);

            SentimentIssue sampleStatement = new SentimentIssue
                                        {
                                            text = "This is a very rude movie"
                                        };

            var resultprediction = predictionFunct.Predict(sampleStatement);

            Console.WriteLine();
            Console.WriteLine("=============== Test of model with a sample ===============");
            Console.WriteLine($"Text: {sampleStatement.text} | Prediction: {(resultprediction.PredictionLabel ? "Negative" : "Positive")} sentiment | Probability: {resultprediction.Probability} ");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

    }
}