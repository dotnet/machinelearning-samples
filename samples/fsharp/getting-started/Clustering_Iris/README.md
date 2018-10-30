# Clustering Iris flowers (F#)

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.6           | Dynamic API | Up-to-date | Console app | .txt file | Clustering Iris flowers | Clustering | K-means++ |

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

Building a model includes: uploading data (`iris-full.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `ConcatEstimator`), and choosing a learning algorithm (`KMeansPlusPlusTrainer`). All of those steps are stored in a `EstimatorChain`:
```fsharp
	// LearningPipeline holds all steps of the learning process: data, transforms, learners.
    
	//1. Create ML.NET context/environment
    use env = new LocalEnvironment()

    //2. Create DataReader with data schema mapped to file's columns
    let reader = 
        TextLoader(
            env, 
            TextLoader.Arguments(
                Separator = "tab", 
                HasHeader = true, 
                Column = 
                    [|
                        TextLoader.Column("Label", Nullable DataKind.R4, 0)
                        TextLoader.Column("SepalLength", Nullable DataKind.R4, 1)
                        TextLoader.Column("SepalWidth", Nullable DataKind.R4, 2)
                        TextLoader.Column("PetalLength", Nullable DataKind.R4, 3)
                        TextLoader.Column("PetalWidth", Nullable DataKind.R4, 4)
                    |]
                )
            )

    //Load training data
    let trainingDataView = MultiFileSource(DataPath) |> reader.Read
```
### 2. Train model
Training the model is a process of running the chosen algorithm on the given data. It is implemented in the `Fit()` method from the Estimator object. To perform training we just call the method and provide our data.
```fsharp
    let model = 
        env
        |> Pipeline.concatEstimator "Features" [| "SepalLength"; "SepalWidth"; "PetalLength"; "PetalWidth" |]
        |> Pipeline.append (KMeansPlusPlusTrainer(env, "Features", clustersCount = 3))
        |> Pipeline.fit trainingDataView

```
### 3. Consume model
After the model is build and trained, we can use the `Predict()` API to predict the cluster for an iris flower and calculate the distance from given flower parameters to each cluster (each centroid of a cluster).

```fsharp
    let sampleIrisData = 
        { 
            SepalLength = 3.3f
            SepalWidth = 1.6f
            PetalLength = 0.2f
            PetalWidth = 5.1f 
        }

    let predictionFunc = loadedModel.MakePredictionFunction<IrisData, IrisPrediction> env
    let prediction = predictionFunc.Predict sampleIrisData

    printfn "Clusters assigned for setosa flowers: %d" prediction.SelectedClusterId
```
```
