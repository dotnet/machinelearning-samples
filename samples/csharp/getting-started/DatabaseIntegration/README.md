# Using a database as a data source
This sample demonstrates how to use a database as a data source for an ML.Net pipeline. As ML.Net does not have native support for a database, this sample shows how data can be accessed using an IEnumerable. Since the database is treated as any other datasource, it is possible to query the database and use the resulting data for training and prediction scenarios.

## Problem
Enterprise users have a need to use their existing data set that is in their company's database to train and predict with ML.Net. They need support to leverage their existing relational table schema, ability to read from the database directly, and to be aware of memory limitations as the data is being consumed.

## Solution
This sample shows how to use the Entity Framework Core to connect to a database, query  and feed the resulting data into an ML.Net pipeline.

This sample uses SQLite to help demonstrate the database integration, but any database that is supported by the Entity Framwork Core can be used. As ML.Net can  create an IDataView from an IEnumerable, this sample will use the IEnumerable that is returned from a query to feed the data into the ML.Net pipeline. To prevent the Entity Framework Core from loading all the data in from a result, a no tracking query is used. 

To get started the following dot net migration commands need to run to setup the database. 

### 1. Create the database
From the command line in the directory where the DatabaseIntegration.csproj file is located, type the following commands:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
### 2. Open the solution, build and run the code
The sample will do the following:
- Downloads a sample dataset
- Seeds the data into the database
- Query the database for the dataset
- Converts the IEnumerable to IDataView
- Trains a LightGBM Binary Classification model 
- Queries the database for a test dataset
- Runs predictions
- Evaluates the prediction metrics

Note: The working directory will need to be set to the directory where DatabaseIntegration.csproj is located, otherwise the database will not be seeded.


