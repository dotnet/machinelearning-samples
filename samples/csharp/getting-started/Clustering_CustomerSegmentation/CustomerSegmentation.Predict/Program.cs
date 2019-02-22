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
            var assetsPath = @"./assets";
            var pivotCsv = Path.Combine(GetDataSetAbsolutePath(assetsPath), "inputs", "pivot.csv");
            var modelZipFilePath = Path.Combine(GetDataSetAbsolutePath(assetsPath), "inputs", "retailClustering.zip");
            var plotSvg = Path.Combine(GetDataSetAbsolutePath(assetsPath), "outputs", "customerSegmentation.svg");
            var plotCsv = Path.Combine(GetDataSetAbsolutePath(assetsPath), "outputs", "customerSegmentation.csv");

            try
            {
                MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic results

                //Create the clusters: Create data files and plot a chart
                var clusteringModelScorer = new ClusteringModelScorer(mlContext, pivotCsv, plotSvg, plotCsv);
                clusteringModelScorer.LoadModelFromZipFile(modelZipFilePath);

                clusteringModelScorer.CreateCustomerClusters();
            } catch (Exception ex)
            {
                Common.ConsoleHelper.ConsoleWriteException(ex.Message);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }

        public static string GetDataSetAbsolutePath(string relativeDatasetPath)
        {
            string projectFolderPath = Common.ConsoleHelper.FindProjectFolderPath();

            string fullPath = Path.Combine(projectFolderPath + "/" + relativeDatasetPath);

            return fullPath;
        }
    }
}
