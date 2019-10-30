# Bike Sharing Demand - Forecasting

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4.0-preview2 | Dynamic API | Up-to-date | Console app | SQL Server | Demand prediction | Forecasting | Single Spectrum Analysis |

In this sample, you can see how to load data from a relational database using the Database Loader to train a forecasting model that predicts bike rental demand. 

## Problem

For a more detailed descritpion of the problem, read the details from the original [
Bike Sharing Demand competition from Kaggle](https://www.kaggle.com/c/bike-sharing-demand).

## DataSet

The data used in this sample comes from the [UCI Bike Sharing Dataset](https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset). Fanaee-T, Hadi, and Gama, Joao, 'Event labeling combining ensemble detectors and background knowledge', Progress in Artificial Intelligence (2013): pp. 1-15, Springer Berlin Heidelberg, [Web Link](https://link.springer.com/article/10.1007%2Fs13748-013-0040-3).

The original dataset contains several columns corresponding to seasonality and weather. For brevity and because the technique used in this sample only requires the values from a single numerical column, the original dataset has been enhanced to include only the following columns:  

- **dteday**: The date of the observation.
- **year**: The encoded year of the observation (0=2011, 1=2012).
- **cnt**: The total number of bike rentals for that day.

The original dataset is mapped to a database table with the following schema in a SQL Server database.

```sql
CREATE TABLE [Rentals] (
	[RentalDate] DATE NOT NULL,
	[Year] INT NOT NULL,
	[TotalRentals] INT NOT NULL
);
```

The following is a sample of the data:

| RentalDate | Year | TotalRentals |
| --- | --- | --- |
|1/1/2011|0|985|
|1/2/2011|0|801|
|1/3/2011|0|1349|

## Database Loader

Database Loader provides a simple API to read data from relational databases directly into an `IDataView`. This loader supports any relational database provider supported by System.Data in .NET Core or .NET Framework, meaning that you can use any RDBMS such as SQL Server, Azure SQL Database, Oracle, SQLite, PostgreSQL, MySQL, Progress, IBM DB2, etc.

To load data, you need to provide a connection string and a SQL command to get data from the database.

```csharp
string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
string dbFilePath = Path.Combine(rootDir, "Data", "DailyDemand.mdf");
var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30;";

MLContext mlContext = new MLContext();
DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<ModelInput>();

string query = "SELECT RentalDate, CAST(Year as REAL) as Year, CAST(TotalRentals as REAL) as TotalRentals FROM Rentals;";

DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance,
                                            connectionString,
                                            query);

IDataView dataView = loader.Load(dbSource)
```

## ML task - [Regression](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#regression)

The ML Task for this sample is forecasting, which is a supervised machine learning task that is used to predict the value of the label (in this case the demand units prediction) from previous data.

## Solution

To solve this problem, you build and train an ML model on existing training data, evaluate how good it is (analyzing the obtained metrics), and lastly you can consume/test the model to predict the demand given input data variables.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

## Training pipeline

A time series training pipeline can be defined by using `ForecastBySsa` transform.

```csharp
var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
	outputColumnName: "ForecastedRentals",
	inputColumnName: "TotalRentals",
	windowSize: 7,
	seriesLength: 30,
	trainSize: 365,
	horizon: 7,
	confidenceLevel: 0.95f,
	confidenceLowerBoundColumn: "LowerBoundRentals",
	confidenceUpperBoundColumn: "UpperBoundRentals");
```

The `forecastingPipeline` takes 365 data points for the first year and samples or splits the time series dataset into 30-day (monthly) intervals as specified by the `seriesLength` parameter. Each of these samples is analyzed through weekly or 7-day window. When determining what the forecasted value for the next period(s) is, the values from previous seven days are used to make a prediction. The model is set to forecast seven periods into the future as defined by the `horizon` parameter. Because a forecast is an informed guess, it's not always 100% accurate. Therefore, it's good to know the range of values in the best and worst-case scenarios as defined by the upper and lower bounds. In this case, the level of confidence for the lower and upper bounds is set to 95%. The confidence level can be increased or decreased accordingly. The higher the value, the wider the range is between the upper and lower bounds to achieve the desired level of confidence.

Then, to train the model, use the `Fit` method.

```csharp
SsaForecastingTransformer forecaster = forecastingPipeline.Fit(firstYearData);
```

## Evaluate the model

To evaluate the model, compare use the `Transform` method to forecast future values. Then, compare them against the actual values and calculate metrics like *Mean Absolute Error* and *Root Mean Squared Error*.

```csharp
static void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
{
	// Make predictions
	IDataView predictions = model.Transform(testData);

	// Actual values
	IEnumerable<float> actual =
		mlContext.Data.CreateEnumerable<ModelInput>(testData, true)
			.Select(observed => observed.TotalRentals);

	// Predicted values
	IEnumerable<float> forecast =
		mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true)
			.Select(prediction => prediction.ForecastedRentals[0]);

	// Calculate error (actual - forecast)
	var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

	// Get metric averages
	var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
	var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

	// Output metrics
	Console.WriteLine("Evaluation Metrics");
	Console.WriteLine("---------------------");
	Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
	Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");
}
```

**Mean Absolute Error**: Measures how close predictions are to the actual value. This value ranges between 0 and infinity. The closer to 0, the better the quality of the model.

**Root Mean Squared Error**: Summarizes the error in the model. This value ranges between 0 and infinity. The closer to 0, the better the quality of the model.

## Forecasting values

To forecast values, create a `TimeSeriesPredictionEngine`, a convenience API to make single predictions.

```csharp
var forecastEngine = forecaster.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
```

Then, use the `Predict` method to generate a single forecast for the number of periods specified by the `horizon`.

```csharp
static void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster, MLContext mlContext)
{
	ModelOutput forecast = forecaster.Predict();

	//... additional code
}
```

## Sample Output

When you run the application, you should see output similar to the following:

```text
Evaluation Metrics
---------------------
Mean Absolute Error: 726.416
Root Mean Squared Error: 987.658

Rental Forecast
---------------------
Date: 1/1/2012
Actual Rentals: 2294
Lower Estimate: 1197.842
Forecast: 2334.443
Upper Estimate: 3471.044

Date: 1/2/2012
Actual Rentals: 1951
Lower Estimate: 1148.412
Forecast: 2360.861
Upper Estimate: 3573.309

Date: 1/3/2012
Actual Rentals: 2236
Lower Estimate: 1068.507
Forecast: 2373.277
Upper Estimate: 3678.046
```
