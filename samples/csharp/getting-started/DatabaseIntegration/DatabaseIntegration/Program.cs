using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Trainers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseIntegration
{
    public class Program
    {
        // The url for the dataset that will be downloaded
        public static string datasetUrl = "https://raw.githubusercontent.com/dotnet/machinelearning/244a8c2ac832657af282aa312d568211698790aa/test/data/adult.train";

        public static IEnumerable<string> ReadRemoteDataset(string url)
        {
            using(var client = new WebClient())
            using(var stream = client.OpenRead(url))
            using(var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Wrapper function that performs the database query and returns an IEnumerable, creating
        /// a database context each time.
        /// </summary>
        /// <remarks>
        /// ML.Net can traverse an IEnumerable with multiple threads. This will result in Entity Core Framwork throwing an exception
        /// as multiple threads cannot access the same database context. To work around this, create a database context
        /// each time a IEnumerable is requested.
        /// </remarks>
        /// <returns>An IEnumerable of the resulting data.</returns>
        private static IEnumerable<AdultCensus> QueryData()
        {
            using (var db = new AdultCensusContext())
            {
                // Query our training data from the database. This query is selecting everything from the AdultCensus table. The
                // result is then loaded by ML.Net through the LoadFromEnumerable. LoadFromEnumerable returns an IDataView which
                // can be consumed by an ML.Net pipeline.
                // NOTE: For training, ML.Net requires that the training data is processed in the same order to produce consistent results.
                // Therefore we are sorting the data by the AdultCensusId, which is an auto-generated id.
                // NOTE: That the query used here sets the query tracking behavior to be NoTracking, this is particularly useful because
                // our scenarios only require read-only access.
                foreach (var adult in db.AdultCensus.AsNoTracking().OrderBy(x => x.AdultCensusId))
                {
                    yield return adult;
                }
            }
        }

        /// <summary>
        /// Populates the database with the specified dataset url.
        /// </summary>
        public static void CreateDatabase(string url)
        {
            var dataset = ReadRemoteDataset(url);
            using (var db = new AdultCensusContext())
            {
                // Ensure that we have a clean database to start with.
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                Console.WriteLine($"Database created, populating...");

                // Parse the dataset.
                var data = dataset
                    .Skip(1) // Skip the header row
                    .Select(l => l.Split(','))
                    .Where(row => row.Length > 1)
                    .Select(row => new AdultCensus()
                    {
                        Age = int.Parse(row[0]),
                        Workclass = row[1],
                        Education = row[3],
                        MaritalStatus = row[5],
                        Occupation = row[6],
                        Relationship = row[7],
                        Race = row[8],
                        Sex = row[9],
                        CapitalGain = row[10],
                        CapitalLoss = row[11],
                        HoursPerWeek = int.Parse(row[12]),
                        NativeCountry = row[13],
                        Label = (int.Parse(row[14]) == 1) ? true : false
                    });

                // Add the data into the database
                db.AdultCensus.AddRange(data);

                var count = db.SaveChanges();
                Console.WriteLine($"Total count of items saved to database: {count}");
            }
        }

        public static void Main()
        {
            // Seed the database with the dataset.
            CreateDatabase(datasetUrl);
            var mlContext = new MLContext(seed: 1);

            /// Query the data from the database, please see <see cref="QueryData"/> for more information.
            var dataView = mlContext.Data.LoadFromEnumerable(QueryData());
            /// Creates the training and testing data sets.
            var trainTestData = mlContext.Data.TrainTestSplit(dataView);

            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] {
                new InputOutputColumnPair("MsOHE", "MaritalStatus"),
                new InputOutputColumnPair("OccOHE", "Occupation"),
                new InputOutputColumnPair("RelOHE", "Relationship"),
                new InputOutputColumnPair("SOHE", "Sex"),
                new InputOutputColumnPair("NatOHE", "NativeCountry")
            }, OneHotEncodingEstimator.OutputKind.Binary)
                .Append(mlContext.Transforms.Concatenate("Features", "MsOHE", "OccOHE", "RelOHE", "SOHE", "NatOHE"))
                .Append(mlContext.BinaryClassification.Trainers.LightGbm());

            Console.WriteLine("Training model...");
            var model = pipeline.Fit(trainTestData.TrainSet);

            Console.WriteLine("Predicting...");

            // Now that the model is trained, we want to test it's prediction results, which is done by using a test dataset
            var predictions = model.Transform(trainTestData.TestSet);

            // Now that we have the predictions, calculate the metrics of those predictions and output the results.
            var metrics = mlContext.BinaryClassification.Evaluate(predictions);
            ConsoleHelper.PrintBinaryClassificationMetrics("Database Example", metrics);
            ConsoleHelper.ConsolePressAnyKey();
        }
    }
}