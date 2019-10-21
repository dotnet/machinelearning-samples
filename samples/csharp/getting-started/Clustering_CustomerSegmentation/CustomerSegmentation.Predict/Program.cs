using System;
using CustomerSegmentation.Model;
using System.IO;
using System.Threading.Tasks;
using Common;
using Microsoft.ML;

namespace CustomerSegmentation
{
    public class Program
    {
        static void Main(string[] args)
        {
            var assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            var pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv");
            var modelPath = Path.Combine(assetsPath, "inputs", "retailClustering.zip");
            var plotSvg = Path.Combine(assetsPath, "outputs", "customerSegmentation.svg");
            var plotCsv = Path.Combine(assetsPath, "outputs", "customerSegmentation.csv");

            try
            {
                MLContext mlContext = new MLContext();  //Seed set to any number so you have a deterministic results

                //Create the clusters: Create data files and plot a chart
                var clusteringModelScorer = new ClusteringModelScorer(mlContext, pivotCsv, plotSvg, plotCsv);
                clusteringModelScorer.LoadModel(modelPath);

                clusteringModelScorer.CreateCustomerClusters();
            } catch (Exception ex)
            {
                Common.ConsoleHelper.ConsoleWriteException(ex.ToString());
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
