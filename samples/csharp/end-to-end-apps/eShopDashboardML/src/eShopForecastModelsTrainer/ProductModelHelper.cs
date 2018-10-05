using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.FastTree;
using Microsoft.ML.Trainers;
using Microsoft.ML.Core.Data;
using System;
using System.IO;
using static eShopForecastModelsTrainer.ConsoleHelpers;

namespace eShopForecastModelsTrainer
{
    public class ProductModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static void TrainAndSaveModel(string dataPath, string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            CreateProductModelUsingPipeline(dataPath, outputModelPath);
        }


        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static void CreateProductModelUsingPipeline(string dataPath, string outputModelPath)
        {
            var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
            var ctx = new RegressionContext(env);

            ConsoleWriteHeader("Training product forecasting");

            var reader = TextLoader.CreateReader(env,
                            c => (
                                next: c.LoadFloat(0),
                                productId: c.LoadText(1),
                                year: c.LoadFloat(2),
                                month: c.LoadFloat(3),
                                units: c.LoadFloat(4),
                                avg: c.LoadFloat(5),
                                count: c.LoadFloat(6),
                                max: c.LoadFloat(7),
                                min: c.LoadFloat(8),
                                prev: c.LoadFloat(9)),
                            separator: ',', hasHeader: true);

            var est = reader.MakeNewEstimator()
                .Append(row => (
                    NumFeatures: row.year.ConcatWith(row.month, row.month, row.units, row.avg, row.count, row.max, row.min, row.prev),
                    CatFeatures: row.productId.OneHotEncoding(),
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
        public static void TestPrediction(string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            ConsoleWriteHeader("Testing Product Unit Sales Forecast model");

            // Read the model that has been previously saved by the method SaveModel
            var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
            ITransformer model;
            using (var file = File.OpenRead(outputModelPath))
            {
                model = TransformerChain
                    .LoadFrom(env, file);
            }

            var predictor = model.MakePredictionFunction<ProductData, ProductUnitPrediction>(env);

            Console.WriteLine("** Testing Product 1 **");

            // Build sample data
            ProductData dataSample = new ProductData()
            {
                productId = "263",
                month = 10,
                year = 2017,
                avg = 91,
                max = 370,
                min = 1,
                count = 10,
                prev = 1675,
                units = 910
            };

            //model.Predict() predicts the nextperiod/month forecast to the one provided
            ProductUnitPrediction prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (units): 551, Forecast Prediction (units): {prediction.Score}");

            dataSample = new ProductData()
            {
                productId = "263",
                month = 11,
                year = 2017,
                avg = 29,
                max = 221,
                min = 1,
                count = 35,
                prev = 910,
                units = 551
            };

            //model.Predict() predicts the nextperiod/month forecast to the one provided
            prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecast Prediction (units): {prediction.Score}");

            Console.WriteLine(" ");

            Console.WriteLine("** Testing Product 2 **");

            dataSample = new ProductData()
            {
                productId = "988",
                month = 10,
                year = 2017,
                avg = 43,
                max = 220,
                min = 1,
                count = 25,
                prev = 1036,
                units = 1094
            };

            prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real Value (units): 1076, Forecasting (units): {prediction.Score}");

            dataSample = new ProductData()
            {
                productId = "988",
                month = 11,
                year = 2017,
                avg = 41,
                max = 225,
                min = 4,
                count = 26,
                prev = 1094,
                units = 1076
            };

            prediction = predictor.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecasting (units): {prediction.Score}");
        }
    }
}
