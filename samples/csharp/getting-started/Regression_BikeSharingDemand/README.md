# Bike Sharing Demand - Regression problem sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7-Preview           | Dynamic API | **Evolving** | Console app | .csv files | Demand prediction | Regression | Fast Tree regressor |


**Note**: This sample is being evolving and currently needs to use ML.NET v0.7 (currently in Preview) so you need to use the latest NuGet preview (nightly build) package available at MyGet instead of NuGet.
The MyGet feed URL you need to configure in Visual Studio is the following:
https://dotnet.myget.org/F/dotnet-core/api/v3/index.json

## DataSet
The original data comes from a public UCI dataset:
https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset

## Problem

This problem is centered around predicting the Bike Sharing Demand based on historic data from the previous DataSet.


## ML task - Regression

The ML Task is Regression, however, this example trains multiple models each one based on a different learner/algorithm and finally evaluates the accuracy of each approach, so you can choose the trained model with better accuracy.
The following list are the trainers/algorithms used and compared:

- Fast Tree
- SDCA (Stochastic Dual Coordinate Ascent) Regressor
- Poisson Regressor


