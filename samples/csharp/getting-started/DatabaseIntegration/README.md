# Using a database as a data source
This sample demonstrates how to use a database as a data source for an ML.Net pipeline using the IDataView interface. The data can then be used for training and testing a model. In order
to support mulitple databass the Entity Framework is used as part of this sample.

## Problem

In scenarios where the dataset lives in a database, the dataset is likely large and associated but not live within a single table. In addition, having to massage the data to be consumeable into an machine learning pipeline can be an expensive and tedious chore.


## Solution
This sample shows how to use the Entity Framework to connect to a database and leverage the existing schema allowing to query for the data that is needed to train and make this available to ML.Net through the IDataView interface.

This sample uses SQLite to help demonstrate the database integration, but any database that is supported by the Entity Framwork can be used. This sample also uses ML.Net's ability to convert an IEnumerable to and IDataView -- as the Entity Framework will expose the results of a query as an IEnumberable. To get started the following commands need to run to setup the database. To create the database, the dot net migrations commands are used.


### 1. Create the database
From the command line in the directory where the DatabaseIntegration.csproj file is located, type the following cmomands:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
### 2. Open the solution, build and run the code
The sample will do the following:
- Downloads a sample dataset
- Upload the dataset into the database
- Query the database for the dataset
- Converts the IEnumerable to IDataView
- Trains a LightGBM Binary Classification model 
