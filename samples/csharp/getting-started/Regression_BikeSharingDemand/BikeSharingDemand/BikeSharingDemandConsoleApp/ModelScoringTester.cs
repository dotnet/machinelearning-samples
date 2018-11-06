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
using Common;

namespace BikeSharingDemand
{
    public static class ModelScoringTester
    {
        public static void VisualizeSomePredictions(MLContext mlContext,
                                                    string modelName, 
                                                    string testDataLocation, 
                                                    ModelScorer<DemandObservation, DemandPrediction> modelScorer, 
                                                    int numberOfPredictions)
        {
            //Make a few prediction tests 
            // Make the provided number of predictions and compare with observed data from the test dataset
            var testData = ReadSampleDataFromCsvFile(testDataLocation, numberOfPredictions);

            for (int i = 0; i < numberOfPredictions; i++)
            {
                var prediction = modelScorer.PredictSingle(testData[i]);

                Common.ConsoleHelper.PrintRegressionPredictionVersusObserved(prediction.PredictedCount.ToString(), 
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
