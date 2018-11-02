using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.FastTree;
using Microsoft.ML.Trainers;
using Microsoft.ML.Core.Data;
using System;
using System.IO;
using System.Linq;
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

            var reader = new TextLoader(env, new TextLoader.Arguments
            {
                Column = new[] {
                    new TextLoader.Column("next", DataKind.R4, 0 ),
                    new TextLoader.Column("productId", DataKind.Text, 1 ),
                    new TextLoader.Column("year", DataKind.R4, 2 ),
                    new TextLoader.Column("month", DataKind.R4, 3 ),
                    new TextLoader.Column("units", DataKind.R4, 4 ),
                    new TextLoader.Column("avg", DataKind.R4, 5 ),
                    new TextLoader.Column("count", DataKind.R4, 6 ),
                    new TextLoader.Column("max", DataKind.R4, 7 ),
                    new TextLoader.Column("min", DataKind.R4, 8 ),
                    new TextLoader.Column("prev", DataKind.R4, 9 )
                },
                HasHeader = true,
                Separator = ","
            });


            var pipeline = new ConcatEstimator(env, "NumFeatures", new[] { "year", "month", "units", "avg", "count", "max", "min", "prev" })
                .Append(new CategoricalEstimator(env, "CatFeatures", "productId"))
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
