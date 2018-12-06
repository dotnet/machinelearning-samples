using System;
using System.IO;

using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Projections;

using CustomerSegmentation.DataStructures;
using Common;

namespace CustomerSegmentation
{
    public class Program
    {
        static void Main(string[] args)
        {
            var assetsPath = PathHelper.GetAssetsPath(@"..\..\..\assets");

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
                TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                        {
                                            Separator = ",",
                                            HasHeader = true,
                                            Column = new[]
                                                        {
                                                        new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
                                                        new TextLoader.Column("LastName", DataKind.Text, 32)
                                                        }
                                        });

                var pivotDataView = textLoader.Read(pivotCsv);

                //STEP 2: Configure data transformations in pipeline
                var dataProcessPipeline =  new PrincipalComponentAnalysisEstimator(mlContext, "Features", "PCAFeatures", rank: 2)
                                                .Append(new OneHotEncodingEstimator(mlContext, new[] { new OneHotEncodingEstimator.ColumnInfo("LastName",
                                                                                                                                              "LastNameKey",
                                                                                                                                              OneHotEncodingTransformer.OutputKind.Ind) }));
                // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
                Common.ConsoleHelper.PeekDataViewInConsole<PivotObservation>(mlContext, pivotDataView, dataProcessPipeline, 10);
                Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", pivotDataView, dataProcessPipeline, 10);

                //STEP 3: Create the training pipeline                
                var trainer = mlContext.Clustering.Trainers.KMeans("Features", clustersCount: 3);
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                //STEP 4: Train the model fitting to the pivotDataView
                Console.WriteLine("=============== Training the model ===============");
                ITransformer trainedModel = trainingPipeline.Fit(pivotDataView);

                //STEP 5: Evaluate the model and show accuracy stats
                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                var predictions = trainedModel.Transform(pivotDataView);
                var metrics = mlContext.Clustering.Evaluate(predictions, score: "Score", features: "Features");

                ConsoleHelper.PrintClusteringMetrics(trainer.ToString(), metrics);

                //STEP 6: Save/persist the trained model to a .ZIP file
                using (var fs = new FileStream(modelZip, FileMode.Create, FileAccess.Write, FileShare.Write))
                    mlContext.Model.Save(trainedModel, fs);

                Console.WriteLine("The model is saved to {0}", modelZip);
            }
            catch (Exception ex)
            {
                Common.ConsoleHelper.ConsoleWriteException(ex.Message);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }
    }
}
