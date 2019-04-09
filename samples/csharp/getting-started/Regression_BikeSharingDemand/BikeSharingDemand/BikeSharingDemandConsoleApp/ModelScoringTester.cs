using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.ML;

using Common;
using BikeSharingDemand.DataStructures;

namespace BikeSharingDemand
{
    public static class ModelScoringTester
    {
        public static void VisualizeSomePredictions(MLContext mlContext,
                                                    string modelName, 
                                                    string testDataLocation,
                                                    PredictionEngine<DemandObservation, DemandPrediction> predEngine,
                                                    int numberOfPredictions)
        {
            //Make a few prediction tests 
            // Make the provided number of predictions and compare with observed data from the test dataset
            var testData = ReadSampleDataFromCsvFile(testDataLocation, numberOfPredictions);

            for (int i = 0; i < numberOfPredictions; i++)
            {
                //Score
                var resultprediction = predEngine.Predict(testData[i]);

                Common.ConsoleHelper.PrintRegressionPredictionVersusObserved(resultprediction.PredictedCount.ToString(), 
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
                    Season = x[2].ToFloatWithInvariantCulture(),
                    Year = x[3].ToFloatWithInvariantCulture(),
                    Month = x[4].ToFloatWithInvariantCulture(),
                    Hour = x[5].ToFloatWithInvariantCulture(),
                    Holiday = x[6].ToFloatWithInvariantCulture(),
                    Weekday = x[7].ToFloatWithInvariantCulture(),
                    WorkingDay = x[8].ToFloatWithInvariantCulture(),
                    Weather = x[9].ToFloatWithInvariantCulture(),
                    Temperature = x[10].ToFloatWithInvariantCulture(),
                    NormalizedTemperature = x[11].ToFloatWithInvariantCulture(),
                    Humidity = x[12].ToFloatWithInvariantCulture(),
                    Windspeed = x[13].ToFloatWithInvariantCulture(),
                    Count = x[16].ToFloatWithInvariantCulture()
                })
                .Take(numberOfRecordsToRead)
                .ToList();
        }
    }
}
