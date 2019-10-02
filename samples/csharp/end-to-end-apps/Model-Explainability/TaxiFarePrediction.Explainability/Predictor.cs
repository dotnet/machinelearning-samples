using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using TaxiFareRegression.DataStructures;


namespace TaxiFareRegression.Explainability
{
    public class Predictor
    {
        private readonly string _modelfile;
        private readonly string _datasetFile;
        private static MLContext context;
        private static ITransformer model;
        private static PredictionEngine<TaxiTrip, TaxiTripFarePredictionWithContribution> predictionEngine;
        public Predictor(string modelfile, string datasetFile)
        {
            _modelfile = modelfile ?? throw new ArgumentNullException(nameof(modelfile));
            _datasetFile = datasetFile ?? throw new ArgumentNullException(nameof(datasetFile));

            context = new MLContext();

            model = context.Model.Load(_modelfile, out var inputSchema);

            predictionEngine = context.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePredictionWithContribution>(model);
        }

        public List<DataStructures.TaxiFarePrediction> RunMultiplePredictions(int numberOfPredictions)
        {

            // Load data as input for predictions.
            IDataView inputDataForPredictions = context.Data.LoadFromTextFile<TaxiTrip>(_datasetFile, hasHeader: true, separatorChar: ',');

            Console.WriteLine("Predictions from saved model:");

            Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should be predicted as fraud (true):");

            var transactionList = new List<DataStructures.TaxiFarePrediction>();
            TaxiTripFarePredictionWithContribution prediction;
            DataStructures.TaxiFarePrediction explainedPrediction;

            context.Data.CreateEnumerable<TaxiTrip>(inputDataForPredictions, reuseRowObject: false)
                        .Take(numberOfPredictions)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData =>
                                    {
                                        testData.PrintToConsole();
                                        prediction = predictionEngine.Predict(testData);
                                        explainedPrediction = new DataStructures.TaxiFarePrediction(prediction.FareAmount, prediction.GetFeatureContributions(model.GetOutputSchema(inputDataForPredictions.Schema)));
                                        transactionList.Add(explainedPrediction);
                                    });

            return transactionList;
        }

    }
}
