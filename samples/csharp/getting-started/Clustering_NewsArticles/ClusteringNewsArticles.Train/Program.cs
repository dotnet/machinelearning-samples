using System;
using Common;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;
using Microsoft.ML.Transforms.Text;

namespace ClusteringNewsArticles.Train
{
    class Program
    {
        const WordEmbeddingEstimator.PretrainedModelKind wordEmbeddingEstimator = WordEmbeddingEstimator.PretrainedModelKind.SentimentSpecificWordEmbedding;

        private static void Main(string[] args)
        {
            Console.WriteLine("=============== Validating Arguments ===============");

            var textTransform = ValidateAndExtractArgs(args);
            var assetsPath = GetAbsolutePath("assets");
            var newsArticlesCsv = Path.Combine(assetsPath, "inputs", "newsArticles.csv");
            var modelPath = Path.Combine(assetsPath, "outputs", "newsArticlesClustering.zip");

            try
            {
                var mlContext = new MLContext(1);
                var newsDataView = mlContext.Data.LoadFromTextFile(newsArticlesCsv, new[]
                {
                    new TextLoader.Column("news_articles", DataKind.String, 0)
                }, ',', true);
                IEstimator<ITransformer> dataProcessPipeline;

                switch (textTransform)
                {
                    case "ApplyWordEmbedding":
                        Console.WriteLine("=============== Applying Word Embedding ===============");
                        dataProcessPipeline = mlContext.Transforms.Text.NormalizeText("news_articles")
                            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "news_articles"))
                            .Append(mlContext.Transforms.Text.ApplyWordEmbedding("Features", "Tokens", wordEmbeddingEstimator));
                        break;
                    default:
                        Console.WriteLine("=============== Applying FeaturizeText ===============");
                        dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Features", "news_articles");
                        break;

                }

                ConsoleHelper.PeekDataViewInConsole(mlContext, newsDataView, dataProcessPipeline, 10);
                ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", newsDataView, dataProcessPipeline, 10);

                var trainer = mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 7);
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                Console.WriteLine("=============== Training the model ===============");

                ITransformer trainedModel = trainingPipeline.Fit(newsDataView);

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");

                var predictions = trainedModel.Transform(newsDataView);
                var metrics = mlContext.Clustering.Evaluate(predictions, null, "Score", "Features");

                ConsoleHelper.PrintClusteringMetrics(trainer.ToString(), metrics);

                mlContext.Model.Save(trainedModel, newsDataView.Schema, modelPath);

                Console.WriteLine("The model is saved to {0}", modelPath);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ConsoleWriteException(ex.ToString());
            }

            ConsoleHelper.ConsolePressAnyKey();
        }

        private static string ValidateAndExtractArgs(IReadOnlyList<string> argument)
        {
            var argumentOptions = new[] { "ApplyWordEmbedding", "FeaturizeText" };
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
            if (dataRoot.Directory?.Parent?.Parent?.Parent != null)
            {
                var assemblyFolderPath = dataRoot.Directory.Parent.Parent.Parent.FullName;
                return Path.Combine(assemblyFolderPath, relativePath);
            }

            throw new DirectoryNotFoundException();
        }
    }
}
