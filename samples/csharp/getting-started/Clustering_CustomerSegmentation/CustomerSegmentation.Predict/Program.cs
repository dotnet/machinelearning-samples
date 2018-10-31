using System;
using CustomerSegmentation.Model;
using System.IO;
using System.Threading.Tasks;
using static CustomerSegmentation.Model.ConsoleHelpers;

namespace CustomerSegmentation
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var assetsPath = ModelHelpers.GetAssetsPath(@"..\..\..\assets");

            var pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv");
            var modelZip = Path.Combine(assetsPath, "inputs", "retailClustering.zip");
            var plotSvg = Path.Combine(assetsPath, "outputs", "customerSegmentation.svg");
            var plotCsv = Path.Combine(assetsPath, "outputs", "customerSegmentation.csv");

            try
            {
                var modelEvaluator = new ModelScorer(pivotCsv, modelZip, plotSvg, plotCsv);
                modelEvaluator.CreateCustomerClusters();
            } catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
