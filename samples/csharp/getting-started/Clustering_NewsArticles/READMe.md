# Clustering News Articles

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1         | Dynamic API | Up-to-date | Console app | .csv file | Clustering News Artciels | Clustering | K-means++ |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to divide news articles into different groups that correspond to different news categories. In the world of machine learning, this task is known as **clustering**.

## Problem
To demonstrate clustering API in action, we will use fourty one categories of news articles: CRIME, ENTERTAINMENT,WORLD NEWS etc. All of them are stored in the same dataset. Even though the type of these news artciles are known, we will not use it and run clustering algorithm only on news short describtion parameters. The task is to group all news categories into fourty one clusters. We would expect the news artciles of different types belong to different clusters.

The inputs of the model are following iris parameters:
* short describtion

## ML task - Clustering
The generalized problem of **clustering** is to group a set of objects in such a way that objects in the same group are more similar to each other than to those in other groups.

Some other examples of clustering:
* group news articles into topics: sports, politics, tech, etc.
* group customers by purchase preferences.
* divide a digital image into distinct regions for border detection or object recognition.

Clustering can look similar to multiclass classification, but the difference is that for clustering tasks we don't know the answers for the past data. So there is no "tutor"/"supervisor" that can tell if our algorithm's prediction was right or wrong. This type of ML task is called **unsupervised learning**.

## Solution
To solve this problem, first we will build and train an ML model. Then we will use trained model for predicting a cluster for news articles.

### 1. Build model

Building a model includes: uploading data (`iris-full.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `Concatenate`), and choosing a learning algorithm (`KMeans`). All of those steps are stored in `trainingPipeline`:
```CSharp
//Create the MLContext to share across components for deterministic results
MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

// STEP 1: Common data loading configuration
var pivotDataView = mlContext.Data.LoadFromTextFile(newsArticlesCsv, new []
{
    new TextLoader.Column("news_articles", DataKind.String, 0)
}, ',', true);
IEstimator<ITransformer> dataProcessPipeline;
                                                
//Split dataset in two parts: TrainingDataset (80%) and TestDataset (20%)
DataOperationsCatalog.TrainTestData trainTestData = mlContext.Data.TrainTestSplit(fullData, testFraction: 0.2);
trainingDataView = trainTestData.TrainSet;
testingDataView = trainTestData.TestSet;

//STEP 2: Process data transformations in pipeline
var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Features", "news_articles");

// STEP 3: Create and train the model     
var trainer = mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 41);
var trainingPipeline = dataProcessPipeline.Append(trainer);
```
### 2. Train model
Training the model is a process of running the chosen algorithm on the given data. To perform training you need to call the Fit() method.
```CSharp
var trainedModel = trainingPipeline.Fit(trainingDataView);
```
### 3. Consume model
After the model is build and trained, we can use the `Predict()` API to predict the cluster for an iris flower and calculate the distance from given flower parameters to each cluster (each centroid of a cluster).

```CSharp
// Test with one sample text 
var clusteringPrediction = new ClusteringPrediction()
{
    NewsArticles = "She left her husband. He killed their children. Just another day in America.",
    Category = "CRIME"
};

// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<IrisData, IrisPrediction>(model);

//Score
var resultprediction = predEngine.Predict(clusteringPrediction);

Console.WriteLine($"Cluster assigned for news article:" + resultprediction.SelectedClusterId);
```
