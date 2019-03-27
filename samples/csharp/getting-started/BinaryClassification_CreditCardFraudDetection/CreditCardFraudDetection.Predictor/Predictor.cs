using CreditCardFraudDetection.Common;
using CreditCardFraudDetection.Common.DataModels;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.Linq;

namespace CreditCardFraudDetection.Predictor
{
    public class Predictor
    {
        private readonly string _modelfile;
        private readonly string _dasetFile;

        public Predictor(string modelfile, string dasetFile) {
            _modelfile = modelfile ?? throw new ArgumentNullException(nameof(modelfile));
            _dasetFile = dasetFile ?? throw new ArgumentNullException(nameof(dasetFile));
        }

        public void RunMultiplePredictions(int numberOfPredictions) {

            var mlContext = new MLContext();

            //Load data sa input for predictions
            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(_dasetFile, separatorChar: ',', hasHeader: true);

            Console.WriteLine($"Predictions from saved model:");

            ITransformer model;
            using (var file = File.OpenRead(_modelfile))
            {
                model = mlContext.Model.Load(file);
            }

            var predictionEngine = model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(mlContext);
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
                                        predictionEngine.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });
        }
     
    }
}
