using System;
using System.Linq;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using System.Collections.Generic;
using Microsoft.ML.Data;

namespace Regression_TaxiFarePrediction.Helpers
{
    public static class ConsoleHelper
    {
        public static List<TaxiTrip> PeekDataViewInConsole(LocalEnvironment context, IDataView dataView, EstimatorChain<ITransformer> pipeline, int numberOfRows = 4)
        {
            string msg = string.Format("Show {0} rows with all the columns", numberOfRows.ToString());
            ConsoleWriteHeader(msg);

            //https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md#how-do-i-look-at-the-intermediate-data
            var transformedData = pipeline.Fit(dataView).Transform(dataView);

            // 'transformedData' is a 'promise' of data, lazy-loading. Let's actually read it.
            // Convert to an enumerable of user-defined type.
            var someRows = transformedData.AsEnumerable<TaxiTrip>(context, reuseRowObject: false)
                                           //.Where(x => x.Count > 0)
                                           // Take a couple values as an array.
                                           .Take(numberOfRows)
                                           .ToList();

            // print to console the peeked rows
            someRows.ForEach(row => { Console.WriteLine($"Label [FareAmount]: {row.FareAmount} || Features: [RateCode] {row.RateCode} [PassengerCount] {row.PassengerCount} [TripTime] {row.TripTime} [TripDistance] {row.TripDistance} [PaymentType] {row.PaymentType} "); });

            return someRows;
        }

        public static List<float[]> PeekFeaturesColumnDataInConsole(string columnName, LocalEnvironment mlcontext, IDataView dataView, EstimatorChain<ITransformer> pipeline, int numberOfRows = 4)
        {
            string msg = string.Format("Show {0} rows with just the '{1}' column", numberOfRows, columnName);
            ConsoleWriteHeader(msg);

            var transformedData = pipeline.Fit(dataView).Transform(dataView);
            // Extract the 'Features' column.

            var someColumnData = transformedData.GetColumn<float[]>(mlcontext, columnName)
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
