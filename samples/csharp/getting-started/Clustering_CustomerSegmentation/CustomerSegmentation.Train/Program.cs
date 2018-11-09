using System;
using System.IO;

using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Projections;

using CustomerSegmentation.DataStructures;

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
                //STEP 0: Special data pre-process in this sample creating the PivotTable csv file
                DataHelpers.PreProcessAndSave(offersCsv, transactionsCsv, pivotCsv);

                //Create the MLContext to share across components for deterministic results
                MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

                // STEP 1: Common data loading configuration
                var textLoader = CustomerSegmentationTextLoaderFactory.CreateTextLoader(mlContext);
                var pivotDataView = textLoader.Read(pivotCsv);

                //STEP 2: Configure data transformations in pipeline
                var dataProcessPipeline =  new PrincipalComponentAnalysisEstimator(mlContext, "Features", "PCAFeatures", rank: 2)
                                                .Append(new OneHotEncodingEstimator(mlContext, new[] { new OneHotEncodingEstimator.ColumnInfo("LastName",
                                                                                                                                              "LastNameKey",
                                                                                                                                              CategoricalTransform.OutputKind.Ind) }));
                // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
                Common.ConsoleHelper.PeekDataViewInConsole<PivotObservation>(mlContext, pivotDataView, dataProcessPipeline, 10);
                Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", pivotDataView, dataProcessPipeline, 10);

                // STEP 3: Create and train the model                
                var trainer = mlContext.Clustering.Trainers.KMeans("Features", clustersCount: 3);
                var modelBuilder = new Common.ModelBuilder<PivotObservation, ClusteringPrediction>(mlContext, dataProcessPipeline);
                modelBuilder.AddTrainer(trainer);
                var trainedModel = modelBuilder.Train(pivotDataView);

                // STEP4: Evaluate accuracy of the model
                var metrics = modelBuilder.EvaluateClusteringModel(pivotDataView);
                Common.ConsoleHelper.PrintClusteringMetrics(trainer.ToString(), metrics);

                // STEP5: Save/persist the model as a .ZIP file
                modelBuilder.SaveModelAsFile(modelZip);

            } catch (Exception ex)
            {
                Common.ConsoleHelper.ConsoleWriteException(ex.Message);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }
    }
}
