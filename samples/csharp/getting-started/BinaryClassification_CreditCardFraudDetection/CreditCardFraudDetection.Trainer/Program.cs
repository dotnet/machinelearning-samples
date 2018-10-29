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
                ConsoleHelpers.UnZipDataSet(zipDataSet, dataSetFile);

                var modelBuilder = new ModelBuilder(assetsPath, dataSetFile);
                modelBuilder.Build();
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
