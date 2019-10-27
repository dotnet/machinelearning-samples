using System;
using System.IO;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ClusteringNewsArticles.Train
{
    class Program
    {
        private static void Main(string[] args)
        {
            var assetsPath = Program.GetAbsolutePath("assets");
            var newsArticlesCsv = Path.Combine(assetsPath, "inputs", "newsArticles.csv");
            var modelPath = Path.Combine(assetsPath, "outputs", "newsArticlesClustering.zip");
            try
            {
                var mlContext = new MLContext(new int?(1));
                var pivotDataView = mlContext.Data.LoadFromTextFile(newsArticlesCsv, new []
                {
                    new TextLoader.Column("news_articles", DataKind.String, 0)
                }, ',', true);
                
                var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Features", "news_articles");

                //var wordEmbeddingEstimator = WordEmbeddingEstimator.PretrainedModelKind.SentimentSpecificWordEmbedding;
                //var dataProcessPipeline = mlContext.Transforms.Text.NormalizeText("news_articles")
                //    .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "news_articles"))
                //    .Append(mlContext.Transforms.Text.ApplyWordEmbedding("Features","Tokens", wordEmbeddingEstimator));

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
                ConsoleHelper.ConsoleWriteException(new string[]
                {
                    ex.ToString()
                });
            }

            ConsoleHelper.ConsolePressAnyKey();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = dataRoot.Directory.Parent.Parent.Parent.FullName;
            return Path.Combine(assemblyFolderPath, relativePath);
        }
    }
}
