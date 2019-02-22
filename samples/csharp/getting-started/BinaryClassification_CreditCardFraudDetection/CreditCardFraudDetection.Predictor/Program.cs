using CreditCardFraudDetection.Common;
using System;
using System.IO;

namespace CreditCardFraudDetection.Predictor
{
    class Program
    {
        static void Main(string[] args)
        {
            string assetsPath = GetDataSetAbsolutePath(@"..\..\..\assets");
            string trainOutput = GetDataSetAbsolutePath(@"..\..\..\..\CreditCardFraudDetection.Trainer\assets\output");

            if (!File.Exists(Path.Combine(trainOutput, "testData.csv")) ||
                !File.Exists(Path.Combine(trainOutput, "fastTree.zip"))){
                ConsoleHelpers.ConsoleWriteWarning("YOU SHOULD RUN TRAIN PROJECT FIRST");
                ConsoleHelpers.ConsolePressAnyKey();
                return;
            }

            // copy files from train output
            Directory.CreateDirectory(assetsPath);
            foreach (var file in Directory.GetFiles(trainOutput)) {

                var fileDestination = Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file));
                if (File.Exists(fileDestination)) {
                    ConsoleHelpers.DeleteAssets(fileDestination);
                }

                File.Copy(file, Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file)));
            }

            var dataSetFile = Path.Combine(assetsPath,"input", "testData.csv");
            var modelFile = Path.Combine(assetsPath, "input", "fastTree.zip");


            var modelEvaluator = new Predictor(modelFile,dataSetFile);

            int numberOfTransactions = 5;
            modelEvaluator.RunMultiplePredictions(numberOfTransactions);

            ConsoleHelpers.ConsolePressAnyKey();
        }

        public static string GetDataSetAbsolutePath(string relativeDatasetPath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            Console.WriteLine($"Assembly Folder Path: " + assemblyFolderPath);

            string fullPath = Path.Combine(assemblyFolderPath + "/" + relativeDatasetPath);
            Console.WriteLine("\n" + $"Full Path: " + fullPath + "\n");

            return fullPath;
        }
    }
}
