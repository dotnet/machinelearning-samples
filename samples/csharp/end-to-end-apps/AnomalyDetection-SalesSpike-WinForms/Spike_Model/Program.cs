using System;
using Microsoft.ML;
using System.IO;
using Microsoft.Data.DataView;

namespace ShampooSalesSpikeDetection
{
    internal static class Program
   {
        private static string BaseDatasetsRelativePath = @"../../../Data";
        private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/shampoo-sales.csv";

        private static string DatasetPath = GetAbsolutePath(DatasetRelativePath);

        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/ShampooSalesModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main()
        {
            // Create MLContext to be shared across the model creation workflow objects 
            MLContext mlcontext = new MLContext();

            // Assign the Number of records in dataset file to cosntant variable
            const int size = 36;

            // STEP 1: Common data loading configuration
            IDataView dataView = mlcontext.Data.LoadFromTextFile<ShampooSalesData>(path: DatasetPath, hasHeader: true, separatorChar: ',');

            // Detect temporay changes (spikes) in the pattern
            ITransformer trainedModel = DetectSpike(mlcontext,size,dataView);

            // Detect persistent change in the pattern
            // DetectChangepoint(mlcontext, size, dataView);
            SaveModel(mlcontext, trainedModel);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");

            Console.ReadLine();
        }

        private static ITransformer DetectSpike(MLContext mlcontext, int size, IDataView dataView)
        {
            Console.WriteLine("Detect anomalies");

            // STEP 2: Set the training algorithm 
            // Note -- This confidence level and p-value work well for the shampoo-sales dataset;
            // you may need to adjust for different datasets
            var trainingPipeLine = mlcontext.Transforms.IidSpikeEstimator(outputColumnName: nameof(ShampooSalesPrediction.Prediction), inputColumnName: nameof(ShampooSalesData.numSales),confidence: 95, pvalueHistoryLength: size / 4);

            // STEP 3: Train the model by fitting the dataview
            Console.WriteLine("=============== Training the model using Spike Detection algorithm ===============");
            ITransformer trainedModel = trainingPipeLine.Fit(dataView);
            Console.WriteLine("=============== End of training process ===============");

            // Step 4: Use/test model
            // Apply data transformation to create predictions.
            Console.WriteLine("=============== Using the model to detect anomalies ===============");
            IDataView transformedData = trainedModel.Transform(dataView);
            var predictions = mlcontext.Data.CreateEnumerable<ShampooSalesPrediction>(transformedData, reuseRowObject: false);
            Console.WriteLine("Alert\tScore\tP-Value");
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2]);
                Console.ResetColor();
            }
            Console.WriteLine("");

            return trainedModel;
        }

        private static void SaveModel(MLContext mlcontext, ITransformer trainedModel)
        {
            Console.WriteLine("=============== Saving model ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlcontext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);
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