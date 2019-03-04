using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Categorical;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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

        public static List<AdultCensus> GetAdultData()
        {
            string adultDataset = DownloadAdultDataset();
            string dataLine;
            var dataset = new List<AdultCensus>();
            using (StreamReader reader = new StreamReader(adultDataset))
            {
                // read the header
                dataLine = reader.ReadLine();
                while ((dataLine = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(dataLine))
                        continue;

                    var data = dataLine.Split(',');
                    dataset.Add(new AdultCensus()
                    {
                        Age = int.Parse(data[0]),
                        Workclass = data[1],
                        Education = data[3],
                        //EducationNum = int.Parse(data[4]),
                        MaritalStatus = data[5],
                        Occupation = data[6],
                        Relationship = data[7],
                        Race = data[8],
                        Sex = data[9],
                        CapitalGain = data[10],
                        CapitalLoss = data[11],
                        HoursPerWeek = int.Parse(data[12]),
                        NativeCountry = data[13],
                        Label = int.Parse(data[14])
                    });
                }
            }

            return dataset;
        }

        public static void PopulateDatabase()
        {
            var adultData = GetAdultData();
            using (var db = new AdultCensusContext())
            {
                foreach(var row in adultData)
                {
                    db.AdultCensus.Add(row);
                }

                var count = db.SaveChanges();
                Console.WriteLine($"Save results {count}");
            }
        }

        public static void Main()
        {
            PopulateDatabase();
            using(var db = new AdultCensusContext())
            {
                var mlContext =  new MLContext();
                var dataView = mlContext.Data.ReadFromEnumerable(db.AdultCensus);
                var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(
                    new OneHotEncodingEstimator.ColumnInfo("MsOHE", "MaritalStatus", OneHotEncodingTransformer.OutputKind.Bin),
                    new OneHotEncodingEstimator.ColumnInfo("OccOHE", "Occupation", OneHotEncodingTransformer.OutputKind.Bin),
                    new OneHotEncodingEstimator.ColumnInfo("RelOHE", "Relationship", OneHotEncodingTransformer.OutputKind.Bin),
                    new OneHotEncodingEstimator.ColumnInfo("SOHE", "Sex", OneHotEncodingTransformer.OutputKind.Bin),
                    new OneHotEncodingEstimator.ColumnInfo("NatOHE", "NativeCountry", OneHotEncodingTransformer.OutputKind.Bin));
                pipeline.Append(mlContext.Transforms.Concatenate("Feature", 
                    "Age", "MsOHE", "OccOHE", "RelOHE", "SOHE", "HoursPerWeek", "NatOHE"));
                pipeline.Append(mlContext.BinaryClassification.Trainers.LightGbm(
                    new Microsoft.ML.LightGBM.Options() { }));

                Console.WriteLine("Training model...");
                var model = pipeline.Fit(dataView);
            }
        }
    }
}