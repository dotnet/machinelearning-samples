using CreditCardFraudDetection.Common;
using CreditCardFraudDetection.Common.DataModels;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Data.IO;
using System;
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

        public void RunMultiplePredictions(int numberOfTransactions, int? seed = 1) {

            var mlContext = new MLContext(seed);

            var binTestData = new BinaryLoader(mlContext, new BinaryLoader.Arguments(), new MultiFileSource(_dasetFile));
            var testRoles = new RoleMappedData(binTestData, roles: TransactionObservation.Roles());
            var dataTest = testRoles.Data;

            //Inspect/Peek data from datasource
            ConsoleHelpers.ConsoleWriterSection($"Inspect {numberOfTransactions} transactions observed as fraud and {numberOfTransactions} not observed as fraud, from the test datasource:");
            ConsoleHelpers.InspectData(mlContext, dataTest, numberOfTransactions);

            ConsoleHelpers.ConsoleWriteHeader($"Predictions from saved model:");
                ITransformer model = mlContext.ReadModel(_modelfile);
                var predictionFunc = model.MakePredictionFunction<TransactionObservation, TransactionFraudPrediction>(mlContext);
                ConsoleHelpers.ConsoleWriterSection($"Test {numberOfTransactions} transactions, from the test datasource, that should be predicted as fraud (true):");
                dataTest.AsEnumerable<TransactionObservation>(mlContext, reuseRowObject: false)
                        .Where(x => x.Label == true)
                        .Take(numberOfTransactions)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData => 
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionFunc.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });


                ConsoleHelpers.ConsoleWriterSection($"Test {numberOfTransactions} transactions, from the test datasource, that should NOT be predicted as fraud (false):");
                dataTest.AsEnumerable<TransactionObservation>(mlContext, reuseRowObject: false)
                        .Where(x => x.Label == false)
                        .Take(numberOfTransactions)
                        .ToList()
                        .ForEach(testData =>
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionFunc.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });
        }
     
    }
}
