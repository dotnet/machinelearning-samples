using System;
using Common;
using System.IO;
using Microsoft.ML;

namespace ClusteringNewsArticles.Predict
{
    class Program
    {
        private static void Main()
        {
            var assetsPath = Program.GetAbsolutePath("assets");
            var newsCsv = Path.Combine(assetsPath, "inputs", "newsArticles.csv");
            var modelPath = Path.Combine(assetsPath, "inputs", "newsArticlesClustering.zip");
            var plotSvg = Path.Combine(assetsPath, "outputs", "newsArticlesClusters.svg");
            var plotCsv = Path.Combine(assetsPath, "outputs", "newsArticlesClusters.csv");

            try
            {
                var mlContext = new MLContext(null);
                var clusteringModelScorer = new ClusteringModelScorer(mlContext, newsCsv, plotSvg, plotCsv);
                clusteringModelScorer.LoadModel(modelPath);
                clusteringModelScorer.CreateNewsArticlesCluster();
            }
            catch (Exception ex)
            {
                ConsoleHelper.ConsoleWriteException(ex.ToString());
            }

            ConsoleHelper.ConsolePressAnyKey();
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
