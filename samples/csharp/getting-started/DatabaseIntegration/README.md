# Using a relational database as a data source for training and validating a model
This sample demonstrates how to use a database as a data source for an ML.NET pipeline by using an IEnumerable. Since the database is treated as any other datasource, it is possible to query the database and use the resulting data for training and prediction scenarios.

## Problem
Enterprise users have a need to use their existing data set that is in their company's database to train and predict with ML.NET. 

Even when in most cases data needs to be clean-up and prepared before training a machine learning model, many enterprises are more familiar with relational databases and SQL statements for transforming and preparing data and prefer to have centralized and secured data into database servers instead of working with exported plain text files.

## Solution

This sample shows how to use Entity Framework Core to connect to a database, query and feed the resulting data into an ML.NET pipeline through an IEnumerable.

This sample uses SQLite to help demonstrate the database integration, but any database (such as SQL Server, Oracle, MySQL, PostgreSQL, etc.) that is supported by Entity Framwork Core can be used. As ML.NET can create an IDataView from an IEnumerable, this sample will use the IEnumerable that is returned from a query to feed the data into the ML.NET pipeline. 

## Important considerations and workarounds

1. To prevent the Entity Framework Core from loading all the data in from a result, **a no tracking query is used**. 

2. It is important to highlight that **the IEnumerable you provide needs to be thread-safe**. This example shows you to create an IEnumerable with Entity Framework that won’t cause issues to LoadFromEnumerable() because it makes sure that each new enumeration of the data happens on a separate DbContext and DbConnection by basically creating a database context in your code each time a IEnumerable is requested.

Specifically this code showing how to create a database context each time a IEnumerable is requested plus using a 'no tracking query' is here: https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/DatabaseIntegration/DatabaseIntegration/Program.cs#L44

## High level process performed by this sample

The sample implements the following:

- Downloads a sample dataset
- Creates and populates the database
- Query database for the dataset
- Converts the IEnumerable to IDataView
- Trains a LightGBM Binary Classification model 
- Queries the database for a test dataset
- Runs predictions
- Evaluates the prediction metrics
