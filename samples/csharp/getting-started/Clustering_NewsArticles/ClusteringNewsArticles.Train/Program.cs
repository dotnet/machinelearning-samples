using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json.Schema;

namespace ClusteringNewsArticles.Train
{
    class Program
    {
        private static void Main(string[] args)
        {
            var textTransform = ValidateArgs(args);

            var assetsPath = Program.GetAbsolutePath("assets");
            var newsArticlesCsv = Path.Combine(assetsPath, "inputs", "newsArticles.csv");
            var modelPath = Path.Combine(assetsPath, "outputs", "newsArticlesClustering.zip");

            try
            {
                var mlContext = new MLContext(1);
                var pivotDataView = mlContext.Data.LoadFromTextFile(newsArticlesCsv, new []
                {
                    new TextLoader.Column("news_articles", DataKind.String, 0)
                }, ',', true);
                IEstimator<ITransformer> dataProcessPipeline;

                switch (textTransform)
                {
                    case "ApplyWordEmbedding":
                        var wordEmbeddingEstimator = WordEmbeddingEstimator.PretrainedModelKind.SentimentSpecificWordEmbedding;
                        dataProcessPipeline = mlContext.Transforms.Text.NormalizeText("news_articles")
                            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "news_articles"))
                            .Append(mlContext.Transforms.Text.ApplyWordEmbedding("Features", "Tokens", wordEmbeddingEstimator));
                        break;
                    default:
                        dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Features", "news_articles");
                        break;

                }

                ConsoleHelper.PeekDataViewInConsole(mlContext, pivotDataView, dataProcessPipeline, 10);
                ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", pivotDataView, dataProcessPipeline, 10);

                var trainer = mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 41);
                var trainingPipeline = dataProcessPipeline.Append(trainer);
                
                Console.WriteLine("=============== Training the model ===============");
                
                ITransformer trainedModel = trainingPipeline.Fit(pivotDataView);
                
                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                
                var predictions = trainedModel.Transform(pivotDataView);
                var metrics = mlContext.Clustering.Evaluate(predictions, null, "Score", "Features");
                
                ConsoleHelper.PrintClusteringMetrics(trainer.ToString(), metrics);
                
                mlContext.Model.Save(trainedModel, pivotDataView.Schema, modelPath);
                
                Console.WriteLine("The model is saved to {0}", modelPath);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ConsoleWriteException(ex.ToString());
            }

            ConsoleHelper.ConsolePressAnyKey();
        }

        private static string ValidateArgs(IReadOnlyList<string> argument)
        {
            var argumentOptions = new[] {"ApplyWordEmbedding", "FeaturizeText" };
            var message = "Parameter passed options are 'ApplyWordEmbedding and 'FeaturizeText' default will be used 'FeaturizeText'.";

            if (!argument.Any() || !argumentOptions.ToList().Contains(argument[0]))
            {
                ConsoleHelper.ConsoleWriteWarning(message);

                return argumentOptions[1];
            }

            return argument[0];
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = dataRoot.Directory.Parent.Parent.Parent.FullName;
            return Path.Combine(assemblyFolderPath, relativePath);
        }
    }
}
