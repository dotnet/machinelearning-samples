using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using AnomalyDetection.DataStructures;
using System.Linq;
using static Microsoft.ML.Transforms.NormalizingEstimator;
using System.IO.Compression;
using System.Collections.Generic;

namespace CreditRisk
{
    class Program
    {
        private static string BaseDatasetsRelativePath = @"../../../../Data";
        private static string DataRelativePath = $"{BaseDatasetsRelativePath}/german.data.txt";

        private static string DataPath = GetAbsolutePath(DataRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/GermanCreditCardModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main()
        {
            // 1. Implement the pipeline for creating and training the model    
            MLContext mlContext = new MLContext(seed:0);

            // 2. Specify how training data is going to be loaded into the DataView
            IDataView dataView = mlContext.Data.LoadFromTextFile<CustomerCreditData>(DataPath, separatorChar: ' ');
            //var peak1 = dataView.Preview();
            
            var trainTestData = mlContext.AnomalyDetection.TrainTestSplit(dataView, 0.2);
            var trainData = trainTestData.TrainSet;
            var testData = trainTestData.TestSet;

            //var peak2 = trainTestData.TrainSet.Preview();
            //var peak3 = trainTestData.TestSet.Preview();

            //count the number of anomalies in test data
            var testDataAbnoramlCount =mlContext.Data.CreateEnumerable<CustomerCreditData>(testData, reuseRowObject: false)
                                           .Where(x => x.Label == 2)              
                                           .ToList().Count;

            //var filteredTrainData = mlContext.Data.FilterRowsByColumn(trainTestData.TrainSet, columnName: "Label", lowerBound: 1, upperBound: 2);


            //string[] featureColumnNames = dataView.Schema.AsQueryable().Select(column => column.Name).ToArray();

            // 2. Create a pipeline to prepare your data
            // 2a. There are 2 types of features in the data. 1.Numerical and 2.Categorical.
            //Transform Categorical features into Numeric values
            //Concatenate numeric features and transformed categorical features
            //Drop Label column from the pipeLine because Anomaly Detection trainer trains on unsupervised data.
            IEstimator<ITransformer> dataProcessingPipeline = mlContext.Transforms.Concatenate("NumericFeatures", nameof(CustomerCreditData.NumOfMonths),
                                nameof(CustomerCreditData.CreditAmount), nameof(CustomerCreditData.InstallmentRate), nameof(CustomerCreditData.ResidentSince),
                                nameof(CustomerCreditData.Age), nameof(CustomerCreditData.NumberOfExistingCredits), nameof(CustomerCreditData.NumberOfLiablePeople))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "ExistingCheckingAccountStatusEncoded", inputColumnName: nameof(CustomerCreditData.ExistingCheckingAccountStatus)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CreditHistoryEncoded", inputColumnName: nameof(CustomerCreditData.CreditHistory)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PurposeEncoded", inputColumnName: nameof(CustomerCreditData.Purpose)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "SavingAccountBondsEncoded", inputColumnName: nameof(CustomerCreditData.SavingAccountBonds)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "employedSinceEncoded", inputColumnName: nameof(CustomerCreditData.EmployedSince)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "StatusAndSexEncoded", inputColumnName: nameof(CustomerCreditData.StatusAndSex)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "gurantorsEncoded", inputColumnName: nameof(CustomerCreditData.Gurantors)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "propertyEncoded", inputColumnName: nameof(CustomerCreditData.Property)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "OtherInstallmentPlansEncoded", inputColumnName: nameof(CustomerCreditData.OtherInstallmentPlans)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HousingEncoded", inputColumnName: nameof(CustomerCreditData.Housing)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "JobStatusEncoded", inputColumnName: nameof(CustomerCreditData.JobStatus)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "TelephoneEncoded", inputColumnName: nameof(CustomerCreditData.Telephone)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "IsForeignWorkerEncoded", inputColumnName: nameof(CustomerCreditData.IsForeignWorker)))
                .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features, "NumericFeatures",
                "ExistingCheckingAccountStatusEncoded",
                "CreditHistoryEncoded",
                "PurposeEncoded",
                 "SavingAccountBondsEncoded",
                 "employedSinceEncoded",
                 "StatusAndSexEncoded",
                 "gurantorsEncoded",
                 "propertyEncoded",
                 "OtherInstallmentPlansEncoded",
                 "HousingEncoded",
                 "JobStatusEncoded",
                 "TelephoneEncoded",
                 "IsForeignWorkerEncoded"
                ))
                .Append(mlContext.Transforms.DropColumns(nameof(CustomerCreditData.Label)));
               
            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            //  Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, DefaultColumnNames.Features, dataView, dataProcessPipeline, 1);
            PeekDataViewInConsole(mlContext, trainData, dataProcessingPipeline, 2);

