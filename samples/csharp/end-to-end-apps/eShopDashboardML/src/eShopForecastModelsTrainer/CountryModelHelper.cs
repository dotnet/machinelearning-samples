using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.FastTree;
using Microsoft.ML.Trainers;
using Microsoft.ML.Core.Data;
using static eShopForecastModelsTrainer.ConsoleHelpers;

namespace eShopForecastModelsTrainer
{
    public class CountryModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static void TrainAndSaveModel(string dataPath, string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            CreateCountryModel(dataPath, outputModelPath);
        }

        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static void CreateCountryModel(string dataPath, string outputModelPath)
        {
            var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
            var ctx = new RegressionContext(env);

            ConsoleWriteHeader("Training country forecasting model");

            var reader = new TextLoader(env, new TextLoader.Arguments
            {
                Column = new[] {
                    new TextLoader.Column("next", DataKind.R4, 0 ),
                    new TextLoader.Column("country", DataKind.Text, 1 ),
                    new TextLoader.Column("year", DataKind.R4, 2 ),
                    new TextLoader.Column("month", DataKind.R4, 3 ),
                    new TextLoader.Column("max", DataKind.R4, 4 ),
                    new TextLoader.Column("min", DataKind.R4, 5 ),
                    new TextLoader.Column("std", DataKind.R4, 6 ),
                    new TextLoader.Column("count", DataKind.R4, 7 ),
                    new TextLoader.Column("sales", DataKind.R4, 8 ),
                    new TextLoader.Column("med", DataKind.R4, 9 ),
                    new TextLoader.Column("prev", DataKind.R4, 10 )
                },
                HasHeader = true,
                Separator = ","
            });
            
            var pipeline = new ConcatEstimator(env, "NumFeatures", new[] { "year", "month", "max", "min", "std", "count", "sales", "med", "prev" })
                .Append(new CategoricalEstimator(env, "CatFeatures", "country"))
                .Append(new ConcatEstimator(env, "Features", new[] { "NumFeatures", "CatFeatures" }))
                .Append(new CopyColumnsEstimator(env, "next", "Label"))
                .Append(new FastTreeTweedieTrainer(env, "Label", "Features"));

            var datasource = reader.Read(new MultiFileSource(dataPath));

            var cvResults = ctx.CrossValidate(datasource, pipeline, labelColumn: "Label", numFolds: 5);

            var L1 = cvResults.Select(r => r.metrics.L1);
            var L2 = cvResults.Select(r => r.metrics.L2);
            var RMS = cvResults.Select(r => r.metrics.L1);
            var lossFunction = cvResults.Select(r => r.metrics.LossFn);
            var R2 = cvResults.Select(r => r.metrics.RSquared);

            var model = pipeline.Fit(datasource);

            Console.WriteLine("Average L1 Loss: " + L1.Average());
            Console.WriteLine("Average L2 Loss: " + L2.Average());
            Console.WriteLine("Average RMS: " + RMS.Average());
            Console.WriteLine("Average Loss Function: " + lossFunction.Average());
            Console.WriteLine("Average R-squared: " + R2.Average());

            using (var file = File.OpenWrite(outputModelPath))
                model.SaveTo(env, file);
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        /// <returns></returns>
        public static void TestPrediction(string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            ConsoleWriteHeader("Testing Country Sales Forecast model");

            var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
            ITransformer model;
            using (var file = File.OpenRead(outputModelPath))
            {
                model = TransformerChain
                    .LoadFrom(env, file);
            }

            var predictor = model.MakePredictionFunction<CountryData, CountrySalesPrediction>(env);

            Console.WriteLine("** Testing Country 1 **");

            // Build sample data
            var dataSample = new CountryData()
            {
                country = "United Kingdom",
                month = 10,
                year = 2017,
                med = 309.945F,
                max = 587.902F,
                min = 135.640F,
                std = 1063.932092F,
                prev = 856548.78F,
                count = 1724,
                sales = 873612.9F,
            };
            // Predict sample data
            var prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Real value (US$): {Math.Pow(6.0084501F, 10)}, Predicted Forecast (US$): {Math.Pow(prediction.Score, 10)}");

            dataSample = new CountryData()
            {
                country = "United Kingdom",
                month = 11,
                year = 2017,
                med = 288.72F,
                max = 501.488F,
                min = 134.5360F,
                std = 707.5642F,
                prev = 873612.9F,
                count = 2387,
                sales = 1019647.67F,
            };
            prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Predicted Forecast (US$):  {Math.Pow(prediction.Score, 10)}");

            Console.WriteLine(" ");

            Console.WriteLine("** Testing Country 2 **");
            dataSample = new CountryData()
            {
                country = "United States",
                month = 10,
                year = 2017,
                med = 400.17F,
                max = 573.63F,
                min = 340.395F,
                std = 340.3959F,
                prev = 4264.94F,
                count = 10,
                sales = 5322.56F
            };
            prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Real value (US$): {Math.Pow(3.805769F, 10)}, Predicted Forecast (US$): {Math.Pow(prediction.Score, 10)}");

            dataSample = new CountryData()
            {
                country = "United States",
                month = 11,
                year = 2017,
                med = 317.9F,
                max = 1135.99F,
                min = 249.44F,
                std = 409.75528F,
                prev = 5322.56F,
                count = 11,
                sales = 6393.96F,
            };
            prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Predicted Forecast (US$):  {Math.Pow(prediction.Score, 10)}");
        }
    }
}
