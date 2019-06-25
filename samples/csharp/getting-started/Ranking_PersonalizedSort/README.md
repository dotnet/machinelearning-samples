# Rank hotel search results to provide personalized sorting

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.1.0         | Dynamic API       | Up-to-date                    | Console app | .csv file | Ranking hotel search results | Ranking          | LightGbm |

This introductory sample shows how to use ML.NET to predict the relevance and order of hotel search results. In the world of machine learning, this type of prediction is known as ranking.

## Problem
The ability to perform ranking is a common problem faced by search engines since users expect query results to be ranked\sorted according to their relevance.  This problem extends beyond the needs of search engines to include a variety of business scenarios where personalized sorting is key to the user experience.  Here are a few specific examples:
* Travel Agency - Provide a list of hotels with those that are most likely to be purchased\booked by the user positioned highest in the list.
* Shopping - Display items from a product catalog in an order that aligns with a user's shopping preferences.
* Recruiting - Retrieve job applications ranked according to the candidates that are most qualified for a new job opening.

Ranking is useful to any scenario where it is important to list items in an order that increases the likelihood of a click, purchase, reservation, etc.
 
In this sample, we show how to apply ranking to the first example listed above to rank hotel search results according to the likelihood that the hotel will be purchased\booked by the user. To perform ranking, there are two algorithms currently available - FastTree Boosting (FastRank) and Light Gradient Boosting (LightGbm).  We use the LightGbm's Lambdarank algorithm in this sample to automatically build an ML model to predict ranking. 

