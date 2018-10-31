using System;
using CustomerSegmentation.Model;
using System.IO;
using System.Threading.Tasks;
using CustomerSegmentation.RetailData;
using static CustomerSegmentation.Model.ConsoleHelpers;
using Microsoft.ML.Runtime.Data;

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

            try
            {
                //DataHelpers.PreProcessAndSave(offersCsv, transactionsCsv, pivotCsv);
                //var modelBuilder = new ModelBuilder(pivotCsv, modelZip, kValuesSvg);
                //modelBuilder.BuildAndTrain();

                DataHelpers.PreProcessAndSave(offersCsv, transactionsCsv, pivotCsv);

                //Create the MLContext to share across components for deterministic results
                LocalEnvironment mlContext = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment

                // STEP 1: Create and train a model
                var modelBuilder = new ModelBuilder(mlContext);
                var model = modelBuilder.BuildAndTrain(pivotCsv);

                // STEP2: Evaluate accuracy of the model
                modelBuilder.Evaluate(pivotCsv, model);

                // STEP3: Save model
                modelBuilder.SaveModel(modelZip, model);

                Console.WriteLine("Press any key to exit..");
                Console.ReadLine();

            } catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
