using CreditCardFraudDetection.Common.DataModels;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CreditCardFraudDetection.Common
{
    public static class ConsoleHelpers
    {
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

        public static string GetAssetsPath(params string[] paths)
        {

            FileInfo _dataRoot = new FileInfo(typeof(ConsoleHelpers).Assembly.Location);
            if (paths == null || paths.Length == 0)
                return null;

            return Path.Combine(paths.Prepend(_dataRoot.Directory.FullName).ToArray());
        }

        public static string DeleteAssets(params string[] paths)
        {
            var location = GetAssetsPath(paths);

            if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
                File.Delete(location);
            return location;
        }

        public static void InspectData(LocalEnvironment env, IDataView data)
        {
            // lets inspect data
            //ConsoleWriteHeader("Show 4");
            ShowVectorModel(env, data, label: true);
            ShowVectorModel(env, data, label: false);
        }

        public static void InspectScoredData(LocalEnvironment env, IDataView data)
        {
            // lets inspect data
            //ConsoleWriteHeader("Show 4");
            ShowEstimatorModel(env, data, label: true);
            ShowEstimatorModel(env, data, label: false);
        }

        public static void ShowVectorModel(LocalEnvironment env, IDataView data, bool label = true, int count = 2)
        {
            data
               // Convert to an enumerable of user-defined type. 
               .AsEnumerable<TransactionVectorModel>(env, reuseRowObject: false)
               .Where(x => x.Label == label)
               // Take a couple values as an array.
               .Take(count)
               .ToList()
               // print to console
               .ForEach(row => { row.PrintToConsole(); });
        }

        public static void ShowEstimatorModel(LocalEnvironment env, IDataView data, bool label = true, int count = 2)
        {
            data
               // Convert to an enumerable of user-defined type. 
               .AsEnumerable<TransactionEstimatorModel>(env, reuseRowObject: false)
               .Where(x => x.Label == label)
               // Take a couple values as an array.
               .Take(count)
               .ToList()
               // print to console
               .ForEach(row => { row.PrintToConsole(); });
        }

        public static void UnZipDataSet(string zipDataSet, string destinationFile)
        {
            if (!File.Exists(destinationFile))
            {
                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                ZipFile.ExtractToDirectory(zipDataSet, $"{destinationDirectory}");
            }
        }
    }

}
