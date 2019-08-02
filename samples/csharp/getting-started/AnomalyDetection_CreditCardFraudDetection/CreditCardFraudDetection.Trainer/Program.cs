using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

using Common;

using CreditCardFraudDetection.Common.DataModels;

using static Microsoft.ML.DataOperationsCatalog;


namespace CreditCardFraudDetection.Trainer
{
    class Program
    {
        static void Main(string[] args)
        {
            // File paths
            string AssetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(AssetsRelativePath);
            string zipDataSet = Path.Combine(assetsPath, "input", "creditcardfraud-dataset.zip");
            string fullDataSetFilePath = Path.Combine(assetsPath, "input", "creditcard.csv");
            string trainDataSetFilePath = Path.Combine(assetsPath, "output", "trainData.csv");
            string testDataSetFilePath = Path.Combine(assetsPath, "output", "testData.csv");
            string modelFilePath = Path.Combine(assetsPath, "output", "randomizedPca.zip");

            // Unzip the original dataset as it is too large for GitHub repo if not zipped
            UnZipDataSet(zipDataSet, fullDataSetFilePath);

            // Create a common ML.NET context.
            // Seed set to any number so you have a deterministic environment for repeateable results
            MLContext mlContext = new MLContext(seed: 1);

            // Prepare data and create Train/Test split datasets
            PrepDatasets(mlContext, fullDataSetFilePath, trainDataSetFilePath, testDataSetFilePath);

            // Load Datasets
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<TransactionObservation>(trainDataSetFilePath, separatorChar: ',', hasHeader: true);
            IDataView testDataView = mlContext.Data.LoadFromTextFile<TransactionObservation>(testDataSetFilePath, separatorChar: ',', hasHeader: true);

            // Train Model
            ITransformer model = TrainModel(mlContext, trainingDataView);

            // Evaluate quality of Model
            EvaluateModel(mlContext, model, testDataView);

            // Save model
            SaveModel(mlContext, model, modelFilePath, trainingDataView.Schema);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }


