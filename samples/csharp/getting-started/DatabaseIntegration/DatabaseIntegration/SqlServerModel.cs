//using DatabaseIntegration.Models;
using DatabaseIntegration.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Microsoft.ML.Data;

namespace DatabaseIntegration
{
    public class SqlServerModel
    {
        public masterContext dbContext;
        public SqlServerModel()
        {
             dbContext = new masterContext();
        }
        public (IDataView,IDataView) LoadData(MLContext mlContext)
        {
            var fullData = dbContext.CreditCardTransaction;
            IDataView fullDataView = mlContext.Data.LoadFromEnumerable(fullData);

            // Split the data 80:20 into train and test sets, train and evaluate.
            var trainTestData = mlContext.Data.TrainTestSplit(fullDataView, testFraction: 0.2, seed: 1);
            return (trainTestData.TrainSet, trainTestData.TestSet);
        }

        public (ITransformer, string) TrainModel(MLContext mlContext, IDataView trainDataView)
        {  
            //Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] featureColumnNames = trainDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != nameof(CreditCardTransaction.Class)) // Do not include the Label column
                .Where(name => name != nameof(CreditCardTransaction.Idkey))               // Do not include the IdPreservationColumn/StratificationColumn
                .Where(name => name != nameof(CreditCardTransaction.Time)) // Do not include the Time column. Not needed as feature column
                .Where(name => name != "SamplingKeyColumn")
                .ToArray();

            // Create the data process pipeline
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { "Time", nameof(CreditCardTransaction.Idkey) })
                                            .Append(mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
                                                                                 outputColumnName: "FeaturesNormalizedByMeanVar")));

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainDataView, dataProcessPipeline, 2);
            //ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainDataView, dataProcessPipeline, 1);

            // Set the training algorithm
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(CreditCardTransaction.Class),
                                                                                                featureColumnName: "FeaturesNormalizedByMeanVar",
                                                                                                numberOfLeaves: 20,
                                                                                                numberOfTrees: 100,
                                                                                                minimumExampleCountPerLeaf: 10,
                                                                                                learningRate: 0.2);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            ConsoleHelper.ConsoleWriteHeader("=============== Training model ===============");

            var model = trainingPipeline.Fit(trainDataView);

            ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");

            // Append feature contribution calculator in the pipeline. This will be used
            // at prediction time for explainability. 
            var fccPipeline = model.Append(mlContext.Transforms
                .CalculateFeatureContribution(model.LastTransformer)
                .Fit(dataProcessPipeline.Fit(trainDataView).Transform(trainDataView)));

            return (fccPipeline, fccPipeline.ToString());
        }

        public void EvaluateModel(MLContext mlContext, ITransformer model, IDataView testDataView, string trainerName)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = model.Transform(testDataView);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions,
                                                                  labelColumnName: nameof(CreditCardTransaction.Class),
                                                                  scoreColumnName: "Score");
            ConsoleHelper.PrintBinaryClassificationMetrics(trainerName, metrics);
        }

        public void PredictModel(MLContext mlContext, ITransformer model, IDataView predictDataView)
        {
            var predictionEngine = mlContext.Model.CreatePredictionEngine<CreditCardTransaction, TransactionFraudPredictionWithContribution>(model);
            Console.WriteLine($"\n \n Test 5 transactions, from the test datasource, that should be predicted as fraud (true):");

            mlContext.Data.CreateEnumerable<CreditCardTransaction>(predictDataView, reuseRowObject: false)
                       .Where(x => x.Class == true)
                       .Take(5)
                       .Select(predictData => predictData)
                       .ToList()
                       .ForEach(predictData =>
                       {
                           Console.WriteLine($"--- Transaction ---");
                           PrintToConsole(predictData);
                           predictionEngine.Predict(predictData).PrintToConsole();
                           Console.WriteLine($"-------------------");
                       });

            Console.WriteLine($"\n \n Test 5 transactions, from the test datasource, that should NOT be predicted as fraud (false):");

            mlContext.Data.CreateEnumerable<CreditCardTransaction>(predictDataView, reuseRowObject: false)
                       .Where(x => x.Class == false)
                       .Take(5)
                       .Select(predictData => predictData)
                       .ToList()
                       .ForEach(predictData =>
                       {
                           Console.WriteLine($"--- Transaction ---");
                           PrintToConsole(predictData);
                           predictionEngine.Predict(predictData).PrintToConsole();
                           Console.WriteLine($"-------------------");
                       });

        }

        public static void PrintToConsole(CreditCardTransaction transaction)
        {
            Console.WriteLine($"Label: {transaction.Class}");
            Console.WriteLine($"Features: [V1] {transaction.V1} [V2] {transaction.V2} [V3] {transaction.V3} ... [V28] {transaction.V28} Amount: {transaction.Amount}");
        }

        private class TransactionFraudPredictionWithContribution : TransactionFraudPrediction
        {
            public float[] FeatureContributions { get; set; }           
        }
    }
}
