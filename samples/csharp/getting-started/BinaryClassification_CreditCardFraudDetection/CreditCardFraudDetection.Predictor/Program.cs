using CreditCardFraudDetection.Common;
using System;
using System.IO;

namespace CreditCardFraudDetection.Predictor
{
    class Program
    {
        static void Main(string[] args)
        {
            string assetsPath = GetAbsolutePath(@"../../../assets");
            string trainOutput = GetAbsolutePath(@"../../../../CreditCardFraudDetection.Trainer/assets/output");

            CopyModelAndDatasetFromTrainingProject(trainOutput, assetsPath);

            var inputDatasetForPredictions = Path.Combine(assetsPath,"input", "testData.csv");
            var modelFilePath = Path.Combine(assetsPath, "input", "fastTree.zip");

            // Create model predictor to perform a few predictions
            var modelPredictor = new Predictor(modelFilePath,inputDatasetForPredictions);

            modelPredictor.RunMultiplePredictions(numberOfPredictions:5);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }

        public static void CopyModelAndDatasetFromTrainingProject(string trainOutput, string assetsPath)
        {
            if (!File.Exists(Path.Combine(trainOutput, "testData.csv")) ||
                !File.Exists(Path.Combine(trainOutput, "fastTree.zip")))
            {
                Console.WriteLine("***** YOU NEED TO RUN THE TRAINING PROJECT IN THE FIRST PLACE *****");
                Console.WriteLine("=============== Press any key ===============");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // copy files from train output
            Directory.CreateDirectory(assetsPath);
            foreach (var file in Directory.GetFiles(trainOutput))
            {

                var fileDestination = Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file));
                if (File.Exists(fileDestination))
                {
                    LocalConsoleHelper.DeleteAssets(fileDestination);
                }

                File.Copy(file, Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file)));
            }

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
