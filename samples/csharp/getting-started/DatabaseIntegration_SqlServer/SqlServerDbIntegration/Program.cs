using Common;
using DatabaseIntegration;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SqlServerDbIntegration
{
    public class Program
    {  
        public static void Main()
        {
            var mlContext = new MLContext(seed: 1);            

            var dataView = mlContext.Data.LoadFromEnumerable(QueryData());

            var trainTestData = mlContext.Data.TrainTestSplit(dataView);

            var train100RowsEnumerable = mlContext.Data.CreateEnumerable<UrlClicks>(trainTestData.TrainSet, reuseRowObject: false).Take(100).ToList();

            var train100RowsDataView = mlContext.Data.LoadFromEnumerable(train100RowsEnumerable);

            //do the transformation in IDataView
            //Transform categorical features into binary
            var CatogoriesTranformer = mlContext.Transforms.Conversion.ConvertType(nameof(UrlClicks.Label), outputKind:Microsoft.ML.Data.DataKind.Boolean).
                Append(mlContext.Transforms.Categorical.OneHotEncoding(new[] {
                new InputOutputColumnPair("Cat14Encoded", "Cat14"),
                new InputOutputColumnPair("Cat15Encoded", "Cat15"),
                new InputOutputColumnPair("Cat16Encoded", "Cat16"),
                new InputOutputColumnPair("Cat17Encoded", "Cat17"),
                new InputOutputColumnPair("Cat18Encoded", "Cat18"),
                new InputOutputColumnPair("Cat19Encoded", "Cat19"),
                new InputOutputColumnPair("Cat20Encoded", "Cat20"),
                new InputOutputColumnPair("Cat21Encoded", "Cat21"),
                new InputOutputColumnPair("Cat22Encoded", "Cat22"),
                new InputOutputColumnPair("Cat23Encoded", "Cat23"),
                new InputOutputColumnPair("Cat24Encoded", "Cat24"),
                new InputOutputColumnPair("Cat25Encoded", "Cat25"),
                new InputOutputColumnPair("Cat26Encoded", "Cat26"),
                new InputOutputColumnPair("Cat27Encoded", "Cat27"),
                new InputOutputColumnPair("Cat28Encoded", "Cat28"),
                new InputOutputColumnPair("Cat29Encoded", "Cat29"),
                new InputOutputColumnPair("Cat30Encoded", "Cat30"),
                new InputOutputColumnPair("Cat31Encoded", "Cat31"),
                new InputOutputColumnPair("Cat32Encoded", "Cat32"),
                new InputOutputColumnPair("Cat33Encoded", "Cat33"),
                new InputOutputColumnPair("Cat34Encoded", "Cat34"),
                new InputOutputColumnPair("Cat35Encoded", "Cat35"),
                new InputOutputColumnPair("Cat36Encoded", "Cat36"),
                new InputOutputColumnPair("Cat37Encoded", "Cat37"),
                new InputOutputColumnPair("Cat38Encoded", "Cat38"),
                new InputOutputColumnPair("Cat391Encoded", "Cat391")
            }, OneHotEncodingEstimator.OutputKind.Binary));
            
            var featuresTransformer = CatogoriesTranformer.Append(
                mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat01Featurized", inputColumnName: nameof(UrlClicks.Feat01)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat02Featurized", inputColumnName: nameof(UrlClicks.Feat02)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat03Featurized", inputColumnName: nameof(UrlClicks.Feat03)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat04Featurized", inputColumnName: nameof(UrlClicks.Feat04)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat05Featurized", inputColumnName: nameof(UrlClicks.Feat05)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat06Featurized", inputColumnName: nameof(UrlClicks.Feat06)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat07Featurized", inputColumnName: nameof(UrlClicks.Feat07)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat08Featurized", inputColumnName: nameof(UrlClicks.Feat08)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat09Featurized", inputColumnName: nameof(UrlClicks.Feat09)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat10Featurized", inputColumnName: nameof(UrlClicks.Feat10)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat11Featurized", inputColumnName: nameof(UrlClicks.Feat11)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat12Featurized", inputColumnName: nameof(UrlClicks.Feat12)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat13Featurized", inputColumnName: nameof(UrlClicks.Feat13)));

            var finalTransformerPipeLine = featuresTransformer.Append(mlContext.Transforms.Concatenate("Features",
                            "Feat01Featurized",
                            "Feat02Featurized",
                            "Feat03Featurized",
                            "Feat04Featurized",
                            "Feat05Featurized",
                            "Feat06Featurized",
                            "Feat07Featurized",
                            "Feat08Featurized",
                            "Feat09Featurized",
                            "Feat10Featurized",
                            "Feat11Featurized",
                            "Feat12Featurized",
                            "Feat12Featurized",
                            "Cat14Encoded", "Cat15Encoded", "Cat16Encoded", "Cat17Encoded", "Cat18Encoded", "Cat19Encoded",
                            "Cat20Encoded", "Cat21Encoded", "Cat22Encoded", "Cat23Encoded", "Cat24Encoded", "Cat25Encoded",
                            "Cat26Encoded", "Cat27Encoded", "Cat28Encoded", "Cat29Encoded", "Cat30Encoded", "Cat31Encoded",
                            "Cat32Encoded", "Cat33Encoded", "Cat34Encoded", "Cat35Encoded", "Cat36Encoded", "Cat37Encoded",
                            "Cat38Encoded", "Cat391Encoded"));

            ConsoleHelper.PeekDataViewInConsole(mlContext, train100RowsDataView, finalTransformerPipeLine, 2);

            var trainingPipeLine = finalTransformerPipeLine.Append(mlContext.BinaryClassification.Trainers.LightGbm(labelColumnName: "Label", featureColumnName: "Features"));
            
            Console.WriteLine("Training model...");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var model = trainingPipeLine.Fit(trainTestData.TrainSet);            

            watch.Stop();
            Console.WriteLine("elapsed time for training the model = {0}", watch.ElapsedMilliseconds);

            Console.WriteLine("Evaluating the model...");
            var predictions = model.Transform(trainTestData.TestSet);
            watch.Start();

            // Now that we have the predictions, calculate the metrics of those predictions and output the results.
            var metrics = mlContext.BinaryClassification.Evaluate(predictions);
            watch.Stop();
            Console.WriteLine("elapsed time for evaluating the model = {0}", watch.ElapsedMilliseconds);
            ConsoleHelper.PrintBinaryClassificationMetrics("====Evaluation Metrics for Large datasets stored in Database====", metrics);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }

        private static IEnumerable<UrlClicks> QueryData()
        {
            using (var urlClickContext = new UrlClickContext())
            {
                // Query our training data from the database. This query is selecting everything from the AdultCensus table. The
                // result is then loaded by ML.Net through the LoadFromEnumerable. LoadFromEnumerable returns an IDataView which
                // can be consumed by an ML.Net pipeline.
                // NOTE: For training, ML.Net requires that the training data is processed in the same order to produce consistent results.
                // Therefore we are sorting the data by the AdultCensusId, which is an auto-generated id.
                // NOTE: That the query used here sets the query tracking behavior to be NoTracking, this is particularly useful because
                // our scenarios only require read-only access.
                foreach (var urlClickRecord in urlClickContext.urlClicks.AsNoTracking().OrderBy(x => x.UrlClickId))
                {
                    yield return urlClickRecord;
                }
            }
        }       
    }
}