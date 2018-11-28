using CreditCardFraudDetection.Common;
using CreditCardFraudDetection.Common.DataModels;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Data.IO;
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

        public void RunMultiplePredictions(int numberOfTransactions, int? seed = 1) {

            var mlContext = new MLContext(seed);

            TextLoader.Column[] columnsPlus = new[] {
                    // A boolean column depicting the 'label'.
                    new TextLoader.Column("Label", DataKind.BL, 0),
                    // 30 Features V1..V28 + Amount + StratificationColumn
                    new TextLoader.Column("V1", DataKind.R4, 1 ),
                    new TextLoader.Column("V2", DataKind.R4, 2 ),
                    new TextLoader.Column("V3", DataKind.R4, 3 ),
                    new TextLoader.Column("V4", DataKind.R4, 4 ),
                    new TextLoader.Column("V5", DataKind.R4, 5 ),
                    new TextLoader.Column("V6", DataKind.R4, 6 ),
                    new TextLoader.Column("V7", DataKind.R4, 7 ),
                    new TextLoader.Column("V8", DataKind.R4, 8 ),
                    new TextLoader.Column("V9", DataKind.R4, 9 ),
                    new TextLoader.Column("V10", DataKind.R4, 10 ),
                    new TextLoader.Column("V11", DataKind.R4, 11 ),
                    new TextLoader.Column("V12", DataKind.R4, 12 ),
                    new TextLoader.Column("V13", DataKind.R4, 13 ),
                    new TextLoader.Column("V14", DataKind.R4, 14 ),
                    new TextLoader.Column("V15", DataKind.R4, 15 ),
                    new TextLoader.Column("V16", DataKind.R4, 16 ),
                    new TextLoader.Column("V17", DataKind.R4, 17 ),
                    new TextLoader.Column("V18", DataKind.R4, 18 ),
                    new TextLoader.Column("V19", DataKind.R4, 19 ),
                    new TextLoader.Column("V20", DataKind.R4, 20 ),
                    new TextLoader.Column("V21", DataKind.R4, 21 ),
                    new TextLoader.Column("V22", DataKind.R4, 22 ),
                    new TextLoader.Column("V23", DataKind.R4, 23 ),
                    new TextLoader.Column("V24", DataKind.R4, 24 ),
                    new TextLoader.Column("V25", DataKind.R4, 25 ),
                    new TextLoader.Column("V26", DataKind.R4, 26 ),
                    new TextLoader.Column("V27", DataKind.R4, 27 ),
                    new TextLoader.Column("V28", DataKind.R4, 28 ),
                    new TextLoader.Column("Amount", DataKind.R4, 29 ),
                    new TextLoader.Column("StratificationColumn", DataKind.R4, 30 )
                };

            //LoaderOptimization test data into DataView
            var dataTest = mlContext.Data.ReadFromTextFile(columnsPlus, _dasetFile,
                                                                          advancedSettings: s => {
                                                                              s.HasHeader = true;
                                                                              s.Separator = ",";
                                                                          }
                                                                         );

            //Inspect/Peek data from datasource
            ConsoleHelpers.ConsoleWriterSection($"Inspect {numberOfTransactions} transactions observed as fraud and {numberOfTransactions} not observed as fraud, from the test datasource:");
            ConsoleHelpers.InspectData(mlContext, dataTest, numberOfTransactions);

            ConsoleHelpers.ConsoleWriteHeader($"Predictions from saved model:");

            ITransformer model;
            using (var file = File.OpenRead(_modelfile))
            {
                model = mlContext.Model.Load(file);
            }

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
