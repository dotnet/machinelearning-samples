using CreditCardFraudDetection.Common;
using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;
using System.Linq;
using System.IO;
using Microsoft.ML.Runtime.Data.IO;
using System;

namespace CreditCardFraudDetection.Trainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var assetsPath = ConsoleHelpers.GetAssetsPath(@"..\..\..\assets");
            var zipDataSet = Path.Combine(assetsPath, "input", "creditcardfraud-dataset.zip");
            var dataSetFile = Path.Combine(assetsPath, "input", "creditcard.csv");

            try
            {
                //Unzip datasets as they are significantly large, too large for GitHub if not zipped
                ConsoleHelpers.UnZipDataSet(zipDataSet, dataSetFile);

                // Create a common ML.NET context.
                // Seed set to any number so you have a deterministic environment for repeateable results
                MLContext mlContext = new MLContext(seed:1);

                var modelBuilder = new ModelBuilder(mlContext, assetsPath, dataSetFile);
                modelBuilder.PreProcessData(mlContext);

                ConsoleHelpers.ConsoleWriteHeader("Creating and training the model");
                modelBuilder.TrainFastTreeAndSaveModels();
            }
            catch (Exception e)
            {
                ConsoleHelpers.ConsoleWriteException(new[] { e.Message , e.StackTrace });
            }

            ConsoleHelpers.ConsolePressAnyKey();
        }
    } 
}
