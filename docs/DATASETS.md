# Datasets used by Samples at ML.NET Samples repo

This page serves as a way to track down the approval of the datasets being used by the ML.NET samples.

MICROSOFT PROVIDES THE DATASETS ON AN "AS IS" BASIS. MICROSOFT MAKES NO WARRANTIES, EXPRESS OR IMPLIED, GUARANTEES OR CONDITIONS WITH RESPECT TO YOUR USE OF THE DATASETS. TO THE EXTENT PERMITTED UNDER YOUR LOCAL LAW, MICROSOFT DISCLAIMS ALL LIABILITY FOR ANY DAMAGES OR LOSSES, INLCUDING DIRECT, CONSEQUENTIAL, SPECIAL, INDIRECT, INCIDENTAL OR PUNITIVE, RESULTING FROM YOUR USE OF THE DATASETS.
The datasets are provided under the original terms that Microsoft received such datasets. See below for more information about each dataset.

|  Dataset name   | Original Dataset | Processed Dataset |       Sample using the Dataset        | Approval Status |
|-----------------|------------------|--------------------|----------------------------------------|--------|
| Wikipedia Detox |   [Original](https://meta.wikimedia.org/wiki/Research:Detox/Data_Release)   | [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SentimentAnalysis/datasets)                   | [BinaryClassification_SentimentAnalysis](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SentimentAnalysis) | PENDING |
| Credit Card Fraud Detection |    [Original](https://www.kaggle.com/mlg-ulb/creditcardfraud)  |    [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection/CreditCardFraudDetection.Trainer/assets/input) | [BinaryClassification_CreditCardFraudDetection](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection) | PENDING |
| Titanic Data |    [Original](http://biostat.mc.vanderbilt.edu/wiki/pub/Main/DataSets/titanic.html)  |    [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets) | [BinaryClasification_Titanic](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClasification_Titanic) | PENDING |
|  Corefx Issues  |  [Original](https://github.com/dotnet/corefx/issues)  |   [Processed](https://github.com/dotnet/machinelearning-samples/blob/master/datasets/corefx-issues-train.tsv)   |   [github-labeler](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/github-labeler)    |  PENDING |
|  Iris flower data set  |  [Original](https://en.wikipedia.org/wiki/Iris_flower_data_set#Use_of_the_data_set)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets)   |   [MulticlassClassification_Iris](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/MulticlassClassification_Iris)    |  PENDING |
|  Iris flower data set  |  [Original](https://en.wikipedia.org/wiki/Iris_flower_data_set#Use_of_the_data_set)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets)   |   [Clustering_Iris](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Clustering_Iris)    |  PENDING |
|  TLC Trip Record Data  |  [Original](http://www.nyc.gov/html/tlc/html/about/trip_record_data.shtml)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets)   |   [Regression_TaxiFarePrediction](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction)    |  PENDING |
|  Online Retail Data Set   |  [Original](http://archive.ics.uci.edu/ml/datasets/online+retail)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/eShopDashboardML/src/eShopForecastModelsTrainer/data)   |   [eShopDashboardML](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/eShopDashboardML)    |  PENDING |
|  Online Retail Data Set   |  [Original](http://archive.ics.uci.edu/ml/datasets/online+retail)  |   [Processed](http://TBD)   |   [Product recommender](http://TBD)    |  PENDING |
|  Bike Sharing Dataset Data Set  |  [Original](https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_BikeSharingDemand/BikeSharingDemandConsoleApp/data)   |   [Regression_BikeSharingDemand](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_BikeSharingDemand)    |  PENDING |
|  TBD Movies-Dataset  |  [Original](http://TBD)  |   [Processed](http://TBD)   |   [Movie recommender](http://TBD)    |  PENDING |
|  WineKMC  |  [Original](https://media.wiley.com/product_ancillary/6X/11186614/DOWNLOAD/ch02.zip) [Related Download 1](http://blog.yhat.com/static/misc/data/WineKMC.xlsx ) [Related Info](http://blog.yhat.com/posts/customer-segmentation-using-python.html) |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Clustering_CustomerSegmentation/CustomerSegmentation.Train/assets/inputs)   |   [Clustering_CustomerSegmentation](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Clustering_CustomerSegmentation)    |  PENDING |
|  WikiMedia photos  |  [Original](https://commons.wikimedia.org/wiki/Category:Images)  |   [Processed](https://github.com/CESARDELATORRE/MLNETTensorFlowScoringv06API/tree/features/dynamicApi/src/ImageClassification/assets/inputs/images)   |   [MLNETTensorFlowScoringv06API ](https://github.com/CESARDELATORRE/MLNETTensorFlowScoringv06API)    |  PENDING |
|  SMS Spam Collection Data Set  |  [Original](https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection)  |   [Processed](http://TBD)   |   [Spam Filter TBD](http://TBD)    |  PENDING |



The datasets are provided under the original terms that Microsoft received such datasets. See below for more information about each dataset.

### Wikipedia Detox

>This dataset is provided under [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/). Redistributing the dataset "wikipedia-detox-250-line-data.tsv" with attribution:
>
> Wulczyn, Ellery; Thain, Nithum; Dixon, Lucas (2016): Wikipedia Detox. figshare.
>
>With modifications by taking a sample of rows and reducing rough language.
>
>Original source: https://doi.org/10.6084/m9.figshare.4054689
>
>Original readme: https://meta.wikimedia.org/wiki/Research:Detox


### NYC Taxi Fare

Redistributing the dataset "taxi-fare-test.csv", "taxi-fare-train.csv" with attribution:

> Original source: https://www.nyc.gov/html/tlc/html/about/trip_record_data.shtml
> 
> The dataset is provided under terms provided by City of New York: https://opendata.cityofnewyork.us/overview/#termsofuse.






