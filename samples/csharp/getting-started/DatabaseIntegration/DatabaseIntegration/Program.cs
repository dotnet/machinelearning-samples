using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using Common;
using System;
using System.IO;
using System.Net;
using System.Linq;
using Microsoft.ML.Transforms;

namespace DatabaseIntegration
{
    public class Program
    {

        /// <summary>
        /// Helper function to download a file given a base uri
        /// </summary>
        /// <param name="uriPath">The uri of where to download the file.</param>
        /// <param name="dataFile">The location of where to save the downloaded file.</param>
        /// <returns></returns>
        private static string Download(string uriPath, string dataFile)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri($"{uriPath}"), dataFile);
            }

            return dataFile;
        }

        public static string DownloadAdultDataset()
            => Download("https://raw.githubusercontent.com/dotnet/machinelearning/244a8c2ac832657af282aa312d568211698790aa/test/data/adult.train", "adult.txt");

        /// <summary>
        /// Populates the database with the AdultCensus dataset.
        /// </summary>
        public static void Seeding()
        {
            string adultDataset = DownloadAdultDataset();
            using (var db = new AdultCensusContext())
            {
                // Ensure that we have a clean database to start with.
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // Parse the downloaded file
                var data = File.ReadLines(adultDataset)
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
                Console.WriteLine($"Save results {count}");
            }
        }

        public static void Main()
        {
            // Seed the database with the dataset.
            Seeding();

            using(var db = new AdultCensusContext())
            {
                // Set the query tracking behavior to be NoTracking, this is particularly useful because
                // our scenarios our read-only with the data. Not having tracking will  
                db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                var mlContext =  new MLContext();
               
                // Query our training data from the database. This query is selecting everything from the AdultCensus table. The
                // result is then loaded by ML.Net through the LoadFromEnumerable. LoadFromEnumerable returns an IDataView which
                // can be consumed by an ML.Net pipeline.
                // NOTE: For training, ML.Net requires that the training data is processed in the same order to produce consistent results.
                // Therefore we are sorting the data by the AdultCensusId, which is an auto-generated id.
                var dataView = mlContext.Data.LoadFromEnumerable(db.AdultCensus.OrderBy(x=>x.AdultCensusId).ToList<AdultCensus>());
                var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(new [] {
                        new InputOutputColumnPair("MsOHE", "MaritalStatus"), 
                        new InputOutputColumnPair("OccOHE", "Occupation"),
                        new InputOutputColumnPair("RelOHE", "Relationship"),
                        new InputOutputColumnPair("SOHE", "Sex"), 
                        new InputOutputColumnPair("NatOHE", "NativeCountry")
                    }, OneHotEncodingEstimator.OutputKind.Binary)
                    .Append(mlContext.Transforms.Concatenate("Features", "MsOHE", "OccOHE", "RelOHE", "SOHE", "NatOHE"))
                    .Append(mlContext.BinaryClassification.Trainers.LightGbm());

                Console.WriteLine("Training model...");
                var model = pipeline.Fit(dataView);

                Console.WriteLine("Predicting...");
                // Now that the model is trained, we want to test it's prediction results, which is done by using a test dataset
                // and transforming it by the trained model.
                // To retrieve our test data, this issues a second query, retrieving the first 2000 records, then converts to an IDataView
                // in order to feed into the ML.Net pipeline.
                var testDataView = mlContext.Data.LoadFromEnumerable(db.AdultCensus.Take(2000).ToList<AdultCensus>());
                var predictions = model.Transform(testDataView);

                // Now that we have the predictions, calculate th metrics of those predictions and output the results.
                var metrics = mlContext.BinaryClassification.Evaluate(predictions);
                ConsoleHelper.PrintBinaryClassificationMetrics("Database Example", metrics);
                ConsoleHelper.ConsolePressAnyKey();
            }
        }
    }
}