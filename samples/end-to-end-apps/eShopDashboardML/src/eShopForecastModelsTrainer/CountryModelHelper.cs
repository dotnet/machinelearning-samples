using ML = Microsoft.ML;
//using Microsoft.ML.Runtime;
//using Microsoft.ML.Runtime.Data;
//using Microsoft.ML.Runtime.EntryPoints;
using Microsoft.ML.Data;
using System;
using System.IO;
using Microsoft.ML;
using System.Threading.Tasks;

namespace eShopForecastModelsTrainer
{
    public class CountryModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static async Task TrainAndSaveModel(string dataPath, string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            var model = CreateCountryModelUsingPipeline(dataPath);

            await model.WriteAsync(outputModelPath);
        }

        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static PredictionModel<CountryData, CountrySalesPrediction> 
                         CreateCountryModelUsingPipeline(string dataPath)
        {
            Console.WriteLine("*************************************************");
            Console.WriteLine("Training country forecasting model using Pipeline");

            var learningPipeline = new LearningPipeline();

            // First node in the workflow will be reading the source csv file, following the schema defined by dataSchema
            learningPipeline.Add(new TextLoader(dataPath).CreateFrom<CountryData>(useHeader: true, separator: ','));

            // The model needs the columns to be arranged into a single column of numeric type
            // First, we group all numeric columns into a single array named NumericalFeatures
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(
                outputColumn: "NumericalFeatures",
                nameof(CountryData.year),
                nameof(CountryData.month),
                nameof(CountryData.max),
                nameof(CountryData.min),
                nameof(CountryData.std),
                nameof(CountryData.count),
                nameof(CountryData.sales),
                nameof(CountryData.med),
                nameof(CountryData.prev)
            ));

            // Second group is for categorical features (just one in this case), we name this column CategoryFeatures
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(outputColumn: "CategoryFeatures", nameof(CountryData.country)));

            // Then we need to transform the category column using one-hot encoding. This will return a numeric array
            learningPipeline.Add(new ML.Transforms.CategoricalOneHotVectorizer("CategoryFeatures"));

            // Once all columns are numeric types, all columns will be combined
            // into a single column, named Features 
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(outputColumn: "Features", "NumericalFeatures", "CategoryFeatures"));

            // Add the Learner to the pipeline. The Learner is the machine learning algorithm used to train a model
            // In this case, FastTreeTweedieRegressor was one of the best performing algorithms, but you can 
            // choose any other regression algorithm (StochasticDualCoordinateAscentRegressor,PoissonRegressor,...)
            learningPipeline.Add(new ML.Trainers.FastTreeTweedieRegressor { NumThreads = 1, FeatureColumn = "Features" });

            // Finally, we train the pipeline using the training dataset set at the first stage
            var model = learningPipeline.Train<CountryData, CountrySalesPrediction>();

            return model;
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        /// <returns></returns>
        public static async Task TestPrediction(string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            Console.WriteLine(" ");
            Console.WriteLine("*********************************");
            Console.WriteLine("Testing Country Sales Forecast model");

            // Read the model that has been previously saved by the method SaveModel
            var model = await PredictionModel.ReadAsync<CountryData, CountrySalesPrediction>(outputModelPath);


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
            var prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Real value (US$): {Math.Pow(6.0084501F,10)}, Predicted Forecast (US$): {Math.Pow(prediction.Score,10)}");

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
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Predicted Forecast (US$):  {Math.Pow(prediction.Score,10)}");

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
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Real value (US$): {Math.Pow(3.805769F,10)}, Predicted Forecast (US$): {Math.Pow(prediction.Score,10)}");

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
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month to predict: {dataSample.month + 1}, year: {dataSample.year} - Predicted Forecast (US$):  {Math.Pow(prediction.Score,10)}");
        }
    }
}