        public static void PrepDatasets(MLContext mlContext, string fullDataSetFilePath, string trainDataSetFilePath, string testDataSetFilePath)
        {
            // Only prep-datasets if train and test datasets don't exist yet
            if (!File.Exists(trainDataSetFilePath) &&
                !File.Exists(testDataSetFilePath))
            {
                Console.WriteLine("===== Preparing train/test datasets =====");

                // Load the original single dataset
                IDataView originalFullData = mlContext.Data.LoadFromTextFile<TransactionObservation>(fullDataSetFilePath, separatorChar: ',', hasHeader: true);

                // Split the data 80:20 into train and test sets, train and evaluate.
                TrainTestData trainTestData = mlContext.Data.TrainTestSplit(originalFullData, testFraction: 0.2, seed: 1);

                // 80% of original dataset
                IDataView trainData = trainTestData.TrainSet;

                // 20% of original dataset
                IDataView testData = trainTestData.TestSet;

                // Inspect TestDataView to make sure there are true and false observations in test dataset, after spliting 
                InspectData(mlContext, testData, 4);

                // Save train split
                using (var fileStream = File.Create(trainDataSetFilePath))
                {
                    mlContext.Data.SaveAsText(trainData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                }

                // Save test split 
                using (var fileStream = File.Create(testDataSetFilePath))
                {
                    mlContext.Data.SaveAsText(testData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                }
            }
        }


        public static ITransformer TrainModel(MLContext mlContext, IDataView trainDataView)
        {

            // Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] featureColumnNames = trainDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get all the column names
                .Where(name => name != nameof(TransactionObservation.Label)) // Do not include the Label column
                .Where(name => name != "IdPreservationColumn")               // Do not include the IdPreservationColumn/StratificationColumn
                .Where(name => name != nameof(TransactionObservation.Time))  // Do not include the Time column. Not needed as feature column
             .ToArray();


            // Create the data process pipeline
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                                                               .Append(mlContext.Transforms.DropColumns(new string[] { nameof(TransactionObservation.Time) }))
                                                                               .Append(mlContext.Transforms.NormalizeLpNorm(outputColumnName: "NormalizedFeatures", inputColumnName: "Features"));

            // In Anomaly Detection, the learner assumes all training examples have label 0, as it only learns from normal examples.
            // If any of the training examples has label 1, it is recommended to use a Filter transform to filter them out before training:
            IDataView normalTrainDataView = mlContext.Data.FilterRowsByColumn(trainDataView, columnName: nameof(TransactionObservation.Label), lowerBound: 0, upperBound: 1);


            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, normalTrainDataView, dataProcessPipeline, 2);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "NormalizedFeatures", normalTrainDataView, dataProcessPipeline, 2);


            var options = new RandomizedPcaTrainer.Options
            {
                FeatureColumnName = "NormalizedFeatures",   // The name of the feature column. The column data must be a known-sized vector of Single.
                ExampleWeightColumnName = null,				// The name of the example weight column (optional). To use the weight column, the column data must be of type Single.
                Rank = 28,									// The number of components in the PCA.
                Oversampling = 20,							// Oversampling parameter for randomized PCA training.
                EnsureZeroMean = true,						// If enabled, data is centered to be zero mean.
                Seed = 1									// The seed for random number generation.
            };


			// Create an anomaly detector. Its underlying algorithm is randomized PCA.
			IEstimator<ITransformer> trainer = mlContext.AnomalyDetection.Trainers.RandomizedPca(options: options);

			EstimatorChain<ITransformer> trainingPipeline = dataProcessPipeline.Append(trainer);

            ConsoleHelper.ConsoleWriteHeader("=============== Training model ===============");

			TransformerChain<ITransformer> model = trainingPipeline.Fit(normalTrainDataView);

            ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");

            return model;
        }


        private static void EvaluateModel(MLContext mlContext, ITransformer model, IDataView testDataView)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");

            var predictions = model.Transform(testDataView);

            AnomalyDetectionMetrics metrics = mlContext.AnomalyDetection.Evaluate(predictions);

            ConsoleHelper.PrintAnomalyDetectionMetrics("RandomizedPca", metrics);
        }


        public static void InspectData(MLContext mlContext, IDataView data, int records)
        {
            // We want to make sure we have both True and False observations
            Console.WriteLine("Show 4 fraud transactions (true)");
            ShowObservationsFilteredByLabel(mlContext, data, label: true, count: records);

            Console.WriteLine("Show 4 NOT-fraud transactions (false)");
            ShowObservationsFilteredByLabel(mlContext, data, label: false, count: records);
        }


        public static void ShowObservationsFilteredByLabel(MLContext mlContext, IDataView dataView, bool label = true, int count = 2)
        {
            // Convert to an enumerable of user-defined type. 
            var data = mlContext.Data.CreateEnumerable<TransactionObservation>(dataView, reuseRowObject: false)
                                            .Where(x => Math.Abs(x.Label - (label ? 1 : 0)) < float.Epsilon)
                                            // Take a couple values as an array.
                                            .Take(count)
                                            .ToList();

            // Print to console
            data.ForEach(row => { row.PrintToConsole(); });
        }


        #region file handeling

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);

            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }



        public static void UnZipDataSet(string zipDataSet, string destinationFile)
        {
            if (!File.Exists(destinationFile))
            {
                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                ZipFile.ExtractToDirectory(zipDataSet, $"{destinationDirectory}");
            }
        }


        private static void SaveModel(MLContext mlContext, ITransformer model, string modelFilePath, DataViewSchema trainingDataSchema)
        {
            mlContext.Model.Save(model, trainingDataSchema, modelFilePath);

            Console.WriteLine("Saved model to " + modelFilePath);
        }

        #endregion
    }
}
