using System;
using CustomerSegmentation.Model;
using System.IO;
using System.Threading.Tasks;
using CustomerSegmentation.DataStructures;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML;
using CustomerSegmentation.Train.DataStructures;
using Microsoft.ML.Trainers.KMeans;

namespace CustomerSegmentation
{
    public class Program
    {
        static void Main(string[] args)
        {
            var assetsPath = ModelHelpers.GetAssetsPath(@"..\..\..\assets");

            var transactionsCsv = Path.Combine(assetsPath, "inputs", "transactions.csv");
            var offersCsv = Path.Combine(assetsPath, "inputs", "offers.csv");
            var pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv");
            var modelZip = Path.Combine(assetsPath, "outputs", "retailClustering.zip");

            try
            {
                //DataHelpers.PreProcessAndSave(offersCsv, transactionsCsv, pivotCsv);
                //var modelBuilder = new ModelBuilder(pivotCsv, modelZip, kValuesSvg);
                //modelBuilder.BuildAndTrain();

                //STEP 0: Special data pre-process in this sample creating the PivotTable csv file
                DataHelpers.PreProcessAndSave(offersCsv, transactionsCsv, pivotCsv);

                //Create the MLContext to share across components for deterministic results
                MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

                //STEP 1: Common data loading
                DataLoader dataLoader = new DataLoader(mlContext);
                var pivotDataView = dataLoader.GetDataView(pivotCsv);

                //STEP 1: Process data transformations in pipeline
                var dataProcessor = new DataProcessor(mlContext, 2);
                var dataProcessPipeline = dataProcessor.DataProcessPipeline;

                // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
                Common.ConsoleHelper.PeekDataViewInConsole<PivotObservation>(mlContext, pivotDataView, dataProcessPipeline, 10);
                Common.ConsoleHelper.PeekFeaturesColumnDataInConsole(mlContext, "Features", pivotDataView, dataProcessPipeline, 10);

                // STEP 2: Create and train the model                
                var trainer = mlContext.Clustering.Trainers.KMeans("Features", clustersCount: 3);
                var modelBuilder = new Common.ModelBuilder<PivotObservation, ClusteringPrediction>(mlContext, dataProcessPipeline, trainer);
                var trainedModel = modelBuilder.Train(pivotDataView);

                // STEP3: Evaluate accuracy of the model
                var metrics = modelBuilder.EvaluateClusteringModel(pivotDataView);
                Common.ConsoleHelper.PrintClusteringMetrics("KMeansPlusPlus", metrics);

                // STEP3: Save/persist the model as a .ZIP file
                modelBuilder.SaveModelAsFile(modelZip);

                Console.WriteLine("Press any key to exit..");
                Console.ReadLine();

            } catch (Exception ex)
            {
                Common.ConsoleHelper.ConsoleWriteException(ex.Message);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }
    }
}
