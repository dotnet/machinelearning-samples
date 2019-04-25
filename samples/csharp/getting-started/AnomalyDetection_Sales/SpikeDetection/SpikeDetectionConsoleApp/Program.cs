﻿using System;
using Microsoft.ML;
using System.IO;
using SpikeDetection.DataStructures;

namespace SpikeDetection
{
    internal static class Program
   {
        private static string BaseDatasetsRelativePath = @"../../../../Data";
        private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/product-sales.csv";

        private static string DatasetPath = GetAbsolutePath(DatasetRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/ProductSalesModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main()
        {
            // Create MLContext to be shared across the model creation workflow objects 
            MLContext mlcontext = new MLContext();

            //assign the Number of records in dataset file to cosntant variable
            const int size = 36;

            //STEP 1: Common data loading configuration
            IDataView dataView = mlcontext.Data.LoadFromTextFile<ProductSalesData>(path: DatasetPath, hasHeader: true, separatorChar: ',');

            //To detech temporay changes in the pattern
            DetectSpike(mlcontext,size,dataView);

            //To detect persistent change in the pattern
            DetectChangepoint(mlcontext, size, dataView);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");

            Console.ReadLine();
        }

        static void DetectSpike(MLContext mlcontext,int size,IDataView dataView)
        {
           Console.WriteLine("Detect temporary changes in pattern");

            //STEP 2: Set the training algorithm    
            var trainingPipeLine = mlcontext.Transforms.DetectIidSpike(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.numSales),confidence: 95, pvalueHistoryLength: size / 4);

            //STEP 3:Train the model by fitting the dataview
            Console.WriteLine("=============== Training the model using Spike Detection algorithm ===============");
            ITransformer trainedModel = trainingPipeLine.Fit(dataView);
            Console.WriteLine("=============== End of training process ===============");

            //Apply data transformation to create predictions.
            IDataView transformedData = trainedModel.Transform(dataView);
            var predictions = mlcontext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);
                      
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
        }

        static void DetectChangepoint(MLContext mlcontext, int size, IDataView dataView)
        {
          Console.WriteLine("Detect Persistent changes in pattern");

          //STEP 2: Set the training algorithm    
          var trainingPipeLine = mlcontext.Transforms.DetectIidChangePoint(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.numSales), confidence: 95, changeHistoryLength: size / 4);

          //STEP 3:Train the model by fitting the dataview
          Console.WriteLine("=============== Training the model Using Change Point Detection Algorithm===============");
          ITransformer trainedModel = trainingPipeLine.Fit(dataView);
          Console.WriteLine("=============== End of training process ===============");

          //Apply data transformation to create predictions.
          IDataView transformedData = trainedModel.Transform(dataView);
          var predictions = mlcontext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);
                       
          Console.WriteLine($"{nameof(ProductSalesPrediction.Prediction)} column obtained post-transformation.");
          Console.WriteLine("Alert\tScore\tP-Value\tMartingale value");
            
          foreach(var p in predictions)
          {
             if (p.Prediction[0] == 1)
             {
                 Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}  <-- alert is on, predicted changepoint", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
             }
             else
             { 
                 Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}",  p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);                  
             }            
          }
            Console.WriteLine("");
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