## Dataset
The training and testing data used by this sample is based on a public [dataset available at Kaggle](https://www.kaggle.com/c/expedia-personalized-sort) originally provided by Expedia (https://www.expedia.com).

Expedia's datasets consist of hotel search results that are grouped according to a user's query; each hotel result includes the following details:
* Hotel attributes, such as location attractiveness and price.
* User's criteria for searching hotels, such as the number of rooms\children\adults, length of stay, etc.
* User's purchase and browsing history, such as whether they clicked the link of a hotel or purchased\booked it.
* Information on similar competitor hotel offerings.

## ML Task - Ranking
As previously mentioned, this sample uses the LightGbm Lambdarank algorithm which is applied using a supervised learning technique known as "Learning to Rank".  This technique requires that train/test datasets contain groups of data instances that are labeled with their ideal ranking value.  The label is a numerical\ordinal value, such as {4, 3, 2, 1, 0} or a text value {"Perfect", "Excellent", "Good", "Fair", or "Bad"}.  The process for labeling these data instances with their ideal ranking value can be done manually by subject matter experts.  Or, the labels can be determined using other metrics, such as the number of clicks on a given search result. This sample uses the latter approach.

Once the train/test datasets are labeled with ideal ranking values, the model (e.g. ranker) can then be trained and tested using this data.  Through the model training process, the ranker learns how to score each data instance within a group based on their label value.  The resulting score of an individual data instance by itself isn't important -- instead, the scores should be compared against one another to determine the relative ordering of a group's data instances.  The higher the score a data instance has, the more relevant and more highly ranked it is within its group.      

## Solution
The sample performs the following high-level steps to rank Expedia hotel search results:
1. Each hotel search result is **labeled** with its ideal ranking value.
2. Once the dataset is labeled, the data is **split** into training and testing datasets.  
3. The model is **trained** using the train dataset using LightGbm Lambdarank algorithm.  
4. The model is **tested** using the test dataset.  This results in a **prediction** that includes a **score** for each hotel instance.  The score is used to determine the ranking relative to other hotels within the same query (e.g. group).  The predictions are then **evaluated** by examining metrics; specifically the [Discounted Cumulative Gain](https://en.wikipedia.org/wiki/Discounted_cumulative_gain).
5. The final step is to **consume** the model to perform ranking predictions for new incoming hotel searches.

### 1. Label Data
To label the data with ideal ranking values, the sample follows [Expedia's evaluation guidance](https://www.kaggle.com/c/expedia-personalized-sort/overview/evaluation):

* 0 - The user neither clicked on this hotel nor purchased\booked a room at this hotel.
* 1 - The user clicked through to see more information on this hotel.
* 2 - The user purchased\booked a room at this hotel.

Expedia's dataset includes both **Click_Bool** and **Booking_Bool** columns that indicate whether the user has clicked or purchased/booked a hotel.  Applying the above guidelines to these columns, we create a new **Label** column that contains values {0, 1, 2} for each hotel search result.

The code for labeling the data is similar to the following:

```CSharp
// Load dataset using TextLoader by specifying the type name that holds the data's schema to be mapped with datasets.
IDataView data = mlContext.Data.LoadFromTextFile<HotelData>(originalDatasetPath, separatorChar: ',', hasHeader: true);

// Create an Estimator and use a custom mapper to transform label hotel instances to values 0, 1, or 2.
IEstimator<ITransformer> dataPipeline = mlContext.Transforms.CustomMapping(Mapper.GetLabelMapper(mlContext, data), null);

// To transform the data, call the Fit() method.
ITransformer dataTransformer = dataPipeline.Fit(data);
IDataView labeledData = dataTransformer.Transform(data);

[...]

// Custom mapper used to label a hotel search result with the ideal rank. 
public static Action<HotelData, HotelRelevance> GetLabelMapper(MLContext mlContext, IDataView data)
{
    Action<HotelData, HotelRelevance> mapper = (input, output) =>
    {
        if (input.Srch_Result_Booked == 1)
        {
            output.Label = 2;
        }
        else if (input.Srch_Result_Clicked == 1)
        {
            output.Label = 1;
        }
        else
        {
            output.Label = 0;
        }
    };

    return mapper;
}
`````
### 2. Split Data
 With the data properly labeled, it is ready to be split into the train/test datasets.  When splitting the data, it's important to make sure that the hotel search results for a given query aren't split across the two datasets.  This would cause label leakage where the same query in our training dataset also exists within the testing dataset.

 Refer to the following code which shows how to split the data:

 ```CSharp
// When splitting the data, 20% is held for the test dataset.
// To avoid label leakage, the GroupId (e.g. search\query id) is specified as the samplingKeyColumnName.  
// This ensures that if two or more hotel instances share the same GroupId, that they are guaranteed to appear in the same subset of data (train or test).
TrainTestData trainTestData = mlContext.Data.TrainTestSplit(labeledData, testFraction: .2, samplingKeyColumnName: nameof(HotelData.GroupId), seed: 1);
IDataView trainData = trainTestData.TrainSet;
IDataView testData = trainTestData.TestSet;

// Save the test dataset to a file to make it faster to load in subsequent runs.
using (var fileStream = File.Create(trainDatasetPath))
{
    mlContext.Data.SaveAsText(trainData, fileStream, separatorChar: ',', headerRow: true, schema: true);
}

// Save the train dataset to a file to make it faster to load in subsequent runs.
using (var fileStream = File.Create(testDatasetPath))
{
    mlContext.Data.SaveAsText(testData, fileStream, separatorChar: ',', headerRow: true, schema: true);
}
`````

### 3. Train Model
This sample trains the model using the LightGbmRankingTrainer which relies on the LightGbm Lambdarank algorithm.  The model requires the following input columns:

* Group Id - Column that contains the group id for each data instance.  Data instances are contained in logical groupings and each group has an identifier known as the group id.  In the case of the Expedia dataset, hotel search results are grouped by their corresponding query where the group id corresponds to the query or search id.  The input group id data type must be [key type](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.data.keydataviewtype). 
* Label - Column that contains the deal rank (e.g. degree of relevance) of each data instance where higher values indicate higher relevance.  The input label data type must be [key type](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.data.keydataviewtype) or [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single). 
* Features - The columns that are influential in determining the relevance\rank of a data instance.  The input feature data must be a fixed size vector of type [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single).

When the trainer is set, **custom gains** are used to apply weights to each of the labeled rank values.  As described earlier in the sample, the potential label rank values are {0, 1, 2} which directly correlates to the specified gains {0, 1, 5}.  This helps to ensure that the model places more emphasis on ranking hotel search results labeled with 2 (e.g. signifies the user purchased\booked the hotel) so that they are positioned higher when compared to results labeled with 0 or 1.    

The following code is used to train the model:

```CSharp
const string FeaturesVectorName = "Features";

// Load the training dataset.
IDataView trainData = mlContext.Data.LoadFromTextFile<HotelData>(trainDatasetPath, separatorChar: ',', hasHeader: true);

// Specify the columns to include in the feature input data.
var featureCols = trainData.Schema.AsQueryable()
    .Select(s => s.Name)
    .Where(c =>
        c == nameof(HotelData.Price_USD) ||
        c == nameof(HotelData.Promotion_Flag) ||
        c == nameof(HotelData.Prop_Id) ||
        c == nameof(HotelData.Prop_Brand) ||
        c == nameof(HotelData.Prop_Review_Score))
    .ToArray();

// Set trainer options.  
LightGbmRankingTrainer.Options options = new LightGbmRankingTrainer.Options();
options.CustomGains = new int[] { 0, 1, 5 };
options.RowGroupColumnName = nameof(HotelData.GroupId);
options.LabelColumnName = nameof(HotelData.Label);
options.FeatureColumnName = FeaturesVectorName;

// Create an Estimator and transform the data:
// 1. Concatenate the feature columns into a single Features vector.
// 2. Create a key type for the label input data by using the value to key transform.
// 3. Create a key type for the group input data by using a hash transform.
IEstimator<ITransformer> dataPipeline = mlContext.Transforms.Concatenate(FeaturesVectorName, featureCols)
    .Append(mlContext.Transforms.Conversion.MapValueToKey(nameof(HotelData.Label)))
    .Append(mlContext.Transforms.Conversion.Hash(nameof(HotelData.GroupId), nameof(HotelData.GroupId), numberOfBits: 20));

// Set the LightGbm Lambdarank trainer.
IEstimator<ITransformer> trainer = mlContext.Ranking.Trainers.LightGbm(options);
IEstimator<ITransformer> trainerPipeline = dataPipeline.Append(trainer);

// Training the model is a process of running the chosen algorithm on the given data. To perform training you need to call the Fit() method.
ITransformer model = trainerPipeline.Fit(trainData);

// Save the model
 mlContext.Model.Save(model, trainData.Schema, modelPath);
`````

### 4. Test and Evaluate Model
We need this step to conclude how accurate our model is. To do so, the model from the previous step is run against another dataset that was not used in training (e.g. the test dataset).  

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.  Specifically, we can gauge the accuracy of our model using Discounted Cumulative Gain (DCG) and Normalized Discounted Cumulative Gain (NDCG) which are included in the `RankingMetrics` returned by `Evaluate()`. 

When evaluating the `RankingMetrics` for this sample's model, you'll notice that the following metrics are reported for DCG and NDCG (the values that you see when running the sample will be similar to these):
* DCG - @1:1.0191, @2:1.5128, @3:1.8371, @4:2.0922, @5:2.2982, @6:2.4641, @7:2.6051, @8:2.7240, @9:2.8234, @10:2.9133

* NDCG - @1:0.1184, @2:0.1719, @3:0.2082, @4:0.2372, @5:0.2608, @6:0.2798, @7:0.2960, @8:0.3096, @9:0.3210, @10:0.3314

The NDCG values are most useful to examine since this allows us to compare accuracy across different queries.  The potential value of NDCG ranges from **0.0** to **1.0**, with 1.0 being a perfect model that exactly matches the ideal ranking.  

With this in mind, let's look at our model's values for NDCG.  In particular, let's look at the value for **NDCG@10** which is **.3314**. This is the average NDCG for a query returning the top **10** hotel search results.  While **.3314** may seem low compared to **1.0**, a more realistic goal is to reach **.5407** which is the score of the first place winner in [Expedia's Personalize Hotel Search contest on Kaggle](https://www.kaggle.com/c/expedia-personalized-sort/leaderboard).  To increase the model's accuracy, we would need to experiment with feature engineering to continue to improve our model.

Refer to the following code used to test and evaluate the model:

```CSharp
// Load the test data and use the model to perform predictions on the test data.
IDataView testData = mlContext.Data.LoadFromTextFile<HotelData>(testDatasetPath, separatorChar: ',', hasHeader: true);
IDataView predictions = model.Transform(testData);

// Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
ConsoleHelper.EvaluateMetrics(mlContext, predictions);

// Evaluate metrics for up to 10 search results (e.g. NDCG@10).
ConsoleHelper.EvaluateMetrics(mlContext, predictions, 10);
`````

### 5.  Consume Model

After the model is built and trained, we can use the `Predict()` API to predict the ranking of hotel search results for a user query.

```CSharp
DataViewSchema predictionPipelineSchema;
ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);

// Load example data and use the model to perform predictions on it.
IDataView exampleData = mlContext.Data.LoadFromTextFile<HotelData>(exampleDatasetPath, separatorChar: ',', hasHeader: true);

// Predict rankings.
IDataView predictions = predictionPipeline.Transform(exampleData);

// In the predictions, get the scores of the hotel search results included in the first query (e.g. group).
IEnumerable<HotelPrediction> hotelQueries = mlContext.Data.CreateEnumerable<HotelPrediction>(predictions, reuseRowObject: false);
var firstGroupId = hotelQueries.First<HotelPrediction>().GroupId;
IEnumerable<HotelPrediction> firstGroupPredictions = hotelQueries.Take(50).Where(p => p.GroupId == firstGroupId).OrderByDescending(p => p.PredictedRank).ToList();

// The individual scores themselves are NOT a useful measure of accuracy; instead, they are used to determine the ranking where a higher score indicates a higher ranking.
ConsoleHelper.PrintScores(firstGroupPredictions);
`````
