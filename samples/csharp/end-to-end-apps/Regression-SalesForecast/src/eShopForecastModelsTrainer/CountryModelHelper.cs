using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using static eShopForecastModelsTrainer.ConsoleHelpers;
using Common;
using Microsoft.ML.Data;

namespace eShopForecastModelsTrainer
{
    public class CountryModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static void TrainAndSaveModel(MLContext mlContext, string dataPath, string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            CreateCountryModel(mlContext, dataPath, outputModelPath);
        }

        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static void CreateCountryModel(MLContext mlContext, string dataPath, string outputModelPath)
        {
            ConsoleWriteHeader("Training country forecasting model");

            var trainingDataView = mlContext.Data.LoadFromTextFile<CountryData>(path:dataPath, hasHeader: true, separatorChar: ',');
            
            var trainer = mlContext.Regression.Trainers.FastTreeTweedie(DefaultColumnNames.Label, DefaultColumnNames.Features);

            var trainingPipeline = mlContext.Transforms.Concatenate(outputColumnName: "NumFeatures", nameof(CountryData.year),
                                nameof(CountryData.month), nameof(CountryData.max), nameof(CountryData.min),
                                nameof(CountryData.std), nameof(CountryData.count), nameof(CountryData.sales),
                                nameof(CountryData.med), nameof(CountryData.prev))
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeatures", inputColumnName: nameof(CountryData.country)))
                        .Append(mlContext.Transforms.Concatenate(outputColumnName: DefaultColumnNames.Features, "NumFeatures", "CatFeatures"))
                        .Append(mlContext.Transforms.CopyColumns(outputColumnName: DefaultColumnNames.Label, inputColumnName: nameof(CountryData.next)))
                        .Append(trainer);

            // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValidationResults = mlContext.Regression.CrossValidate(data:trainingDataView, estimator:trainingPipeline, numFolds: 6, labelColumn: DefaultColumnNames.Label);
            ConsoleHelper.PrintRegressionFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            // Create and Train the model
            var model = trainingPipeline.Fit(trainingDataView);

            using (var file = File.OpenWrite(outputModelPath))
                model.SaveTo(mlContext, file);
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        /// <returns></returns>
        public static void TestPrediction(MLContext mlContext, string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            ConsoleWriteHeader("Testing Country Sales Forecast model");

            ITransformer trainedModel;
            using (var stream = File.OpenRead(outputModelPath))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            //ITransformer trainedModel;
            //using (var file = File.OpenRead(outputModelPath))
            //{
            //    trainedModel = TransformerChain
            //        .LoadFrom(mlContext, file);
            //}

            var predictionEngine = trainedModel.CreatePredictionEngine<CountryData, CountrySalesPrediction>(mlContext);

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
            var prediction = predictionEngine.Predict(dataSample);
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
            prediction = predictionEngine.Predict(dataSample);
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
            prediction = predictionEngine.Predict(dataSample);
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
            prediction = predictionEngine.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Predicted Forecast (US$):  {Math.Pow(prediction.Score, 10)}");
        }
    }
}
