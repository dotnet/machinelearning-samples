using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using System;
using System.IO;
using System.Linq;
using static eShopForecastModelsTrainer.ConsoleHelpers;
using Common;

namespace eShopForecastModelsTrainer
{
    public class ProductModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static void TrainAndSaveModel(MLContext mlContext, string dataPath, string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            CreateProductModelUsingPipeline(mlContext, dataPath, outputModelPath);
        }


        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static void CreateProductModelUsingPipeline(MLContext mlContext, string dataPath, string outputModelPath)
        {
            ConsoleWriteHeader("Training product forecasting");

            var textLoader = mlContext.Data.TextReader(new TextLoader.Arguments
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

            var trainer = mlContext.Regression.Trainers.FastTreeTweedie("Label", "Features");

            var trainingPipeline = mlContext.Transforms.Concatenate(outputColumn: "NumFeatures", "year", "month", "units", "avg", "count", "max", "min", "prev" )
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(inputColumn:"productId", outputColumn:"CatFeatures"))
                .Append(mlContext.Transforms.Concatenate(outputColumn: "Features", "NumFeatures", "CatFeatures"))
                .Append(mlContext.Transforms.CopyColumns("next", "Label"))
                .Append(trainer);

            var trainingDataView = textLoader.Read(dataPath);

            // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValidationResults = mlContext.Regression.CrossValidate(trainingDataView, trainingPipeline, numFolds: 6, labelColumn: "Label");
            ConsoleHelper.PrintRegressionFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            var model = trainingPipeline.Fit(trainingDataView);
            
            using (var file = File.OpenWrite(outputModelPath))
                model.SaveTo(mlContext, file);
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        /// <returns></returns>
        public static void TestPrediction(MLContext mlContext, string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            ConsoleWriteHeader("Testing Product Unit Sales Forecast model");

            // Read the model that has been previously saved by the method SaveModel

            ITransformer trainedModel;
            using (var stream = File.OpenRead(outputModelPath))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            var predictionFunct = trainedModel.MakePredictionFunction<ProductData, ProductUnitPrediction>(mlContext);

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
            ProductUnitPrediction prediction = predictionFunct.Predict(dataSample);
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
            prediction = predictionFunct.Predict(dataSample);
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

            prediction = predictionFunct.Predict(dataSample);
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

            prediction = predictionFunct.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecasting (units): {prediction.Score}");
        }
    }
}
