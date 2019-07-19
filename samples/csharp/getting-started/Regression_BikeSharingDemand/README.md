# Bike Sharing Demand - Regression problem sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.1.0 | Dynamic API | Up-to-date | Console app | .csv files | Demand prediction | Regression | Fast Tree regressor compared to additional regression algorithms|

In this sample, you can see how to use ML.NET to predict the demand of bikes. Since you are trying to predict specific numeric values based on past observed data, in machine learning this type of method for prediction is known as regression.

## Problem

For a more detailed descritpion of the problem, read the details from the original [
Bike Sharing Demand competition from Kaggle](https://www.kaggle.com/c/bike-sharing-demand).

## DataSet
The original data comes from a public UCI dataset:
https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset


## ML task - [Regression](https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/tasks#regression)

The ML Task for this sample is a Regression, which is a supervised machine learning task that is used to predict the value of the label (in this case the demand units prediction) from a set of related features/variables. 

## Solution

To solve this problem, you build and train an ML model on existing training data, evaluate how good it is (analyzing the obtained metrics), and lastly you can consume/test the model to predict the demand given input data variables.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

However, in this example we train multiple models (instead of a single one), each one based on a different regression learner/algorithm and finally we evaluate the accuracy of each approach/algorithm, so you can choose the trained model with better accuracy.

The following list are the trainers/algorithms used and compared:

- Fast Tree
- Poisson Regressor
- SDCA (Stochastic Dual Coordinate Ascent) Regressor
- FastTreeTweedie

