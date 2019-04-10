using System;
using System.IO;
using Microsoft.ML;
using Common;
using Clustering_Iris.DataStructures;
using Microsoft.ML.Data;

namespace Clustering_Iris
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsRelativePath = @"../../../../Data";
        private static string DataSetRealtivePath = $"{BaseDatasetsRelativePath}/iris-full.txt";

        private static string DataPath = GetAbsolutePath(DataSetRealtivePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/IrisModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);
        private static IDataView trainingDataView;
        private static IDataView testingDataView;

        private static void Main(string[] args)
        {
            //Create the MLContext to share across components for deterministic results
            MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

            // STEP 1: Common data loading configuration            
            IDataView fullData = mlContext.Data.LoadFromTextFile(path: DataPath,
                                                columns:new[]
                                                            {
                                                                new TextLoader.Column("Label", DataKind.Single, 0),
                                                                new TextLoader.Column(nameof(IrisData.SepalLength), DataKind.Single, 1),
                                                                new TextLoader.Column(nameof(IrisData.SepalWidth), DataKind.Single, 2),
                                                                new TextLoader.Column(nameof(IrisData.PetalLength), DataKind.Single, 3),
                                                                new TextLoader.Column(nameof(IrisData.PetalWidth), DataKind.Single, 4),
                                                            },
                                                hasHeader:true,
                                                separatorChar:'\t');

            //Split dataset in two parts: TrainingDataset (80%) and TestDataset (20%)
            DataOperationsCatalog.TrainTestData trainTestData = mlContext.Data.TrainTestSplit(fullData, testFraction: 0.2);
            trainingDataView = trainTestData.TrainSet;
            testingDataView = trainTestData.TestSet;

            //STEP 2: Process data transformations in pipeline
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", nameof(IrisData.SepalLength), nameof(IrisData.SepalWidth), nameof(IrisData.PetalLength), nameof(IrisData.PetalWidth));

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);

            // STEP 3: Create and train the model     
            var trainer = mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 3);
            var trainingPipeline = dataProcessPipeline.Append(trainer);
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP4: Evaluate accuracy of the model
            IDataView predictions = trainedModel.Transform(testingDataView);
            var metrics = mlContext.Clustering.Evaluate(predictions, scoreColumnName: "Score", featureColumnName: "Features");

            ConsoleHelper.PrintClusteringMetrics(trainer.ToString(), metrics);

            // STEP5: Save/persist the model as a .ZIP file
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);

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
            
            ITransformer model = mlContext.Model.Load(ModelPath, out var modelInputSchema);
            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<IrisData, IrisPrediction>(model);

            //Score
            var resultprediction = predEngine.Predict(sampleIrisData);

            Console.WriteLine($"Cluster assigned for setosa flowers:" + resultprediction.SelectedClusterId);
          
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();           
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
