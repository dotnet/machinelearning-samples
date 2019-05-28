using Common;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using UrlClassification.DataStructures;
using static Microsoft.ML.DataOperationsCatalog;
using static Microsoft.ML.TrainCatalogBase;

namespace UrlClassification
{
    class Program
    {
        static string originalDataDirectoryRelativePath = @"../../../Data/OriginalUrlData";
        static string originalDataReltivePath = @"../../../Data/OriginalUrlData/url_svmlight";
        static string transformedDataReltivePath = @"../../../Data/TransformedUrlData";

        static string originalDataDirectoryPath = GetAbsolutePath(originalDataDirectoryRelativePath);
        static string originalDataPath = GetAbsolutePath(originalDataReltivePath);
        static string transformedDataPath = GetAbsolutePath(transformedDataReltivePath);

        //to be removed
        static string fixedDataPath = GetAbsolutePath(@"../../../../Data/train/fixed.svm");
        static void Main(string[] args)
        {
            //STEP 1: Download dataset
            DownloadDataset(originalDataDirectoryPath);

            //Step 2:Prepare/Transofrm data
            PrepareDataset(originalDataPath, transformedDataPath);

            //STEP 3: Create MLContext to be shared across the model creation workflow objects 
            MLContext mlContext = new MLContext();

            var fullDataView = mlContext.Data.LoadFromTextFile<UrlData>(path: Path.Combine(transformedDataPath, "*"),
                                                      hasHeader: false,
                                                      allowSparse: true);

            //Step 4: Divide the whole dataset into 80% training and 20% testing data.
            TrainTestData trainTestData = mlContext.Data.TrainTestSplit(fullDataView, testFraction: 0.2, seed: 1);
            IDataView trainDataView = trainTestData.TrainSet;
            IDataView testDataView = trainTestData.TestSet;
            
            //Step 5: Map label value from string to bool
            var UrlLabelMap = new Dictionary<string, bool>();
            UrlLabelMap["+1"] = true; //Malicious url
            UrlLabelMap["-1"] = false; //Benign 
            var dataProcessingPipeLine = mlContext.Transforms.Conversion.MapValue("LabelKey", UrlLabelMap, "LabelColumn");
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainDataView, dataProcessingPipeLine, 2);   
           
            //Step 6: Append trainer to pipeline
            var trainingPipeLine = dataProcessingPipeLine.Append(
                mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(labelColumnName: "LabelKey", featureColumnName: "FeatureVector"));                     

            //Step 7: Train the model
            Console.WriteLine("====Training the model=====");            
            var mlModel = trainingPipeLine.Fit(trainDataView);
            Console.WriteLine("====Completed Training the model=====");
            Console.WriteLine("");

            //Step 8: Evaluate the model
            Console.WriteLine("====Evaluating the model=====");
            var predictions = mlModel.Transform(testDataView);
            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "LabelKey", scoreColumnName: "Score");
            Console.WriteLine("");
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*       Metrics for {mlModel.ToString()} binary classification model      ");
            PrintMetrics(metrics);

