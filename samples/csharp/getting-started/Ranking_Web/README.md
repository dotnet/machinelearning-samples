# Rank search engine results

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4         | Dynamic API       | Up-to-date                    | Console app | .csv file | Ranking search engine results | Ranking          | LightGBM |

This introductory sample shows how to use ML.NET to predict the the best order to display search engine results. In the world of machine learning, this type of prediction is known as ranking.

## Problem
The ability to perform ranking is a common problem faced by search engines since users expect query results to be ranked/sorted according to their relevance. This problem extends beyond the needs of search engines to include a variety of business scenarios where personalized sorting is key to the user experience. Here are a few specific examples:
* Travel Agency - Provide a list of hotels with those that are most likely to be purchased/booked by the user positioned highest in the list.
* Shopping - Display items from a product catalog in an order that aligns with a user's shopping preferences.
* Recruiting - Retrieve job applications ranked according to the candidates that are most qualified for a new job opening.

Ranking is useful to any scenario where it is important to list items in an order that increases the likelihood of a click, purchase, reservation, etc.
 
In this sample, we show how to apply ranking to search engine results. To perform ranking, there are two algorithms currently available - FastTree Boosting (FastRank) and Light Gradient Boosting Machine (LightGBM). We use the LightGBM's LambdaRank implementation in this sample to automatically build an ML model to predict ranking. 

