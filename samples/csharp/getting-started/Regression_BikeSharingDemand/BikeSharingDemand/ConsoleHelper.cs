using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

using Microsoft.ML.Core.Data;
using System.Collections.Generic;
using Microsoft.ML.Data;
using Microsoft.ML;

namespace BikeSharingDemand.Helpers
{
    public static class ConsoleHelper
    {
        public static void PrintPrediction(BikeSharingData.Prediction prediction)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"Predicted : {prediction.PredictedCount}");
            Console.WriteLine($"*************************************************");
        }

        public static void PrintRegressionMetrics(string name, RegressionEvaluator.Result metrics)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for {name} regression model      ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       LossFn: {metrics.LossFn:0.##}");
            Console.WriteLine($"*       R2 Score: {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Absolute loss: {metrics.L1:#.##}");
            Console.WriteLine($"*       Squared loss: {metrics.L2:#.##}");
            Console.WriteLine($"*       RMS loss: {metrics.Rms:#.##}");
            Console.WriteLine($"*************************************************");
        }

        public static List<BikeSharingData.Demand> PeekDataViewInConsole(MLContext mlContext, IDataView dataView, IEstimator<ITransformer> pipeline, int numberOfRows = 4)
        {
            string msg = string.Format("Show {0} rows with all the columns", numberOfRows.ToString());
            ConsoleWriteHeader(msg);

            //https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md#how-do-i-look-at-the-intermediate-data
            var transformedData = pipeline.Fit(dataView).Transform(dataView);

            // 'transformedData' is a 'promise' of data, lazy-loading. Let's actually read it.
            // Convert to an enumerable of user-defined type.
            var someRows = transformedData.AsEnumerable<BikeSharingData.Demand>(mlContext, reuseRowObject: false)
                                           //.Where(x => x.Count > 0)
                                           // Take a couple values as an array.
                                           .Take(numberOfRows)
                                           .ToList();

            // print to console the peeked rows
            someRows.ForEach(row => { Console.WriteLine($"Label [Count]: {row.Count} || Features: [Season] {row.Season} [Year] {row.Year} [Month] {row.Month} [Hour] {row.Hour} [Holiday] {row.Holiday} [Weekday] {row.Weekday} [WorkingDay] {row.WorkingDay} [Weather] {row.Weather} [Temperature] {row.Temperature} [NormalizedTemperature] {row.NormalizedTemperature} [Humidity] {row.Humidity} [Windspeed] {row.Windspeed} "); });

            return someRows;
        }

        public static List<float[]> PeekFeaturesColumnDataInConsole(MLContext mlContext, string columnName, IDataView dataView, IEstimator<ITransformer> pipeline, int numberOfRows = 4)
        {
            string msg = string.Format("Show {0} rows with just the '{1}' column", numberOfRows, columnName );
            ConsoleWriteHeader(msg);

            var transformedData = pipeline.Fit(dataView).Transform(dataView);
            // Extract the 'Features' column.
            
            var someColumnData = transformedData.GetColumn<float[]>(mlContext, columnName)
                                                        .Take(numberOfRows).ToList();

            // print to console the peeked rows
            someColumnData.ForEach(row => {
                                            String concatColumn = String.Empty;
                                            foreach (float f in row)
                                            {
                                                concatColumn += f.ToString();                                              
                                            }
                                            Console.WriteLine(concatColumn);
                                          });

            return someColumnData;
        }

        public static void ConsoleWriteHeader(params string[] lines)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" ");
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            var maxLength = lines.Select(x => x.Length).Max();
            Console.WriteLine(new string('#', maxLength));
            Console.ForegroundColor = defaultColor;
        }

        public static void ConsoleWriterSection(params string[] lines)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(" ");
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            var maxLength = lines.Select(x => x.Length).Max();
            Console.WriteLine(new string('-', maxLength));
            Console.ForegroundColor = defaultColor;
        }

        public static void ConsolePressAnyKey()
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" ");
            Console.WriteLine("Press any key to finish.");
            Console.ReadKey();
        }

        public static void ConsoleWriteException(params string[] lines)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            const string exceptionTitle = "EXCEPTION";
            Console.WriteLine(" ");
            Console.WriteLine(exceptionTitle);
            Console.WriteLine(new string('#', exceptionTitle.Length));
            Console.ForegroundColor = defaultColor;
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        public static void ConsoleWriteWarning(params string[] lines)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            const string warningTitle = "WARNING";
            Console.WriteLine(" ");
            Console.WriteLine(warningTitle);
            Console.WriteLine(new string('#', warningTitle.Length));
            Console.ForegroundColor = defaultColor;
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

    }
}
