# Rank search engine results

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.1.0         | Dynamic API       | Up-to-date                    | Console app | .csv file | Ranking search engine results | Ranking          | LightGBM |

This introductory sample shows how to use ML.NET to predict the the best order to display search engine results. In the world of machine learning, this type of prediction is known as ranking.

## Problem
The ability to perform ranking is a common problem faced by search engines since users expect query results to be ranked\sorted according to their relevance.  This problem extends beyond the needs of search engines to include a variety of business scenarios where personalized sorting is key to the user experience.  Here are a few specific examples:
* Travel Agency - Provide a list of hotels with those that are most likely to be purchased\booked by the user positioned highest in the list.
* Shopping - Display items from a product catalog in an order that aligns with a user's shopping preferences.
* Recruiting - Retrieve job applications ranked according to the candidates that are most qualified for a new job opening.

Ranking is useful to any scenario where it is important to list items in an order that increases the likelihood of a click, purchase, reservation, etc.
 
In this sample, we show how to apply ranking to search engine results.  To perform ranking, there are two algorithms currently available - FastTree Boosting (FastRank) and Light Gradient Boosting Machine (LightGBM).  We use the LightGBM's LambdaRank implementation in this sample to automatically build an ML model to predict ranking. 

## Dataset
The training and testing data used by this sample is based on a public [dataset provided by Microsoft](https://www.microsoft.com/en-us/research/project/mslr/) originally provided Microsoft Bing.

The following description is provided for this dataset:

    The datasets are machine learning data, in which queries and urls are represented by IDs. The datasets consist of feature vectors extracted from query-url pairs along with relevance judgment labels:

    * The relevance judgments are obtained from a retired labeling set of a commercial web search engine (Microsoft Bing), which take 5 values from 0 (irrelevant) to 4 (perfectly relevant).

    * The features are basically extracted by us (e.g. Microsoft), and are those widely used in the research community.

    In the data files, each row corresponds to a query-url pair. The first column is relevance label of the pair, the second column is query id, and the following columns are features. The larger value the relevance label has, the more relevant the query-url pair is. A query-url pair is represented by a 136-dimensional feature vector.

## ML Task - Ranking
As previously mentioned, this sample uses the LightGBM LambdaRank algorithm which is applied using a supervised learning technique known as [**Learning to Rank**](https://en.wikipedia.org/wiki/Learning_to_rank).  This technique requires that train\test datasets contain groups of data instances that are each labeled with their relevance score (e.g. relevance judgment label).  The label is a numerical\ordinal value, such as {0, 1, 2, 3, 4}.  The process for labeling these data instances with their relevance scores can be done manually by subject matter experts.  Or, the labels can be determined using other metrics, such as the number of clicks on a given search result. 

It is expected that the dataset will have many more "Bad" relevance scores than "Perfect".  This helps to avoid converting a ranked list directly into equally sized bins of {0, 1, 2, 3, 4}.  The relevance scores are also reused so that you will have many items **per group** that are labeled 0, which means the result is "Bad".  And, only one or a few labeled 4, which means that the result is "Perfect". 

Once the train\test datasets are labeled with relevance scores, the model (e.g. ranker) can then be trained and tested using this data.  Through the model training process, the ranker learns how to score each data instance within a group based on their label value.  The resulting score of an individual data instance by itself isn't important -- instead, the scores should be compared against one another to determine the relative ordering of a group's data instances.  The higher the score a data instance has, the more relevant and more highly ranked it is within its group.      

## Solution
Since this sample's dataset already is already labeled with relevance scores, we can immediately start with training the model.  In cases where you start with a dataset that isn't labeled, you will need to go through this process first by having subject matter experts provide relevance scores or by using some other metrics to determine relevance.

This sample performs the following high-level steps to rank the search engine results:
1. The model is **trained** using the train dataset with LightGBM LambdaRank.  
2. The model is **tested** using the test dataset.  This results in a **prediction** that includes a **score** for each search engine result.  The score is used to determine the ranking relative to other results within the same query (e.g. group).  The predictions are then **evaluated** by examining metrics; specifically the [Normalized Discounted Cumulative Gain](https://en.wikipedia.org/wiki/Discounted_cumulative_gain)(NDCG).
3. The final step is to **consume** the model to perform ranking predictions for new incoming searches.

### 1. Train Model
This sample trains the model using the LightGbmRankingTrainer which relies on LightGBM LambdaRank.  The model requires the following input columns:

* Group Id - Column that contains the group id for each data instance.  Data instances are contained in logical groupings representing all candidate results in a single query and each group has an identifier known as the group id.  In the case of the search engine dataset, search results are grouped by their corresponding query where the group id corresponds to the query id.  The input group id data type must be [key type](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.data.keydataviewtype). 
* Label - Column that contains the relevance label of each data instance where higher values indicate higher relevance.  The input label data type must be [key type](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.data.keydataviewtype) or [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single). 
* Features - The columns that are influential in determining the relevance\rank of a data instance.  The input feature data must be a fixed size vector of type [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single).

When the trainer is set, **custom gains** (or relevance gains) can also be used to apply weights to each of the labeled relevance scores.  This helps to ensure that the model places more emphasis on ranking results higher that have a higher weight.  For the purposes of this sample, we use the default provided weights.   

The following code is used to train the model:

```CSharp
const string FeaturesVectorName = "Features";

// Load the training dataset.
IDataView trainData = mlContext.Data.LoadFromTextFile<SearchResultData>(trainDatasetPath, separatorChar: '\t', hasHeader: true);

// Specify the columns to include in the feature input data.
var featureCols = trainData.Schema.AsQueryable()
    .Select(s => s.Name)
    .Where(c =>
        c != nameof(SearchResultData.Label) &&
        c != nameof(SearchResultData.GroupId))
    .ToArray();

// Create an Estimator and transform the data:
// 1. Concatenate the feature columns into a single Features vector.
// 2. Create a key type for the label input data by using the value to key transform.
// 3. Create a key type for the group input data by using a hash transform.
IEstimator<ITransformer> dataPipeline = mlContext.Transforms.Concatenate(FeaturesVectorName, featureCols)
    .Append(mlContext.Transforms.Conversion.MapValueToKey(nameof(SearchResultData.Label)))
    .Append(mlContext.Transforms.Conversion.Hash(nameof(SearchResultData.GroupId), nameof(SearchResultData.GroupId), numberOfBits: 20));

// Set the LightGBM LambdaRank trainer.
IEstimator<ITransformer> trainer = mlContext.Ranking.Trainers.LightGbm(labelColumnName: nameof(SearchResultData.Label), featureColumnName: FeaturesVectorName, rowGroupColumnName: nameof(SearchResultData.GroupId));  ;
IEstimator<ITransformer> trainerPipeline = dataPipeline.Append(trainer);

// Training the model is a process of running the chosen algorithm on the given data. To perform training you need to call the Fit() method.
ITransformer model = trainerPipeline.Fit(trainData);
IDataView transformedTrainData = model.Transform(trainData);

// Save the model
mlContext.Model.Save(model, null, modelPath);
`````

### 2. Test and Evaluate Model
We need this step to determine how effective our model is at ranking. To do so, the model from the previous step is run against another dataset that was not used in training (e.g. the test dataset).  

`Evaluate()` compares the predicted values for the test dataset and produces various metrics you can explore.  Specifically, we can gauge the quality of our model using Discounted Cumulative Gain (DCG) and Normalized Discounted Cumulative Gain (NDCG) which are included in the `RankingMetrics` returned by `Evaluate()`. 

When evaluating the `RankingMetrics` for this sample's model, you'll notice that the following metrics are reported for DCG and NDCG (the values that you see when running the sample will be similar to these):
* DCG - @1:11.9058, @2:17.4132, @3:21.2908, @4:24.5243, @5:27.3235, @6:29.6794, @7:31.9928, @8:34.0955, @9:36.0850, @10:37.9679

* NDCG - @1:0.5012, @2:0.4945, @3:0.4986, @4:0.5055, @5:0.5131, @6:0.5182, @7:0.5251, @8:0.5308, @9:0.5365, @10:0.5417

The NDCG values are most useful to examine since this allows us to compare our model's ranking ability across different datasets.  The potential value of NDCG ranges from **0.0** to **1.0**, with 1.0 being a perfect model that exactly matches the ideal ranking.  

With this in mind, let's look at our model's values for NDCG.  In particular, let's look at the value for **NDCG@10** which is **0.5417**. This is the average NDCG for a query returning the top **10** search engine results.  To increase the model's ranking ability, we would need to experiment with feature engineering and model hyperparameters to continue to improve our model.

Refer to the following code used to test and evaluate the model:

```CSharp
// Load the test data and use the model to perform predictions on the test data.
IDataView testData = mlContext.Data.LoadFromTextFile<SearchResultData>(testDatasetPath, separatorChar: '\t', hasHeader: false);
IDataView predictions = model.Transform(testData);

[...]

// Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
RankingMetrics metrics = mlContext.Ranking.Evaluate(predictions);
`````

### 3. Consume Model

After the model is built and trained, we can use the `Predict()` API to predict the ranking of search engine results for a user query.

```CSharp
// Load test data and use the model to perform predictions on it.
IDataView testData = mlContext.Data.LoadFromTextFile<SearchResultData>(testDatasetPath, separatorChar: '\t', hasHeader: false);

// Load the model.
DataViewSchema predictionPipelineSchema;
ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);

// Predict rankings.
IDataView predictions = predictionPipeline.Transform(testData);

// In the predictions, get the scores of the search results included in the first query (e.g. group).
IEnumerable<SearchResultPrediction> searchQueries = mlContext.Data.CreateEnumerable<SearchResultPrediction>(predictions, reuseRowObject: false);
var firstGroupId = searchQueries.First<SearchResultPrediction>().GroupId;
IEnumerable<SearchResultPrediction> firstGroupPredictions = searchQueries.Take(100).Where(p => p.GroupId == firstGroupId).OrderByDescending(p => p.Score).ToList();

// The individual scores themselves are NOT a useful measure of result quality; instead, they are only useful as a relative measure to other scores in the group. 
// The scores are used to determine the ranking where a higher score indicates a higher ranking versus another candidate result.
ConsoleHelper.PrintScores(firstGroupPredictions);
`````
