using System;
using System.IO;
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

            var reader = TextLoader.CreateReader(env,
                            c => (
                                next: c.LoadFloat(0),
                                country: c.LoadText(1),
                                year: c.LoadFloat(2),
                                month: c.LoadFloat(3),
                                max: c.LoadFloat(4),
                                min: c.LoadFloat(5),
                                std: c.LoadFloat(6),
                                count: c.LoadFloat(7),
                                sales: c.LoadFloat(8),
                                med: c.LoadFloat(9),
                                prev: c.LoadFloat(10)),
                            separator: ',', hasHeader: true);

            var est = reader.MakeNewEstimator()
                .Append(row => (
                    NumFeatures: row.year.ConcatWith(row.month, row.max, row.min, row.std, row.count, row.sales, row.med, row.prev),
                    CatFeatures: row.country.OneHotEncoding(),
                    Label: row.next))
                .Append(row => (
                    Features: row.NumFeatures.ConcatWith(row.CatFeatures),
                    row.Label))
                .Append(r => (r.Label, score: ctx.Trainers.FastTree(r.Label, r.Features)));

            var datasource = reader.Read(new MultiFileSource(dataPath));
            var model = est.Fit(datasource);

            using (var file = File.OpenWrite(outputModelPath))
                model.AsDynamic.SaveTo(env, file);
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
