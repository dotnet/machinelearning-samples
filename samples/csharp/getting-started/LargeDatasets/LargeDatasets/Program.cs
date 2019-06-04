using Common;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using LargeDatasets.DataStructures;
using static Microsoft.ML.DataOperationsCatalog;

namespace LargeDatasets
{
    class Program
    {
        static string originalDataDirectoryRelativePath = @"../../../Data/OriginalUrlData";
        static string originalDataReltivePath = @"../../../Data/OriginalUrlData/url_svmlight";
        static string preparedDataReltivePath = @"../../../Data/PreparedUrlData/url_svmlight";

        static string originalDataDirectoryPath = GetAbsolutePath(originalDataDirectoryRelativePath);
        static string originalDataPath = GetAbsolutePath(originalDataReltivePath);
        static string preparedDataPath = GetAbsolutePath(preparedDataReltivePath);
        static void Main(string[] args)
        {
            //STEP 1: Download dataset
            DownloadDataset(originalDataDirectoryPath);

            //Step 2: Prepare data by adding second column with value total number of features.
            PrepareDataset(originalDataPath, preparedDataPath);
            
            MLContext mlContext = new MLContext();

            //STEP 3: Common data loading configuration  
            var fullDataView = mlContext.Data.LoadFromTextFile<UrlData>(path: Path.Combine(preparedDataPath, "*"),
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
            ConsoleHelper.PrintBinaryClassificationMetrics(mlModel.ToString(),metrics);

            // Try a single prediction
            Console.WriteLine("====Predicting sample data=====");
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
                Console.WriteLine("====Downloading and extracting data====");
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
                Console.WriteLine("====Downloading and extracting is completed====");
            }
        }

        private static void PrepareDataset(string originalDataPath,string preparedDataPath)
        {
            //Create folder for prepared Data path if it does not exist.
            if (!Directory.Exists(preparedDataPath))
            {
                Directory.CreateDirectory(preparedDataPath);
            }
                Console.WriteLine("====Preparing Data====");
                Console.WriteLine("");
                //ML.Net API checks for number of features column before the sparse matrix format
                //So add total number of features i.e 3231961 as second column by taking all the files from originalDataPath
                //and save those files in preparedDataPath.
                if (Directory.GetFiles(preparedDataPath).Length == 0)
                {
                    var ext = new List<string> { ".svm" };
                    var filesInDirectory = Directory.GetFiles(originalDataPath, "*.*", SearchOption.AllDirectories)
                                                .Where(s => ext.Contains(Path.GetExtension(s)));
                    foreach (var file in filesInDirectory)
                    {
                        AddFeaturesColumn(Path.GetFullPath(file), preparedDataPath);
                    }
                }
                Console.WriteLine("====Data Preparation is done====");
                Console.WriteLine("");
                Console.WriteLine("original data path= {0}", originalDataPath);
                Console.WriteLine("");
                Console.WriteLine("prepared data path= {0}", preparedDataPath);
                Console.WriteLine("");
        }
        
        private static void AddFeaturesColumn(string sourceFilePath,string preparedDataPath)
        {
            string sourceFileName = Path.GetFileName(sourceFilePath);
            string preparedFilePath = Path.Combine(preparedDataPath, sourceFileName);

            //if the file does not exist in preparedFilePath then copy from sourceFilePath and then add new column
            if (!File.Exists(preparedFilePath))
            {
                File.Copy(sourceFilePath, preparedFilePath, true);
            }
            string newColumnData =  "3231961";            
            string[] CSVDump = File.ReadAllLines(preparedFilePath);            
            List<List<string>> CSV = CSVDump.Select(x => x.Split(' ').ToList()).ToList();
            for (int i = 0; i < CSV.Count; i++)
            {
                CSV[i].Insert(1, newColumnData);
            }
           
            File.WriteAllLines(preparedFilePath, CSV.Select(x => string.Join('\t', x)));
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
    }
}
