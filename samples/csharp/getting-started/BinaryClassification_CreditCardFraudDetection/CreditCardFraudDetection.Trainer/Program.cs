using Microsoft.ML;
using System.Linq;
using System.IO;
using System;
using Common;
using CreditCardFraudDetection.Common.DataModels;
using System.IO.Compression;
using Microsoft.ML.Trainers;
using static Microsoft.ML.DataOperationsCatalog;

namespace CreditCardFraudDetection.Trainer
{
    class Program
    {
        static void Main(string[] args)
        {
            //File paths
            string AssetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(AssetsRelativePath);
            string zipDataSet = Path.Combine(assetsPath, "input", "creditcardfraud-dataset.zip");
            string fullDataSetFilePath = Path.Combine(assetsPath, "input", "creditcard.csv");
            string trainDataSetFilePath = Path.Combine(assetsPath, "output", "trainData.csv"); 
            string testDataSetFilePath = Path.Combine(assetsPath, "output", "testData.csv");
            string modelFilePath = Path.Combine(assetsPath, "output", "fastTree.zip");

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
            (ITransformer model, string trainerName) = TrainModel(mlContext, trainingDataView);

            // Evaluate quality of Model
            EvaluateModel(mlContext, model, testDataView, trainerName);

            // Save model
            SaveModel(mlContext, model, modelFilePath, trainingDataView.Schema);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }

        public static void PrepDatasets(MLContext mlContext, string fullDataSetFilePath, string trainDataSetFilePath, string testDataSetFilePath)
        {
            //Only prep-datasets if train and test datasets don't exist yet

            if (!File.Exists(trainDataSetFilePath) &&
                !File.Exists(testDataSetFilePath))
            {
                Console.WriteLine("===== Preparing train/test datasets =====");

                //Load the original single dataset
                IDataView originalFullData = mlContext.Data.LoadFromTextFile<TransactionObservation>(fullDataSetFilePath, separatorChar: ',', hasHeader: true);
                             
                // Split the data 80:20 into train and test sets, train and evaluate.
                TrainTestData trainTestData = mlContext.Data.TrainTestSplit(originalFullData, testFraction: 0.2, seed: 1);
                IDataView trainData = trainTestData.TrainSet;
                IDataView testData = trainTestData.TestSet;

                //Inspect TestDataView to make sure there are true and false observations in test dataset, after spliting 
                InspectData(mlContext, testData, 4);

                // save train split
                using (var fileStream = File.Create(trainDataSetFilePath))
                {
                    mlContext.Data.SaveAsText(trainData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                }

                // save test split 
                using (var fileStream = File.Create(testDataSetFilePath))
                {
                    mlContext.Data.SaveAsText(testData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                }
            }
        }

        public static (ITransformer model, string trainerName) TrainModel(MLContext mlContext, IDataView trainDataView)
        {
            //Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] featureColumnNames = trainDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != nameof(TransactionObservation.Label)) // Do not include the Label column
                .Where(name => name != "IdPreservationColumn")               // Do not include the IdPreservationColumn/StratificationColumn
                .Where(name => name != "Time")                               // Do not include the Time column. Not needed as feature column
                .ToArray();

            // Create the data process pipeline
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { "Time" }))
                                            .Append(mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
                                                                                 outputColumnName: "FeaturesNormalizedByMeanVar"));

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainDataView, dataProcessPipeline, 2);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainDataView, dataProcessPipeline, 1);
          
            // Set the training algorithm
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(TransactionObservation.Label),
                                                                                                featureColumnName: "FeaturesNormalizedByMeanVar",
                                                                                                numberOfLeaves: 20,
                                                                                                numberOfTrees: 100,
                                                                                                minimumExampleCountPerLeaf: 10,
                                                                                                learningRate: 0.2);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            ConsoleHelper.ConsoleWriteHeader("=============== Training model ===============");

            var model = trainingPipeline.Fit(trainDataView);

            ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");

            // Append feature contribution calculator in the pipeline. This will be used
            // at prediction time for explainability. 
            var fccPipeline = model.Append(mlContext.Transforms
                .CalculateFeatureContribution(model.LastTransformer)
                .Fit(dataProcessPipeline.Fit(trainDataView).Transform(trainDataView)));

            return (fccPipeline, fccPipeline.ToString());

        }

        private static void EvaluateModel(MLContext mlContext, ITransformer model, IDataView testDataView, string trainerName)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = model.Transform(testDataView);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, 
                                                                  labelColumnName: nameof(TransactionObservation.Label), 
                                                                  scoreColumnName: "Score");

            ConsoleHelper.PrintBinaryClassificationMetrics(trainerName, metrics);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public static void InspectData(MLContext mlContext, IDataView data, int records)
        {
            //We want to make sure we have True and False observations

            Console.WriteLine("Show 4 fraud transactions (true)");
            ShowObservationsFilteredByLabel(mlContext, data, label: true, count: records);

            Console.WriteLine("Show 4 NOT-fraud transactions (false)");
            ShowObservationsFilteredByLabel(mlContext, data, label: false, count: records);
        }

        public static void ShowObservationsFilteredByLabel(MLContext mlContext, IDataView dataView, bool label = true, int count = 2)
        {
            // Convert to an enumerable of user-defined type. 
            var data = mlContext.Data.CreateEnumerable<TransactionObservation>(dataView, reuseRowObject: false)
                                            .Where(x => x.Label == label)
                                            // Take a couple values as an array.
                                            .Take(count)
                                            .ToList();

            // print to console
            data.ForEach(row => { row.PrintToConsole(); });
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
            mlContext.Model.Save(model,trainingDataSchema, modelFilePath);

            Console.WriteLine("Saved model to " + modelFilePath);
        }
    }
}
