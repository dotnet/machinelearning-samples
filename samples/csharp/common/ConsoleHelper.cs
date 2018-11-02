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

using System.Reflection;

namespace Common
{
    public static class ConsoleHelper
    {
        public static void PrintPrediction(string prediction)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"Predicted : {prediction}");
            Console.WriteLine($"*************************************************");
        }

        public static void PrintRegressionPredictionVersusObserved(string predictionCount, string observedCount)
        {
            Console.WriteLine($"-------------------------------------------------");
            Console.WriteLine($"Predicted : {predictionCount}");
            Console.WriteLine($"Actual:     {observedCount}");
            Console.WriteLine($"-------------------------------------------------");
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

        public static void PrintClusteringMetrics(string name, ClusteringEvaluator.Result metrics)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for {name} clustering model      ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       AvgMinScore: {metrics.AvgMinScore}");
            Console.WriteLine($"*       Dbi is: {metrics.Dbi}");
            Console.WriteLine($"*************************************************");
        }

        public static List<TObservation> PeekDataViewInConsole<TObservation>(MLContext mlContext, IDataView dataView, IEstimator<ITransformer> pipeline, int numberOfRows = 4)
            where TObservation : class, new()
        {
            string msg = string.Format("Showing {0} rows with all the columns", numberOfRows.ToString());
            ConsoleWriteHeader(msg);

            //https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md#how-do-i-look-at-the-intermediate-data
            var transformedData = pipeline.Fit(dataView).Transform(dataView);

            // 'transformedData' is a 'promise' of data, lazy-loading. Let's actually read it.
            // Convert to an enumerable of user-defined type.
            var someRows = transformedData.AsEnumerable<TObservation>(mlContext, reuseRowObject: false)
                                           // Take the specified number of rows
                                           .Take(numberOfRows)
                                           // Convert to List
                                           .ToList();

            someRows.ForEach(row =>
                                {
                                    string lineToPrint = "Row--> ";
                                    foreach (FieldInfo field in row.GetType().GetFields())
                                    {
                                        lineToPrint += $"| {field.Name}: {field.GetValue(row)}";
                                    }
                                    Console.WriteLine(lineToPrint);
                                });

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
