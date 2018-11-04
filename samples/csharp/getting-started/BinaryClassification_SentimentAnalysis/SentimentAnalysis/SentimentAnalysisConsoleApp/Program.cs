using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using Microsoft.ML.Trainers;

using SentimentAnalysisConsoleApp.DataStructures;
using Microsoft.ML.Transforms.Text;

namespace SentimentAnalysisConsoleApp
{

    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/wikipedia-detox-250-line-data.tsv";
        private static string TestDataPath = $"{BaseDatasetsLocation}/wikipedia-detox-250-line-test.tsv";

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelPath = $"{BaseModelsPath}/SentimentModel.zip";

        static void Main(string[] args)
        {
            //1. Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext();

            //2. Create DataReader with data schema mapped to file's columns
            var reader = new TextLoader(mlContext,
                                        new TextLoader.Arguments()
                                        {
                                            Separator = "tab",
                                            HasHeader = true,
                                            Column = new[]
                                            {
                                                new TextLoader.Column("Label", DataKind.Bool, 0),
                                                new TextLoader.Column("Text", DataKind.Text, 1)
                                            }
                                        });

            //Load training data
            IDataView trainingDataView = reader.Read(new MultiFileSource(TrainDataPath));


            //3.Create a flexible pipeline (composed by a chain of estimators) for creating/traing the model.

            var trainer = new LinearClassificationTrainer(mlContext,"Features","Label");
            //(TBD) var trainer = mlContext.BinaryClassification.Trainers.???("Features", "Label");

            var pipeline = new TextFeaturizingEstimator(mlContext, "Text", "Features")  //Convert the text column to numeric vectors (Features column)  
                                        .Append(trainer);                                                                
                                        
            //4. Create and train the model            
            Console.WriteLine("=============== Create and Train the Model ===============");

            var model = pipeline.Fit(trainingDataView);

            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();


            //5. Evaluate the model and show accuracy stats

            //Load evaluation/test data
            IDataView testDataView = reader.Read(new MultiFileSource(TestDataPath));

            Console.WriteLine("=============== Evaluating Model's accuracy with Test data===============");
            var predictions = model.Transform(testDataView);

            var binClassificationCtx = new BinaryClassificationContext(mlContext);
            var metrics = binClassificationCtx.Evaluate(predictions, "Label");

            Console.WriteLine();
            Console.WriteLine("Model quality metrics evaluation");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of Model's evaluation ===============");
            Console.WriteLine();


            //6. Test Sentiment Prediction with one sample text 
            var predictionFunct = model.MakePredictionFunction<SentimentIssue, SentimentPrediction>(mlContext);

            SentimentIssue sampleStatement = new SentimentIssue
            {
                Text = "This is a very rude movie"
            };

            var resultprediction = predictionFunct.Predict(sampleStatement);

            Console.WriteLine();
            Console.WriteLine("=============== Test of model with a sample ===============");

            Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Toxic" : "Nice")} sentiment | Probability: {resultprediction.Probability} ");

            // Save model to .ZIP file
            SaveModelAsFile(mlContext, model);

            // Predict again but now testing the model loading from the .ZIP file
            PredictWithModelLoadedFromFile(sampleStatement);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
            
        }

        private static void SaveModelAsFile(MLContext env, TransformerChain<BinaryPredictionTransformer<Microsoft.ML.Runtime.Internal.Internallearn.IPredictorWithFeatureWeights<float>>> model)
        {
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                model.SaveTo(env, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private static void PredictWithModelLoadedFromFile(SentimentIssue sampleStatement)
        {
            // Test with Loaded Model from .zip file

            var env2 = new MLContext();
            
                ITransformer loadedModel;
                using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    loadedModel = TransformerChain.LoadFrom(env2, stream);
                }

                // Create prediction engine and make prediction.

                var engine = loadedModel.MakePredictionFunction<SentimentIssue, SentimentPrediction>(env2);

                var predictionFromLoaded = engine.Predict(sampleStatement);

                Console.WriteLine();
                Console.WriteLine("=============== Test of model with a sample ===============");

                Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(predictionFromLoaded.Prediction) ? "Toxic" : "Nice")} sentiment | Probability: {predictionFromLoaded.Probability} ");

            
        }


    }
}