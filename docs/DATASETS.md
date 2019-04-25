# Datasets used by Samples at ML.NET Samples repo

This page serves as a way to track down the approval of the datasets being used by the ML.NET samples.

MICROSOFT PROVIDES THE DATASETS ON AN "AS IS" BASIS. MICROSOFT MAKES NO WARRANTIES, EXPRESS OR IMPLIED, GUARANTEES OR CONDITIONS WITH RESPECT TO YOUR USE OF THE DATASETS. TO THE EXTENT PERMITTED UNDER YOUR LOCAL LAW, MICROSOFT DISCLAIMS ALL LIABILITY FOR ANY DAMAGES OR LOSSES, INLCUDING DIRECT, CONSEQUENTIAL, SPECIAL, INDIRECT, INCIDENTAL OR PUNITIVE, RESULTING FROM YOUR USE OF THE DATASETS.
The datasets are provided under the original terms that Microsoft received such datasets. See below for more information about each dataset.

|  Dataset name   | Original Dataset | Processed Dataset |       Sample using the Dataset        | Approval Status |
|-----------------|------------------|--------------------|----------------------------------------|--------|
| Wikipedia Detox |   [Original](https://meta.wikimedia.org/wiki/Research:Detox/Data_Release)   | [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SentimentAnalysis/datasets)                   | [BinaryClassification_SentimentAnalysis](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_SentimentAnalysis) | APPROVED |
| Credit Card Fraud Detection |    [Original](https://www.kaggle.com/mlg-ulb/creditcardfraud)  |    [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection/CreditCardFraudDetection.Trainer/assets/input) | [BinaryClassification_CreditCardFraudDetection](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection) | APPROVED |
|  Corefx Issues  |  [Original](https://github.com/dotnet/corefx/issues)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/MulticlassClassification-GitHubLabeler/GitHubLabeler/Data)   |   [github-labeler](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/github-labeler)    |  PENDING |
|  Iris flower data set  |  [Original](https://en.wikipedia.org/wiki/Iris_flower_data_set#Use_of_the_data_set)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets)   |   [MulticlassClassification_Iris](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/MulticlassClassification_Iris)    |  APPROVED |
|  Iris flower data set  |  [Original](https://en.wikipedia.org/wiki/Iris_flower_data_set#Use_of_the_data_set)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets)   |   [Clustering_Iris](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Clustering_Iris)    |  APPROVED |
|  TLC Trip Record Data  |  [Original](http://www.nyc.gov/html/tlc/html/about/trip_record_data.shtml)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/datasets)   |   [Regression_TaxiFarePrediction](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction)    |  APPROVED |
|  Online Retail Data Set   |  [Original](http://archive.ics.uci.edu/ml/datasets/online+retail)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/eShopDashboardML/src/eShopForecastModelsTrainer/data)   |   [eShopDashboardML](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/eShopDashboardML)    |  APPROVED |
|  Online Retail Data Set   |  [Original](http://archive.ics.uci.edu/ml/datasets/online+retail)  |   [Processed](http://TBD)   |   [Product recommender](http://TBD)    |  APPROVED |
|  Bike Sharing Dataset  |  [Original](https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_BikeSharingDemand/BikeSharingDemandConsoleApp/data)   |   [Regression_BikeSharingDemand](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_BikeSharingDemand)    |  APPROVED |
|  TBD Movies-Dataset  |  [Original](http://TBD)  |   [Processed](http://TBD)   |   [Movie recommender](http://TBD)    |  PENDING |
|  WineKMC  |  [Original](https://media.wiley.com/product_ancillary/6X/11186614/DOWNLOAD/ch02.zip) [Related Download 1](http://blog.yhat.com/static/misc/data/WineKMC.xlsx ) [Related Info](http://blog.yhat.com/posts/customer-segmentation-using-python.html) |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Clustering_CustomerSegmentation/CustomerSegmentation.Train/assets/inputs)   |   [Clustering_CustomerSegmentation](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Clustering_CustomerSegmentation)    |  APPROVED |
|  WikiMedia photos  |  [Original](https://commons.wikimedia.org/wiki/Category:Images)  |   [Processed](https://github.com/CESARDELATORRE/MLNETTensorFlowScoringv06API/tree/features/dynamicApi/src/ImageClassification/assets/inputs/images)   |   [MLNETTensorFlowScoringv06API ](https://github.com/CESARDELATORRE/MLNETTensorFlowScoringv06API)    |  APPROVED |
|  SMS Spam Collection Data Set  |  [Original](https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection)  |   [Processed](http://TBD)   |   [Spam Filter TBD](http://TBD)    |  PENDING until de-identify, cleaned-up |



The datasets are provided under the original terms that Microsoft received such datasets. See below for more information about each dataset.

### Wikipedia Detox dataset

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
>This dataset is provided under [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/). Redistributing the dataset "wikipedia-detox-250-line-data.tsv" with attribution:
>
> Wulczyn, Ellery; Thain, Nithum; Dixon, Lucas (2016): Wikipedia Detox. figshare.
>
>With modifications by taking a sample of rows and reducing rough language.
>
>Original source: https://doi.org/10.6084/m9.figshare.4054689
>
>Original readme: https://meta.wikimedia.org/wiki/Research:Detox

### Credit Card Fraud Detection dataset

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
>The trainig and testing data is based on a public dataset available at Kaggle originally from Worldline and the Machine Learning Group (http://mlg.ulb.ac.be) of ULB (Université Libre de Bruxelles), collected and analysed during a research collaboration.
>
>The datasets contains transactions made by credit cards in September 2013 by european cardholders. This dataset presents transactions that occurred in two days, where we have 492 frauds out of 284,807 transactions.
>
> License: https://opendatacommons.org/licenses/odbl/1.0/
>
>By: Andrea Dal Pozzolo, Olivier Caelen, Reid A. Johnson and Gianluca Bontempi. Calibrating Probability with Undersampling for Unbalanced Classification. In Symposium on Computational Intelligence and Data Mining (CIDM), IEEE, 2015
>
>More details on current and past projects on related topics are available on http://mlg.ulb.ac.be/BruFence and http://mlg.ulb.ac.be/ARTML
>

### Corefx Issues

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
>Issues downloaded from public repository https://github.com/dotnet/corefx.
>

### Iris dataset

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
>Original source: https://en.wikipedia.org/wiki/Iris_flower_data_set#Use_of_the_data_set.
>

### TLC Trip Record Data (NYC Taxi Fare)

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source: https://www.nyc.gov/html/tlc/html/about/trip_record_data.shtml
> 
> The dataset is provided under terms provided by City of New York: https://opendata.cityofnewyork.us/overview/#termsofuse.
>

### Online Retail Data Set (eShopDashboardML sample)

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source: Online Retail Dataset from UCI: http://archive.ics.uci.edu/ml/datasets/online+retail
>
>Daqing Chen, Sai Liang Sain, and Kun Guo, Data mining for the online retail industry: A case study of RFM model-based customer segmentation using data mining, Journal of Database Marketing and Customer Strategy Management, Vol. 19, No. 3, pp. 197â€“208, 2012 (Published online before print: 27 August 2012. doi: 10.1057/dbm.2012.17).

### Bike Sharing Dataset

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source:  from UCI dataset: https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset
>

### WineKMC dataset

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source:  John Foreman's book Data Smart: http://www.john-foreman.com/data-smart-book.html
>
> Dataset copyright
> Copyright © 2000-2018 by John Wiley & Sons, Inc., or related companies. All rights reserved.
>
> https://media.wiley.com/product_ancillary/6X/11186614/DOWNLOAD/ch02.zip
> 

### WikiMedia photos

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source: https://commons.wikimedia.org/wiki/Category:Images
>
> Specific licenses per images used in samples
> https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow/ImageClassification/assets/inputs/images/wikimedia.md
>


### SMS Spam Collection Data Set
>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source: https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection
>