            // Try a single prediction
            var predEngine = mlContext.Model.CreatePredictionEngine<UrlData, UrlPrediction>(mlModel);
            // Create sample data to do a single prediction with it 
            var sampleDatas = CreateSingleDataSample(mlContext, trainDataView);
            foreach (var sampleData in sampleDatas)
            {
                UrlPrediction predictionResult = predEngine.Predict(sampleData);
                Console.WriteLine($"Single Prediction --> Actual value: {sampleData.LabelColumn} | Predicted value: {predictionResult.Prediction}");
            }
            Console.WriteLine("====End of Process..Press any key to exit====");
            Console.ReadLine();
        }

        public static void DownloadDataset(string originalDataDirectoryPath)
        {
            if (!Directory.Exists(originalDataDirectoryPath))
            {
                Console.WriteLine("Downloading and extracting data.........");
                using (var client = new WebClient())
                {
                    //The code below will download a dataset from a third-party, UCI (link), and may be governed by separate third-party terms. 
                    //By proceeding, you agree to those separate terms.
                    client.DownloadFile("https://archive.ics.uci.edu/ml/machine-learning-databases/url/url_svmlight.tar.gz", "url_svmlight.zip");
                }

                Stream inputStream = File.OpenRead("url_svmlight.zip");
                Stream gzipStream = new GZipInputStream(inputStream);
                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(originalDataDirectoryPath);

                tarArchive.Close();
                gzipStream.Close();
                inputStream.Close();
            }           
        }

        public static void PrepareDataset(string originalDataPath,string transformedDataPath)
        {
            //Create folder for transformed Data path if it does not exist.
            if (!Directory.Exists(transformedDataPath))
            {
                Directory.CreateDirectory(transformedDataPath);
            }
                Console.WriteLine("Preparing Data for training and evaluation...........");
                Console.WriteLine("");
                //ML.Net API checks for number of features column before the sparse matrix format
                //So add total number of features i.e 3231961 as second column by taking all the files from originalDataPath
                //and save those files in transformedDataPath.
                if (Directory.GetFiles(transformedDataPath).Length == 0)
                {
                    var ext = new List<string> { ".svm" };
                    var filesInDirectory = Directory.GetFiles(originalDataPath, "*.*", SearchOption.AllDirectories)
                                                .Where(s => ext.Contains(Path.GetExtension(s)));
                    foreach (var file in filesInDirectory)
                    {
                        AddFeaturesColumn(Path.GetFullPath(file), transformedDataPath);
                    }
                }
                Console.WriteLine("Data Preparation is done...........");
                Console.WriteLine("");
                Console.WriteLine("original data path= {0}", originalDataPath);
                Console.WriteLine("");
                Console.WriteLine("Transformed data path= {0}", transformedDataPath);
                Console.WriteLine("");
        }
        
        public static void AddFeaturesColumn(string sourceFilePath,string transformedDataPath)
        {
            string trasnformedFileName = Path.GetFileName(sourceFilePath);
            string trasnformedFilePath = Path.Combine(transformedDataPath, trasnformedFileName);
            
            if (!File.Exists(trasnformedFilePath))
            {
                File.Copy(sourceFilePath, trasnformedFilePath, true);
            }
            string newColumnData =  "3231961";            
            string[] CSVDump = File.ReadAllLines(trasnformedFilePath);            
            List<List<string>> CSV = CSVDump.Select(x => x.Split(' ').ToList()).ToList();
            for (int i = 0; i < CSV.Count; i++)
            {
                CSV[i].Insert(1, newColumnData);
            }
           
            File.WriteAllLines(trasnformedFilePath, CSV.Select(x => string.Join('\t', x)));
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
        private static List<UrlData> CreateSingleDataSample(MLContext mlContext, IDataView dataView)
        {
            // Here (ModelInput object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            List<UrlData> sampleForPredictions = mlContext.Data.CreateEnumerable<UrlData>(dataView, false).Take(4).ToList();                                                                        ;
            return sampleForPredictions;
        }

        public static void PrintMetrics(CalibratedBinaryClassificationMetrics metrics)
        {
            Console.WriteLine("");
            Console.WriteLine($"************************************************************");
            // Console.WriteLine($"*       Metrics for {mlModel.ToString()} binary classification model      ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"*       Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"*       Area Under Roc Curve:      {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"*       Area Under PrecisionRecall Curve:  {metrics.AreaUnderPrecisionRecallCurve:P2}");
            Console.WriteLine($"*       F1Score:  {metrics.F1Score:P2}");
            Console.WriteLine($"*       LogLoss:  {metrics.LogLoss:#.##}");
            Console.WriteLine($"*       LogLossReduction:  {metrics.LogLossReduction:#.##}");
            Console.WriteLine($"*       PositivePrecision:  {metrics.PositivePrecision:#.##}");
            Console.WriteLine($"*       PositiveRecall:  {metrics.PositiveRecall:#.##}");
            Console.WriteLine($"*       NegativePrecision:  {metrics.NegativePrecision:#.##}");
            Console.WriteLine($"*       NegativeRecall:  {metrics.NegativeRecall:P2}");
            Console.WriteLine($"************************************************************");
            Console.WriteLine("");
        }

        //public static void PrintMetrics(IReadOnlyList<CrossValidationResult<CalibratedBinaryClassificationMetrics>> results)
        //{
        //    var me = results;
        //    var metrics= me.Select(x => x.Metrics);
        //    Console.WriteLine("");
        //    Console.WriteLine($"************************************************************");
        //    // Console.WriteLine($"*       Metrics for {mlModel.ToString()} binary classification model      ");
        //    Console.WriteLine($"*-----------------------------------------------------------");
        //    Console.WriteLine($"*       Accuracy: {metrics.Select(x=>x.Accuracy):P2}");
        //    Console.WriteLine($"*       Area Under Roc Curve:      {metrics.Select(x => x.AreaUnderRocCurve):P2}");
        //    Console.WriteLine($"*       Area Under PrecisionRecall Curve:  {metrics.Select(x => x.AreaUnderPrecisionRecallCurve):P2}");
        //    Console.WriteLine($"*       F1Score:  {metrics.Select(x => x.F1Score):P2}");
        //    Console.WriteLine($"*       LogLoss:  {metrics.Select(x => x.LogLoss):#.##}");
        //    Console.WriteLine($"*       LogLossReduction:  {metrics.Select(x => x.LogLossReduction):#.##}");
        //    Console.WriteLine($"*       PositivePrecision:  {metrics.Select(x => x.PositivePrecision):#.##}");
        //    Console.WriteLine($"*       PositiveRecall:  {metrics.Select(x => x.PositiveRecall):#.##}");
        //    Console.WriteLine($"*       NegativePrecision:  {metrics.Select(x => x.NegativePrecision):#.##}");
        //    Console.WriteLine($"*       NegativeRecall:  {metrics.Select(x => x.NegativeRecall):P2}");
        //    Console.WriteLine($"************************************************************");
        //    Console.WriteLine("");
        //}
    }
}
