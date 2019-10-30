# Bike Sharing Demand - Forecasting

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4.0-preview2 | Dynamic API | Up-to-date | Console app | SQL Server | Demand prediction | Forecasting | Single Spectrum Analysis |

In this sample, you can see how to load data from a relational database using the Database Loader to train a forecasting model that predicts bike rental demand. 

## Problem

For a more detailed descritpion of the problem, read the details from the original [
Bike Sharing Demand competition from Kaggle](https://www.kaggle.com/c/bike-sharing-demand).

## DataSet

The data used in this tutorial comes from the [UCI Bike Sharing Dataset](https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset). Fanaee-T, Hadi, and Gama, Joao, 'Event labeling combining ensemble detectors and background knowledge', Progress in Artificial Intelligence (2013): pp. 1-15, Springer Berlin Heidelberg, [Web Link](https://link.springer.com/article/10.1007%2Fs13748-013-0040-3).

The original dataset contains several columns corresponding to seasonality and weather. For brevity and because the technique used in this tutorial only requires the values from a single numerical column, the original dataset has been enhanced to include only the following columns:  

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
