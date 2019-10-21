# Using LoadFromEnumerable and Entity Framework with a relational database as a data source for training and validating a model
This sample demonstrates how to use a database as a data source for an ML.NET pipeline by using an IEnumerable. Since a database is treated as any other datasource, it is possible to query the database and use the resulting data for training and prediction scenarios.

**Update (Sept. 2nd 2019): If you want to load data from a relational database, there's a simpler approach in ML.NET by using the DatabaseLoader. Check the [DatabaseLoader sample](/samples/csharp/getting-started/DatabaseLoader)**. 

Note that you could also implement a similar aproach using **LoadFromEnumerable** but using a **No-SQL** database or any other data source instead a relational database. However, this example is using a relational database being accessed by Entity Framework.

## Problem
Enterprise users have a need to use their existing data set that is in their company's database to train and predict with ML.NET. 

Even when in most cases data needs to be clean-up and prepared before training a machine learning model, many enterprises are very familiar with databases for transforming and preparing data and prefer to have centralized and secured data into database servers instead of working with exported plain text files.

## Out of scope

Note that the process for preparing your dataset is out of scope for this sample. This sample assumes that you already have prepared your dataset as a single table in a relational database. You can also create/use multiple tables when preparing your dataset and specify a join query when obtaining your IEnumerable, however, the less joins you do when querying data while training an ML model, the better performance you will have and the less time you will need to finish your model training processes. That's why a single table is the ideal case for training a ML model.

### Why Data preparation is important

Why can't you simply create a join query against your transational tables? - Even when tecnically you could create the IEnumerable from any join query, in most real-world situations that won't work for the ML algorithms/trainers. 

Data preparation is important because most machine learning trainers/algorithms require data to be formatted in a very specific way or input feature columns to be in very specific data types, so datasets generally require some data preparation before you can really train a model with it. You also need to clean-up data, some data sources might have missing values (null/nan), or invalid values (data might need to be in a different scale, you might need to upsample or normalize numeric values in features, etc.) making the training process to either break of to produce a less accurate result or even misleading.

Therefore, data preparation is needed in almost 100% of the cases before you can train an ML model.

For further information on *'Data preparation for machine learning'* read the following articles:

https://machinelearningmastery.com/how-to-prepare-data-for-machine-learning/

## Solution

This sample shows how to use Entity Framework Core to connect to a database, query and feed the resulting data into an ML.NET pipeline through an IEnumerable.

This sample uses SQLite to help demonstrate the database integration, but any database (such as SQL Server, Oracle, MySQL, PostgreSQL, etc.) that is supported by Entity Framwork Core can be used. As ML.NET can create an IDataView from an IEnumerable, this sample will use the IEnumerable that is returned from a query to feed the data into the ML.NET pipeline. 

## Important considerations and workarounds

1. To prevent the Entity Framework Core from loading all the data in from a result, **a no tracking query is used**. 

2. It is important to highlight that **the IEnumerable you provide needs to be thread-safe**. This example shows you to create an IEnumerable with Entity Framework that won’t cause issues to LoadFromEnumerable() because it makes sure that each new enumeration of the data happens on a separate DbContext and DbConnection by basically creating a database context in your code each time a IEnumerable is requested.

Specifically, the code showing you how to create a database context each time a IEnumerable is requested plus using a 'no tracking query' is here: https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/DatabaseIntegration/DatabaseIntegration/Program.cs#L44

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
