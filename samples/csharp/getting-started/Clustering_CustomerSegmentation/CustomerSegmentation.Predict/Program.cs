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

            try
            {
                var modelEvaluator = new ModelEvaluator(pivotCsv, modelZip, plotSvg);
                modelEvaluator.Evaluate();
            } catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
