using System;
using System.IO;

using Microsoft.ML;
using Common;
using Clustering_Iris.DataStructures;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;

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

            // STEP 1: Common data loading configuration
            TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                            {
                                                Separator = "\t",
                                                HasHeader = true,
                                                Column = new[]
                                                            {
                                                                new TextLoader.Column("Label", DataKind.R4, 0),
                                                                new TextLoader.Column("SepalLength", DataKind.R4, 1),
                                                                new TextLoader.Column("SepalWidth", DataKind.R4, 2),
                                                                new TextLoader.Column("PetalLength", DataKind.R4, 3),
                                                                new TextLoader.Column("PetalWidth", DataKind.R4, 4),
                                                            }
                                            });

            IDataView fullData = textLoader.Read(DataPath);

            //Split dataset in two parts: TrainingDataset (80%) and TestDataset (20%)
            (IDataView trainingDataView, IDataView testingDataView) = mlContext.Clustering.TrainTestSplit(fullData, testFraction: 0.2);

            //STEP 2: Process data transformations in pipeline
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth");

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole<IrisData>(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);

            // STEP 3: Create and train the model     
            var trainer = mlContext.Clustering.Trainers.KMeans(features: "Features", clustersCount: 3);
            var trainingPipeline = dataProcessPipeline.Append(trainer);
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP4: Evaluate accuracy of the model
            IDataView predictions = trainedModel.Transform(testingDataView);
            var metrics = mlContext.Clustering.Evaluate(predictions, score: "Score", features: "Features");

            ConsoleHelper.PrintClusteringMetrics(trainer.ToString(), metrics);

            // STEP5: Save/persist the model as a .ZIP file
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

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

            ///
            ITransformer model;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                model = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predFunction = trainedModel.MakePredictionFunction<IrisData, IrisPrediction>(mlContext);

            //Score
            var resultprediction = predFunction.Predict(sampleIrisData);
            ///

            Console.WriteLine($"Cluster assigned for setosa flowers:" + resultprediction.SelectedClusterId);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();           
        }
    }

}
