using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using BikeSharingDemand.DataStructures;

using Microsoft.ML;

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
                    Season = float.Parse(x[2], CultureInfo.InvariantCulture),
                    Year = float.Parse(x[3], CultureInfo.InvariantCulture),
                    Month = float.Parse(x[4], CultureInfo.InvariantCulture),
                    Hour = float.Parse(x[5], CultureInfo.InvariantCulture),
                    Holiday = float.Parse(x[6], CultureInfo.InvariantCulture),
                    Weekday = float.Parse(x[7], CultureInfo.InvariantCulture),
                    WorkingDay = float.Parse(x[8], CultureInfo.InvariantCulture),
                    Weather = float.Parse(x[9], CultureInfo.InvariantCulture),
                    Temperature = float.Parse(x[10], CultureInfo.InvariantCulture),
                    NormalizedTemperature = float.Parse(x[11], CultureInfo.InvariantCulture),
                    Humidity = float.Parse(x[12], CultureInfo.InvariantCulture),
                    Windspeed = float.Parse(x[13], CultureInfo.InvariantCulture),
                    Count = float.Parse(x[16], CultureInfo.InvariantCulture)
                })
                .Take(numberOfRecordsToRead)
                .ToList();
        }
    }
}
