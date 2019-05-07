using CreditCardFraudDetection.Common.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;

namespace CreditCardFraudDetection.Predictor
{
    public class Predictor
    {
        private readonly string _modelfile;
        private readonly string _dasetFile;

        public Predictor(string modelfile, string dasetFile)
        {
            _modelfile = modelfile ?? throw new ArgumentNullException(nameof(modelfile));
            _dasetFile = dasetFile ?? throw new ArgumentNullException(nameof(dasetFile));
        }

        public void RunMultiplePredictions(int numberOfPredictions)
        {

            var mlContext = new MLContext();

            //Load data as input for predictions
            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(_dasetFile, separatorChar: ',', hasHeader: true);

            Console.WriteLine($"Predictions from saved model:");

            ITransformer model = mlContext.Model.Load(_modelfile, out var inputSchema);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPredictionWithContribution>(model);
            Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should be predicted as fraud (true):");

            mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                        .Where(x => x.Label == true)
                        .Take(numberOfPredictions)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData =>
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionEngine.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });


            Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should NOT be predicted as fraud (false):");

            mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                       .Where(x => x.Label == false)
                       .Take(numberOfPredictions)
                       .ToList()
                       .ForEach(testData =>
                                   {
                                       Console.WriteLine($"--- Transaction ---");
                                       testData.PrintToConsole();
                                       predictionEngine.Predict(testData).PrintToConsole(model.GetOutputSchema(inputDataForPredictions.Schema));
                                       Console.WriteLine($"-------------------");
                                   });
        }

        private class TransactionFraudPredictionWithContribution : TransactionFraudPrediction
        {
            public float[] FeatureContributions { get; set; }

            public void PrintToConsole(DataViewSchema dataview)
            {
                base.PrintToConsole();
                VBuffer<ReadOnlyMemory<char>> slots = default;
                dataview.GetColumnOrNull("Features").Value.GetSlotNames(ref slots);
                var featureNames = slots.DenseValues().ToArray();
                Console.WriteLine($"Feature Contributions: " +
                                  $"[{featureNames[0]}] {FeatureContributions[0]} " +
                                  $"[{featureNames[1]}] {FeatureContributions[1]} " +
                                  $"[{featureNames[2]}] {FeatureContributions[2]} ... " +
                                  $"[{featureNames[27]}] {FeatureContributions[27]} " +
                                  $"[{featureNames[28]}] {FeatureContributions[28]}");
            }
        }
    }
}