            // Set the training algorithm
            IEstimator<ITransformer> trainer = mlContext.AnomalyDetection.Trainers.RandomizedPca(featureColumnName: DefaultColumnNames.Features);
                

            //Append the trainer to dataprocessingPipeLine
            IEstimator<ITransformer> trainingPipeLine = dataProcessingPipeline.Append(trainer);

            // 3. Get a model by training the pipeline that was built.
            Console.WriteLine("Creating and Training a model for Anomaly Detection using ML.NET");
            ITransformer model = trainingPipeLine.Fit(trainData);

            // 4. Evaluate the model to see how well it performs on different dataset (test data).
            Console.WriteLine("Training of model is complete \nEvaluating the model with test data");

            IDataView transformedData = model.Transform(testData);
            var metrics = mlContext.AnomalyDetection.Evaluate(transformedData);

            // Getting the data of the newly created column as an IEnumerable of IidSpikePrediction.
            var results = mlContext.Data.CreateEnumerable<CustomerCreditDataPrediction>(transformedData, reuseRowObject: false).ToList();

            var outliers = results.Where(x => x.Score < 0.1).ToList();

            Console.WriteLine("Number of Records that are at risk = {0}", outliers.Count);

            
            Console.WriteLine("Displaying the records that are at risk or identified as outliers");
            // The i-th example's feature vector in text format.
            //var featuresInText = string.Join(',', featureColumnNames);
            var TestDataCount = mlContext.Data.CreateEnumerable<CustomerCreditData>(testData, reuseRowObject: false)
                                           
                                           .ToList().Count;
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                
                       
                if (result.Score <0.1)
                {
                    // The i-th sample is predicted as an outlier.
                    //Console.BackgroundColor = ConsoleColor.DarkYellow;
                    //Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("the {0} th record and Predictedlabel : {1}",
                      i,result.PredictedLabel);
                }
            }



            //Console.WriteLine($"{outputColumnName} column obtained post-transformation.");
            //Console.WriteLine("Alert\tScore\tP-Value");
            //foreach (var prediction in predictionColumn)
            //    Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}", prediction.Prediction[0], prediction.Prediction[1], prediction.Prediction[2]);
            //Console.WriteLine("");
            // 6. Save the model to file so it can be used in another app.
            Console.WriteLine("Saving the model");

            using (var fs = new FileStream("sentiment_model.zip", FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                model.SaveTo(mlContext, fs);
                fs.Close();
            }

            Console.ReadLine();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public static void ConsoleWriteHeader(params string[] lines)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" ");
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            var maxLength = lines.Select(x => x.Length).Max();
            Console.WriteLine(new string('#', maxLength));
            Console.ForegroundColor = defaultColor;
        }


        public static void PeekDataViewInConsole(MLContext mlContext, IDataView dataView, IEstimator<ITransformer> pipeline, int numberOfRows = 4)
        {
            string msg = string.Format("Peek data in DataView: Showing {0} rows with the columns", numberOfRows.ToString());
            ConsoleWriteHeader(msg);

            //https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md#how-do-i-look-at-the-intermediate-data
            var transformer = pipeline.Fit(dataView);
            var transformedData = transformer.Transform(dataView);

            // 'transformedData' is a 'promise' of data, lazy-loading. call Preview  
            //and iterate through the returned collection from preview.

            var preViewTransformedData = transformedData.Preview(maxRows: numberOfRows);

            foreach (var row in preViewTransformedData.RowView)
            {
                var ColumnCollection = row.Values;
                string lineToPrint = "Row--> ";
                foreach (KeyValuePair<string, object> column in ColumnCollection)
                {
                    lineToPrint += $"| {column.Key}:{column.Value}";
                }
                Console.WriteLine(lineToPrint + "\n");
            }
        }

        public static void UnZipDataSet(string zipDataSet, string destinationFile)
        {
            if (!File.Exists(destinationFile))
            {
                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                ZipFile.ExtractToDirectory(zipDataSet, $"{destinationDirectory}");
            }
        }
        /// <summary>
        /// Input class that tells ML.NET how to read the dataset.
        /// </summary>

    }
}
