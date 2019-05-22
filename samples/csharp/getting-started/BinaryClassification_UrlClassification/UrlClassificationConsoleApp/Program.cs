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
using static Microsoft.ML.TrainCatalogBase;

namespace UrlClassification
{
    class Program
    {
        static string dataReltivePath = @"../../../../Data/train/url_svmlight/*";
        static string testDataRelativePath = @"../../../../Data/test/Day21.svm";
        static string predictDataRelativePath = @"../../../../Data/test/predict.svm";

        //static string AppPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        //static string DataDirectoryPath = Path.Combine(AppPath, "..", "..", "..", "Data1", "train");
        //static string TrainDataPath = Path.Combine(AppPath, "..", "..", "..", "Data1", "train", "url_svmlight");

        static string dataPath = GetAbsolutePath(dataReltivePath);
        static string testDataPath = GetAbsolutePath(testDataRelativePath);
        static string predictDataPath = GetAbsolutePath(predictDataRelativePath);
        static void Main(string[] args)
        {
           

            //if (!File.Exists(TrainDataPath))
            //{
            //    using (var client = new WebClient())
            //    {
            //        //The code below will download a dataset from a third-party, UCI (link), and may be governed by separate third-party terms. 
            //        //By proceeding, you agree to those separate terms.
            //        client.DownloadFile("https://archive.ics.uci.edu/ml/machine-learning-databases/url/url_svmlight.tar.gz", "url_svmlight.zip");
            //    }

            //    ZipFile.ExtractToDirectory("URLfiles.zip", DataDirectoryPath);
            //}

            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            MLContext mlContext = new MLContext();

            var traindata = mlContext.Data.LoadFromTextFile<UrlData>(path: dataPath,
                                                      hasHeader: false,
                                                      separatorChar: ' ',
                                                      allowSparse: true);

            var testDataView = mlContext.Data.LoadFromTextFile<UrlData>(testDataPath,
                hasHeader: false, separatorChar: ' ', allowSparse: true);

            var predictDataView = mlContext.Data.LoadFromTextFile<UrlData>(predictDataPath,
                hasHeader: false, separatorChar: ' ', allowSparse: true);
            
            //Map label value from string to bool
            var UrlLabelMap = new Dictionary<string, bool>();
            UrlLabelMap["+1"] = true; //Malicious url
            UrlLabelMap["-1"] = false; //Benign 

            var pipeLine = mlContext.Transforms.Conversion.MapValue("LabelKey", UrlLabelMap, "LabelColumn");
           
            var est = pipeLine.Append(mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(labelColumnName: "LabelKey", featureColumnName: "FeatureVector"));
         
            Console.WriteLine("====Training the model=====");            
            var mlModel = est.Fit(traindata);
            var predictions = mlModel.Transform(testDataView);

            Console.WriteLine("====Evaluating the model=====");
            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "LabelKey", scoreColumnName: "Score");
            PrintMetrics(metrics, mlModel);

            // Try a single prediction
            var predEngine = mlContext.Model.CreatePredictionEngine<UrlData, UrlPrediction>(mlModel);
            // Create sample data to do a single prediction with it 
            //var sampleData = CreateSingleDataSample(mlContext, predictDataView);
            var sampleDatas = CreateSingleDataSample(mlContext, predictDataView);
            foreach (var sampleData in sampleDatas)
            {
                UrlPrediction predictionResult = predEngine.Predict(sampleData);

                Console.WriteLine($"Single Prediction --> Actual value: {sampleData.LabelColumn} | Predicted value: {predictionResult.Prediction}");
            }
            Console.WriteLine("====End of Process..Press any key to exit====");
            Console.ReadLine();
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
            List<UrlData> sampleForPredictions = mlContext.Data.CreateEnumerable<UrlData>(dataView, false).ToList();                                                                        ;
            return sampleForPredictions;
        }
        
        public static void PrintMetrics(CalibratedBinaryClassificationMetrics metrics, ITransformer mlModel)
        {
            Console.WriteLine("");
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*       Metrics for {mlModel.ToString()} binary classification model      ");
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
    }
}
