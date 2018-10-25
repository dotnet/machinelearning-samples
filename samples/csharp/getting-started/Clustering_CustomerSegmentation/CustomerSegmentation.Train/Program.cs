using System;
using CustomerSegmentation.Model;
using System.IO;
using System.Threading.Tasks;
using CustomerSegmentation.RetailData;
using static CustomerSegmentation.Model.ConsoleHelpers;

namespace CustomerSegmentation
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var assetsPath = ModelHelpers.GetAssetsPath(@"..\..\..\assets");

            var transactionsCsv = Path.Combine(assetsPath, "inputs", "transactions.csv");
            var offersCsv = Path.Combine(assetsPath, "inputs", "offers.csv");
            var pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv");
            var modelZip = Path.Combine(assetsPath, "outputs", "retailClustering.zip");
            var kValuesSvg = Path.Combine(assetsPath, "outputs", "kValues.svg");

            try
            {
                DataHelpers.PreProcessAndSave(offersCsv, transactionsCsv, pivotCsv);
                var modelBuilder = new ModelBuilder(pivotCsv, modelZip, kValuesSvg);
                modelBuilder.BuildAndTrain();
            } catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
