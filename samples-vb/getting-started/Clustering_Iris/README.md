# Clustering Iris Data
In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to divide iris flowers into different groups that correspond to different types of iris. In the world of machine learning, this task is known as **clustering**.

## Problem
To demonstrate clustering API in action, we will use three types of iris flowers: setosa, versicolor, and virginica. All of them are stored in the same dataset. Even though the type of these flowers is known, we will not use it and run clustering algorithm only on flower parameters such as petal length, petal width, etc. The task is to group all flowers into three different clusters. We would expect the flowers of different types belong to different clusters.

The inputs of the model are following iris parameters:
* petal length
* petal width
* sepal length
* sepal width

## ML task - Clustering
The generalized problem of **clustering** is to group a set of objects in such a way that objects in the same group are more similar to each other than to those in other groups.

Some other examples of clustering:
* group news articles into topics: sports, politics, tech, etc.
* group customers by purchase preferences.
* divide a digital image into distinct regions for border detection or object recognition.

Clustering can look similar to multiclass classification, but the difference is that for clustering tasks we don't know the answers for the past data. So there is no "tutor"/"supervisor" that can tell if our algorithm's prediction was right or wrong. This type of ML task is called **unsupervised learning**.

## Solution
To solve this problem, first we will build and train an ML model. Then we will use trained model for predicting a cluster for iris flowers.

### 1. Build model

Building a model includes: uploading data (`iris-full.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `ColumnConcatenator`), and choosing a learning algorithm (`KMeansPlusPlusClusterer`). All of those steps are stored in a `LearningPipeline`:
```VB
' LearningPipeline holds all steps of the learning process: data, transforms, learners.
' The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
' all the column names and their types.
' ColumnConcatenator concatenates all columns into Features column
' KMeansPlusPlusClusterer is an algorithm that will be used to build clusters. We set the number of clusters to 3.
Dim pipeline As New LearningPipeline From {
    New TextLoader(DataPath).CreateFrom(Of IrisData)(useHeader:=True),
    New ColumnConcatenator("Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth"),
    New KMeansPlusPlusClusterer With {
        .K = 3
    }
}
```
### 2. Train model
Training the model is a process of running the chosen algorithm on the given data. It is implemented in the `Train()` API. To perform training we just call the method and provide our data object  `IrisData` and  prediction object `ClusterPrediction`.
```VB
Dim model = pipeline.Train(Of IrisData, ClusterPrediction)
```
### 3. Consume model
After the model is build and trained, we can use the `Predict()` API to predict the cluster for an iris flower and calculate the distance from given flower parameters to each cluster (each centroid of a cluster).

```VB
Dim prediction1 = model.Predict(TestIrisData.Setosa1)
```
Where `TestIrisData.Setosa1` stores the information about a setosa iris flower.
```VB
Friend Class TestIrisData
    Friend Shared ReadOnly Setosa1 As New IrisData With {
        .SepalLength = 5.1F,
        .SepalWidth = 3.3F,
        .PetalLength = 1.6F,
        .PetalWidth = 0.2F
    }
    ' (...)
End Class
```