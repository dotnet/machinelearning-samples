using Microsoft.ML;
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
        private static string Download(string baseGitPath, string dataFile)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri($"{baseGitPath}"), dataFile);
            }

            return dataFile;
        }

        public static string DownloadAdultDataset()
            => Download("https://raw.githubusercontent.com/dotnet/machinelearning/244a8c2ac832657af282aa312d568211698790aa/test/data/adult.train", "adult.txt");

        public static void Seeding()
        {
            string adultDataset = DownloadAdultDataset();
            using (var db = new AdultCensusContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                var data = File.ReadLines(adultDataset)
                    .Skip(1) // Skip the header row
                    .Select(l => l.Split(','))
                    .Where(row => row.Length > 1)
                    .Select(row => new AdultCensus()
                    {
                            Age = int.Parse(row[0]),
                            Workclass = row[1],
                            Education = row[3],
                            //EducationNum = int.Parse(data[4]),
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
                db.AdultCensus.AddRange(data);
                var count = db.SaveChanges();
                Console.WriteLine($"Save results {count}");
            }
        }

        public static void Main()
        {
            Seeding();
            using(var db = new AdultCensusContext())
            {
                var mlContext =  new MLContext();
                
                // NOTE: For training, we are sorting the data by the AdultCensusId, which is an auto-generated id. ML.Net requires
                // that the training data is processed in the same order to produce consistent results with the model.
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
                var test = db.AdultCensus.Take(2000).ToList<AdultCensus>();

                var testDataView = mlContext.Data.LoadFromEnumerable(db.AdultCensus.Take(2000).ToList<AdultCensus>());
                var predictions = model.Transform(testDataView);
                var metrics = mlContext.BinaryClassification.Evaluate(predictions);
                ConsoleHelper.PrintBinaryClassificationMetrics("Database Example", metrics);
                ConsoleHelper.ConsolePressAnyKey();
            }
        }
    }
}