using System;
using System.Linq;

using Microsoft.ML;

using CreditCardFraudDetection.Common.DataModels;

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

            // Load data as input for predictions
            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(_dasetFile, separatorChar: ',', hasHeader: true);

            Console.WriteLine($"Predictions from saved model:");

            ITransformer model = mlContext.Model.Load(_modelfile, out var inputSchema);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);

            Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should be predicted as fraud (true):");

            mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                        .Where(x => x.Label > 0)
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
                       .Where(x => x.Label < 1)
                       .Take(numberOfPredictions)
                       .ToList()
                       .ForEach(testData =>
                                   {
                                       Console.WriteLine($"--- Transaction ---");
                                       testData.PrintToConsole();
                                       predictionEngine.Predict(testData).PrintToConsole();
                                       Console.WriteLine($"-------------------");
                                   });
        }
    }
}
