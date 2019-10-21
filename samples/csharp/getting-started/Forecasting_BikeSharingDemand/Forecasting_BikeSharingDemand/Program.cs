using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

namespace Forecasting_BikeSharingDemand
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DailyDemand.mdf");
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30";

            MLContext mlContext = new MLContext();

            DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<ModelInput>();

            string query = "SELECT RentalDate, CAST(Year as REAL) as Year, CAST(TotalRentals as REAL) as TotalRentals FROM Rentals";

            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance,
                                            connectionString,
                                            query);

            IDataView dataView = loader.Load(dbSource);

            IDataView firstYearData = mlContext.Data.FilterRowsByColumn(dataView, "Year", upperBound: 1);
            IDataView secondYearData = mlContext.Data.FilterRowsByColumn(dataView, "Year", lowerBound: 1);

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedDemand",
                inputColumnName: "TotalRentals",
                windowSize: 7,
                seriesLength: 30,
                trainSize: 365,
                horizon: 7,
                confidenceLevel: 0.90f,
                confidenceLowerBoundColumn: "ConfidenceLowerBound",
                confidenceUpperBoundColumn: "ConfidenceUpperBound");

            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(firstYearData);

            Forecast(secondYearData, 7, forecaster, mlContext);

            Console.ReadKey();
        }

        private static void Forecast(IDataView testData, int horizon, ITransformer model, MLContext mlContext)
        {
            TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster =
                model.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);

            ModelOutput forecast = forecaster.Predict();

            IEnumerable<string> forecastOutput =
                mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
                    .Take(horizon)
                    .Select((ModelInput rental, int index) =>
                    {
                        string rentalDate = rental.RentalDate.ToShortDateString();
                        float actualRentals = rental.TotalRentals;
                        float lowerEstimate = Math.Max(0, forecast.ConfidenceLowerBound[index]);
                        float estimate = forecast.ForecastedDemand[index];
                        float upperEstimate = forecast.ConfidenceUpperBound[index];
                        return $"Date: {rentalDate}\n" +
                        $"Actual Rentals: {actualRentals}\n" +
                        $"Lower Estimate: {lowerEstimate}\n" +
                        $"Forecast: {estimate}\n" +
                        $"Upper Estimate: {upperEstimate}\n";
                    });

            // Output predictions
            Console.WriteLine("Rental Forecast");
            Console.WriteLine("---------------------\n");
            foreach (var prediction in forecastOutput)
            {
                Console.WriteLine(prediction);
                Console.WriteLine("---------------------\n");
            }
        }
    }

    public class ModelInput
    {
        public DateTime RentalDate { get; set; }

        public float Year { get; set; }

        public float TotalRentals { get; set; }
    }

    public class ModelOutput
    {
        public float[] ForecastedDemand { get; set; }

        public float[] ConfidenceLowerBound { get; set; }

        public float[] ConfidenceUpperBound { get; set; }
    }
}
