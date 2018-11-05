using System;
using System.IO;

using Microsoft.ML;
using Common;
using Clustering_Iris.DataStructures;

namespace Clustering_Iris
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string DataPath = $"{BaseDatasetsLocation}/iris-full.txt";

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelPath = $"{BaseModelsPath}/IrisModel.zip";

        private static void Main(string[] args)
        {
            //Create the MLContext to share across components for deterministic results
            MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

            //STEP 1: Common data loading
            DataLoader dataLoader = new DataLoader(mlContext);
            var trainingDataView = dataLoader.GetDataView(DataPath);

            //STEP 2: Process data transformations in pipeline
            var dataProcessor = new DataProcessor(mlContext);
            var dataProcessPipeline = dataProcessor.DataProcessPipeline;

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole<IrisData>(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);

            // STEP 3: Create and train the model                
            var modelBuilder = new ModelBuilder<IrisData, IrisPrediction>(mlContext, dataProcessPipeline);
            var trainer = mlContext.Clustering.Trainers.KMeans(features: "Features", clustersCount: 3);
            modelBuilder.AddTrainer(trainer);
            var trainedModel = modelBuilder.Train(trainingDataView);

            // STEP4: Evaluate accuracy of the model
            var metrics = modelBuilder.EvaluateClusteringModel(trainingDataView);
            Common.ConsoleHelper.PrintClusteringMetrics("KMeans", metrics);

            // STEP5: Save/persist the model as a .ZIP file
            modelBuilder.SaveModelAsFile(ModelPath);

            Console.WriteLine("=============== End of training process ===============");

            Console.WriteLine("=============== Predict a cluster for a single case (Single Iris data sample) ===============");

            // Test with one sample text 
            var sampleIrisData = new IrisData()
            {
                SepalLength = 3.3f,
                SepalWidth = 1.6f,
                PetalLength = 0.2f,
                PetalWidth = 5.1f,
            };

            //Create the clusters: Create data files and plot a chart
            var modelScorer = new ModelScorer<IrisData, IrisPrediction>(mlContext);
            modelScorer.LoadModelFromZipFile(ModelPath);

            var prediction = modelScorer.PredictSingle(sampleIrisData);

            Console.WriteLine($"Cluster assigned for setosa flowers:"+prediction.SelectedClusterId);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();           
        }
    }







}
