
# Sample using DatabaseLoader for training an ML model directly against data in a SQL Server database (Or any relational database)

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.16-Preview          | Dynamic API | up-to-date | Console app | SQL Server database or any relational database | IDataView from DB | Any | Any |

![](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2019/08/database-loader-illustration-300x181.png)

This sample shows you how you can use the native database loader ro directly train an ML model against relational databases. This loader supports any relational database provider supported by System.Data in .NET Core or .NET Framework, meaning that you can use any RDBMS such as SQL Server, Azure SQL Database, Oracle, SQLite, PostgreSQL, MySQL, Progress, IBM DB2, etc.

## Problem

In the enterprise and many organizations in general, data is organized and stored as relational databases to be used by enterprise applications. Many of those organizations also prepare their ML model training/evaluation data in relational databases which is also where the new data is being collected and prepared. Therefore, many of those users would also like to directly train/evaluate ML models directly agaist that data stored in relational databases.  

## Background

In previous [ML.NET](https://dot.net/ml) releases, since [ML.NET](https://dot.net/ml) 1.0, you could also train against a relational database by providing data through an IEnumerable collection by using the [LoadFromEnumerable()](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.dataoperationscatalog.loadfromenumerable?view=ml-dotnet) API where the data could be coming from a relational database or any other source. However, when using that approach, you as a developer are responsible for the code reading from the relational database (such as using Entity Framework or any other approach) which needs to be implemented properly so you are streaming data while training the ML model, as in this [previous sample using LoadFromEnumerable()](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/DatabaseIntegration).

## Solution

This new Database Loader provides a much simpler code implementation for you since the way it reads from the database and makes data available through the IDataView is provided out-of-the-box by the [ML.NET](https://dot.net/ml) framework so you just need to specify your database connection string, what’s the SQL statement for the dataset columns and what’s the data-class to use when loading the data. It is that simple!

Here’s example code on how easily you can now configure your code to load data directly from a relational database into an IDataView which will be used later on when training your model.

```cs --source-file ./DatabaseLoaderConsoleApp/Program.cs --project ./SentimentAnalysis/SentimentAnalysisConsoleApp/SentimentAnalysisConsoleApp.csproj --editable false  --region step1to3

var mlContext = new MLContext();

// The following is a connection string using a localdb SQL database, 
// but you can also use connection strings against on-premises SQL Server, Azure SQL Database 
// or any other relational database (Oracle, SQLite, PostgreSQL, MySQL, Progress, IBM DB2, etc.)

// localdb SQL database connection string using a filepath to attach the database file into localdb
string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlLocalDb", "Criteo-100k-rows.mdf");
string connectionString = $"Data Source = (LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFilePath};Database=Criteo-100k-rows;Integrated Security = True";

string commandText = "SELECT * from URLClicks";

DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<UrlClick>();
            
DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, 
                                                connectionString, 
                                                commandText);
            
IDataView dataView = loader.Load(dbSource);

// From this point you can use the IDataView for training and validating an ML.NET model as in any other sample
```

Check the rest of the sample training and evaluating an ML.NET model in the **program.cs** file.