## Dataset
The data used by this sample is based on a public [dataset provided by Microsoft](https://www.microsoft.com/en-us/research/project/mslr/) originally provided Microsoft Bing. The dataset is released under a [CC-by 4.0](https://creativecommons.org/licenses/by/4.0/) license and includes training, validation, and testing data.

```
@article{DBLP:journals/corr/QinL13,
  author    = {Tao Qin and 
               Tie{-}Yan Liu},
  title     = {Introducing {LETOR} 4.0 Datasets},
  journal   = {CoRR},
  volume    = {abs/1306.2597},
  year      = {2013},
  url       = {https://arxiv.org/abs/1306.2597},
  timestamp = {Mon, 01 Jul 2013 20:31:25 +0200},
  biburl    = {https://dblp.uni-trier.de/rec/bib/journals/corr/QinL13},
  bibsource = {dblp computer science bibliography, https://dblp.org}
}
```

The following description is provided for this dataset:

The datasets are machine learning data, in which queries and urls are represented by IDs. The datasets consist of feature vectors extracted from query-url pairs along with relevance judgment labels:

* The relevance judgments are obtained from a retired labeling set of a commercial web search engine (Microsoft Bing), which take 5 values from 0 (irrelevant) to 4 (perfectly relevant).

* The features are basically extracted by us (e.g. Microsoft), and are those widely used in the research community.

In the data files, each row corresponds to a query-url pair. The first column is relevance label of the pair, the second column is query id, and the following columns are features. The larger value the relevance label has, the more relevant the query-url pair is. A query-url pair is represented by a 136-dimensional feature vector.

## ML Task - Ranking
As previously mentioned, this sample uses the LightGBM LambdaRank algorithm which is applied using a supervised learning technique known as [**Learning to Rank**](https://en.wikipedia.org/wiki/Learning_to_rank). This technique requires that train/validation/test datasets contain groups of data instances that are each labeled with their relevance score (e.g. relevance judgment label). The label is a numerical/ordinal value, such as {0, 1, 2, 3, 4}. The process for labeling these data instances with their relevance scores can be done manually by subject matter experts. Or, the labels can be determined using other metrics, such as the number of clicks on a given search result. 

It is expected that the dataset will have many more "Bad" relevance scores than "Perfect". This helps to avoid converting a ranked list directly into equally sized bins of {0, 1, 2, 3, 4}. The relevance scores are also reused so that you will have many items **per group** that are labeled 0, which means the result is "Bad". And, only one or a few labeled 4, which means that the result is "Perfect". Here is a breakdown of the dataset's distribution of labels. You'll notice that there are 70x more 0 (e.g. "Bad") than 4 (e.g. "Perfect") labels:
* Label 0 -- 624,263
* Label 1 -- 386,280
* Label 2 -- 159,451
* Label 3 -- 21,317
* Label 4 -- 8,881

Once the train/validation/test datasets are labeled with relevance scores, the model (e.g. ranker) can then be trained and evaluated using this data. Through the model training process, the ranker learns how to score each data instance within a group based on their label value. The resulting score of an individual data instance by itself isn't important -- instead, the scores should be compared against one another to determine the relative ordering of a group's data instances. The higher the score a data instance has, the more relevant and more highly ranked it is within its group.     

## Solution
Since this sample's dataset already is already labeled with relevance scores, we can immediately start with training the model. In cases where you start with a dataset that isn't labeled, you will need to go through this process first by having subject matter experts provide relevance scores or by using some other metrics to determine relevance.

Generally, the pattern to train, validate, and test a model includes the following steps:
1. The model is trained on the **training** dataset. The model's metrics are then evaluated using the **validation** dataset.
2. Step #1 is repeated by retraining and reevaluating the model until the desired metrics are achieved. The outcome of this step is a pipeline that applies the necessary data transformations and trainer.
3. The pipeline is used to train on the combined **training** + **validation** datasets. The model's metrics are then evaluated on the **testing** dataset (exactly once) -- this is the final set of metrics used to measure the model's quality.
4. The final step is to retrain the pipeline on **all** of the combined **training** + **validation** +  **testing** datasets. This model is then ready to be deployed into production.

The final estimate of how well the model will do in production is the metrics from step #3. The final model for production, trained on all available data, is trained in step #4.

This sample performs a simplified version of the above steps to rank the search engine results:
1. The pipeline is setup with the necessary data transforms and the LightGBM LambdaRank trainer.
2. The model is **trained** using the **training** dataset. The model is then **evaluated** using the **validation** dataset. This results in a **prediction** for each search engine result. The predictions are **evaluated** by examining metrics; specifically the [Normalized Discounted Cumulative Gain](https://en.wikipedia.org/wiki/Discounted_cumulative_gain) (NDCG). 
3. The pipeline is used to **retrain** the model using the **training + validation** datasets. The resulting model is **evaluated** using the **test** dataset -- this is our final set of metrics for the model.
4. The model is **retrained** one last time using the **training + validation + testing** datasets. The final step is to **consume** the model to perform ranking predictions for new incoming searches. This results in a **score** for each search engine result. The score is used to determine the ranking relative to other results within the same query (e.g. group). 

### 1. Setup the Pipeline
This sample trains the model using the LightGbmRankingTrainer which relies on LightGBM LambdaRank. The model requires the following input columns:

* Group Id - Column that contains the group id for each data instance. Data instances are contained in logical groupings representing all candidate results in a single query and each group has an identifier known as the group id. In the case of the search engine dataset, search results are grouped by their corresponding query where the group id corresponds to the query id. The input group id data type must be [key type](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.data.keydataviewtype). 
* Label - Column that contains the relevance label of each data instance where higher values indicate higher relevance. The input label data type must be [key type](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.data.keydataviewtype) or [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single). 
* Features - The columns that are influential in determining the relevance/rank of a data instance. The input feature data must be a fixed size vector of type [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single).

When the trainer is set, **custom gains** (or relevance gains) can also be used to apply weights to each of the labeled relevance scores. This helps to ensure that the model places more emphasis on ranking results higher that have a higher weight. For the purposes of this sample, we use the default provided weights.  

The following code is used to setup the pipeline:

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
`````

### 2. Train and Evaluate Model
First, we need to train our model using the **train** dataset. Then, we need to evaluate our model to determine how effective it is at ranking. To do so, the model is run against another dataset that was not used in training (e.g. the **validation** dataset). 

`Evaluate()` compares the predicted values for the **validation** dataset against the dataset's labels and produces various metrics you can explore. Specifically, we can gauge the quality of our model using Discounted Cumulative Gain (DCG) and Normalized Discounted Cumulative Gain (NDCG) which are included in the `RankingMetrics` returned by `Evaluate()`. 

When evaluating the `RankingMetrics` for this sample's model, you'll notice that the following metrics are reported for DCG and NDCG (the values that you see when running the sample will be similar to these):
* DCG - @1:11.9736, @2:17.5429, @3:21.2532, @4:24.4245, @5:27.0554, @6:29.5571, @7:31.7560, @8:33.7904, @9:35.7949, @10:37.6874

* NDCG: @1:0.4847, @2:0.4820, @3:0.4833, @4:0.4910, @5:0.4977, @6:0.5058, @7:0.5125, @8:0.5182, @9:0.5247, @10:0.5312

The NDCG values are most useful to examine since this allows us to compare our model's ranking ability across different datasets. The potential value of NDCG ranges from **0.0** to **1.0**, with 1.0 being a perfect model that exactly matches the ideal ranking. 

With this in mind, let's look at our model's values for NDCG. In particular, let's look at the value for **NDCG@10** which is **0.5312**. This is the average NDCG for a query returning the top **10** search engine results and is useful to gauge whether the top **10** results will be ranked correctly. To increase the model's ranking ability, we would need to experiment with feature engineering and model hyperparameters and modify the pipeline accordingly. We would continue to iterate on this by modifying the pipeline, training the model, and evaluating the metrics until the desired model quality is achieved.

Refer to the following code used to train and evaluate the model:

```CSharp
// Train the model on the training dataset. To perform training you need to call the Fit() method.
ITransformer model = pipeline.Fit(trainData);

// Load the validation data and use the model to perform predictions on the validation data.
IDataView validationData = mlContext.Data.LoadFromTextFile<SearchResultData>(ValidationDatasetPath, separatorChar: '\t', hasHeader: false);

[...]

// Predict rankings.
IDataView predictions = model.Transform(validationData);

[...]

// Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
RankingMetrics metrics = mlContext.Ranking.Evaluate(predictions);
`````
### 3. Retrain and Perform Final Evaluation of Model
Once the desired metrics are achieved, the resulting pipeline is used to train on the combined **train + validation** datasets. We then evaluate this model one last time using the **test** dataset to get the model's final metrics.

Refer to the following code:

```CSharp
// Train the model on the train + validation dataset.
model = pipeline.Fit(trainValidationData);

// Evaluate the model using the metrics from the testing dataset; you do this only once and these are your final metrics.
IDataView testData = mlContext.Data.LoadFromTextFile<SearchResultData>(TestDatasetPath, separatorChar: '\t', hasHeader: false);

[...]

// Predict rankings.
IDataView predictions = model.Transform(testData);

[...]

// Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
RankingMetrics metrics = mlContext.Ranking.Evaluate(predictions);

```

### 4. Retrain and Consume the Model

The final step is to retrain the model using the all of the data, **training + validation + testing**.

After the model is trained, we can use the `Predict()` API to predict the ranking of search engine results for a new, incoming user query.

```CSharp
// Retrain the model on all of the data, train + validate + test.
model = pipeline.Fit(allData);

// Save the model
mlContext.Model.Save(model, null, modelPath);

// Load the model to perform predictions with it.
DataViewSchema predictionPipelineSchema;
ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);

// Predict rankings.
IDataView predictions = predictionPipeline.Transform(data);

 // In the predictions, get the scores of the search results included in the first query (e.g. group).
 IEnumerable<SearchResultPrediction> searchQueries = mlContext.Data.CreateEnumerable<SearchResultPrediction>(predictions, reuseRowObject: false);
 var firstGroupId = searchQueries.First<SearchResultPrediction>().GroupId;
 IEnumerable<SearchResultPrediction> firstGroupPredictions = searchQueries.Take(100).Where(p => p.GroupId == firstGroupId).OrderByDescending(p => p.Score).ToList();

 // The individual scores themselves are NOT a useful measure of result quality; instead, they are only useful as a relative measure to other scores in the group. 
 // The scores are used to determine the ranking where a higher score indicates a higher ranking versus another candidate result.
 ConsoleHelper.PrintScores(firstGroupPredictions);
`````
