using System;
using System.IO;

using CreditCardFraudDetection.Common;

namespace CreditCardFraudDetection.Predictor
{
    class Program
    {
        static void Main(string[] args)
        {
            string assetsPath = GetAbsolutePath(@"../../../assets");
            string trainOutput = GetAbsolutePath(@"../../../../CreditCardFraudDetection.Trainer/assets/output");

            var inputDatasetForPredictions = Path.Combine(assetsPath, "input", "testData.csv");
            var modelFilePath = Path.Combine(assetsPath, "input", "randomizedPca.zip");

            //Always copy the trained model from the trainer project just in case there's a new version trained. 
            CopyModelAndDatasetFromTrainingProject(trainOutput, assetsPath);

            // Create model predictor to perform a few predictions
            var modelPredictor = new Predictor(modelFilePath, inputDatasetForPredictions);

            modelPredictor.RunMultiplePredictions(numberOfPredictions: 5);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }


        public static void CopyModelAndDatasetFromTrainingProject(string trainOutput, string assetsPath)
        {
            if (!File.Exists(Path.Combine(trainOutput, "testData.csv")) ||
                !File.Exists(Path.Combine(trainOutput, "randomizedPca.zip")))
            {
                Console.WriteLine("***** YOU NEED TO RUN THE TRAINING PROJECT FIRST *****");
                Console.WriteLine("=============== Press any key ===============");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Copy files from train output
            Directory.CreateDirectory(assetsPath);

            foreach (var file in Directory.GetFiles(trainOutput))
            {
                var fileDestination = Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file));

                if (File.Exists(fileDestination))
                {
                    LocalConsoleHelper.DeleteAssets(fileDestination);
                }

                //Only copy the files we need for the scoring project
                if ((Path.GetFileName(file) == "testData.csv") || (Path.GetFileName(file) == "randomizedPca.zip"))
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
