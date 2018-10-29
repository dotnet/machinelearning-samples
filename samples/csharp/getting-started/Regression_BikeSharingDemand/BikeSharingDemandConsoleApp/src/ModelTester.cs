using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;

using BikeSharingDemand.DataStructures;
using BikeSharingDemand.Helpers;

namespace BikeSharingDemand
{
    public static class ModelTester
    {
        public static ITransformer LoadModelFromZipFile(MLContext mlContext, string modelPath)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = TransformerChain.LoadFrom(mlContext, stream);
            }

            return loadedModel;
        }

        public static void VisualizeSomePredictions(string modelName, string testDataLocation, ITransformer model, int numberOfPredictions)
        {
            //Prediction test
            var mlcontext = new MLContext();

            // Create prediction engine 
            var engine = model.MakePredictionFunction<DemandObservation, DemandPrediction>(mlcontext); 

            // Make the provided number of predictions and compare with observed data from the test dataset
            var testData = ReadSampleDataFromCsvFile(testDataLocation, numberOfPredictions);

            for (int i = 0; i < numberOfPredictions; i++)
            {
                var prediction = engine.Predict(testData[i]);

                ConsoleHelper.PrintPredictionVersusObserved(prediction.PredictedCount.ToString(), 
                                                            testData[i].Count.ToString());
            }

        }

        //This method is using regular .NET System.IO.File and LinQ to read just some sample data to test/predict with 
        public static List<DemandObservation> ReadSampleDataFromCsvFile(string dataLocation, int numberOfRecordsToRead)
        {
            return File.ReadLines(dataLocation)
                .Skip(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(','))
                .Select(x => new DemandObservation()
                {
                    Season = float.Parse(x[2]),
                    Year = float.Parse(x[3]),
                    Month = float.Parse(x[4]),
                    Hour = float.Parse(x[5]),
                    Holiday = float.Parse(x[6]),
                    Weekday = float.Parse(x[7]),
                    WorkingDay = float.Parse(x[8]),
                    Weather = float.Parse(x[9]),
                    Temperature = float.Parse(x[10]),
                    NormalizedTemperature = float.Parse(x[11]),
                    Humidity = float.Parse(x[12]),
                    Windspeed = float.Parse(x[13]),
                    Count = float.Parse(x[16])
                })
                .Take(numberOfRecordsToRead)
                .ToList();
        }
    }
}
