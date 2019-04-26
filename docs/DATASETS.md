# Datasets used by Samples at ML.NET Samples repo

This page serves as a way to track down the approval of the datasets being used by the ML.NET samples.

.NET FOUNDATION PROVIDES THE DATASETS ON AN "AS IS" BASIS. .NET FOUNDATION MAKES NO WARRANTIES, EXPRESS OR IMPLIED, GUARANTEES OR CONDITIONS WITH RESPECT TO YOUR USE OF THE DATASETS. TO THE EXTENT PERMITTED UNDER YOUR LOCAL LAW, MICROSOFT DISCLAIMS ALL LIABILITY FOR ANY DAMAGES OR LOSSES, INLCUDING DIRECT, CONSEQUENTIAL, SPECIAL, INDIRECT, INCIDENTAL OR PUNITIVE, RESULTING FROM YOUR USE OF THE DATASETS.
The datasets are provided under the original terms that .NET FOUNDATION received such datasets. See below for more information about each dataset.

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
|  Heart Disease Data Set  |  [Original](https://archive.ics.uci.edu/ml/datasets/Heart+Disease)  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/Data)   |   [Heart Disease Detection](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/BinaryClassification_HeartDiseaseDetection)    |  Approved |
|  Product Sales Data Set  |  sample data created  |   [Processed](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/AnomalyDetection_Sales/SpikeDetection/Data)   |   [Sales Spike Detection](https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/AnomalyDetection_Sales)    |  Approved |

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
>By accessing datasets and feeds available through NYC Open Data, the user agrees to all of the Terms of Use of NYC.gov as well as the Privacy Policy for NYC.gov. The user also agrees to any additional terms of use defined by the agencies, bureaus, and offices providing data. Public data sets made available on NYC Open Data are provided for informational purposes. The City does not warranty the completeness, accuracy, content, or fitness for any particular purpose or use of any public data set made available on NYC Open Data, nor are any such warranties to be implied or inferred with respect to the public data sets furnished therein.
>
>The City is not liable for any deficiencies in the completeness, accuracy, content, or fitness for any particular purpose or use of any public data set, or application utilizing such data set, provided by any third party.
>
>Submitting City Agencies are the authoritative source of data available on NYC Open Data. These entities are responsible for data quality and retain version control of data sets and feeds accessed on the Site. Data may be updated, corrected, or refreshed at any time.

### Online Retail Data Set (eShopDashboardML sample)

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Original source: Online Retail Dataset from UCI: http://archive.ics.uci.edu/ml/datasets/online+retail
>
>Citation Policy:
>
>If you publish material based on databases obtained from this repository, then, in your acknowledgements, please note the assistance you received by using this repository. This will help others to obtain the same data sets and replicate your experiments. We suggest the following pseudo-APA reference format for referring to this repository:
>
>Dua, D. and Graff, C. (2019). UCI Machine Learning Repository [http://archive.ics.uci.edu/ml]. Irvine, CA: University of California, School of Information and Computer Science.
>
>Here is a BiBTeX citation as well:
>
>@misc{Dua:2019 ,
>author = "Dua, Dheeru and Graff, Casey",
>year = "2017",
>title = "{UCI} Machine Learning Repository",
>url = "http://archive.ics.uci.edu/ml",
>institution = "University of California, Irvine, School of Information and Computer Sciences" }
>
>A few data sets have additional citation requests. These requests can be found on the bottom of each data set's web page.
>
>http://archive.ics.uci.edu/ml/datasets/online+retail, in turn, contains the following notice:
>
>Citation Request:
>
>Daqing Chen, Sai Liang Sain, and Kun Guo, Data mining for the online retail industry: A case study of RFM model-based customer segmentation using data mining, Journal of Database Marketing and Customer Strategy Management, Vol. 19, No. 3, pp. 197â€“208, 2012 (Published online before print: 27 August 2012. doi: 10.1057/dbm.2012.17).
>

### Bike Sharing Dataset

>
>Redistributing the "Processed Dataset" datasets with attribution:
>
> Citation Policy:
>
> If you publish material based on databases obtained from this repository, then, in your acknowledgements, please note the assistance you received by using this repository. This will help others to obtain the same data sets and replicate your experiments. We suggest the following pseudo-APA reference format for referring to this repository:
>
> Dua, D. and Graff, C. (2019). UCI Machine Learning Repository [http://archive.ics.uci.edu/ml]. Irvine, CA: University of California, School of Information and Computer Science.
>
> Here is a BiBTeX citation as well:
>
>@misc{Dua:2019 ,
>
>author = "Dua, Dheeru and Graff, Casey",
>
>year = "2017",
>
>title = "{UCI} Machine Learning Repository",
>
>url = "http://archive.ics.uci.edu/ml",
>
>institution = "University of California, Irvine, School of Information and Computer Sciences" }
>
>A few data sets have additional citation requests. These requests can be found on the bottom of each data set's web page. 
>
>https://archive.ics.uci.edu/ml/datasets/bike+sharing+dataset, in turn, contains the following notice:
>
>Citation Request:
>
>Fanaee-T, Hadi, and Gama, Joao, 'Event labeling combining ensemble detectors and background knowledge', Progress in Artificial Intelligence (2013): pp. 1-15, Springer Berlin Heidelberg, 


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
> Citation Policy:
>
>If you publish material based on databases obtained from this repository, then, in your acknowledgements, please note the assistance you received by using this repository. This will help others to obtain the same data sets and replicate your experiments. We suggest the following pseudo-APA reference format for referring to this repository:
>
>Dua, D. and Graff, C. (2019). UCI Machine Learning Repository [http://archive.ics.uci.edu/ml]. Irvine, CA: University of California, School of Information and Computer Science.
>
>Here is a BiBTeX citation as well:
>
>@misc{Dua:2019 ,
>author = "Dua, Dheeru and Graff, Casey",
>year = "2017",
>title = "{UCI} Machine Learning Repository",
>url = "http://archive.ics.uci.edu/ml",
>institution = "University of California, Irvine, School of Information and Computer Sciences" }
>
>A few data sets have additional citation requests. These requests can be found on the bottom of each data set's web page.
>
>https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection, in turn, contains the following notice:
>
>Relevant Papers:
>
>We offer a comprehensive study of this corpus in the following paper. This work presents a number of statistics, studies and baseline results for several machine learning methods. 
>
>Almeida, T.A., GÃ³mez Hidalgo, J.M., Yamakami, A. Contributions to the Study of SMS Spam Filtering: New Collection and Results. Proceedings of the 2011 ACM Symposium on Document Engineering (DOCENG'11), Mountain View, CA, USA, 2011.
>
>Citation Request:
>
>If you find this dataset useful, you make a reference to our paper and the web page.[http://www.dt.fee.unicamp.br/~tiago/smsspamcollection] in your papers, research, etc; 
Send us a message to talmeida ufscar.br or jmgomezh yahoo.es in case you make use of the corpus. 
>
>We would like to thank Min-Yen Kan and his team for making the NUS SMS Corpus available.
>


### Heart Disease Dataset
>
>Redistributing the "Processed Dataset" datasets with attribution:
>
>Citation Policy:
>
>This is available at https://archive.ics.uci.edu/ml/datasets/Heart+Disease
>
>If you publish material based on databases obtained from this repository, then, in your acknowledgements, please note the assistance you received by using this repository. This will help others to obtain the same data sets and replicate your experiments. We suggest the following pseudo-APA reference format for referring to this repository:
>
> Dua, D. and Graff, C. (2019). UCI Machine Learning Repository [http://archive.ics.uci.edu/ml]. Irvine, CA: University of California, School of Information and Computer Science. 
>
>  Here is a BiBTeX citation as well:
>
> @misc{Dua:2019 ,
>
> author = "Dua, Dheeru and Graff, Casey",
>
> year = "2017",
>
> title = "{UCI} Machine Learning Repository",
>
> url = "http://archive.ics.uci.edu/ml",
>
> institution = "University of California, Irvine, School of Information and Computer Sciences" } 
>
>A few data sets have additional citation requests. These requests can be found on the bottom of each data set's web page.
>
>https://archive.ics.uci.edu/ml/datasets/heart+Disease, in turn, contains the following notice:
>
>Citation Request:
>
>The authors of the databases have requested that any publications resulting from the use of the data include the names of the principal investigator responsible for the data collection at each institution. They would be: 
>
>1. Hungarian Institute of Cardiology. Budapest: Andras Janosi, M.D. 
>2. University Hospital, Zurich, Switzerland: William Steinbrunn, M.D. 
>3. University Hospital, Basel, Switzerland: Matthias Pfisterer, M.D. 
>4. V.A. Medical Center, Long Beach and Cleveland Clinic Foundation: Robert Detrano, M.D., Ph.D.
>


### Product Sales Dataset
>we have created sample dataset for Product sales that looks like below.
>
>**Product Sales DataSet:**
>
>| Month  | ProductSales |
>|--------|--------------|
>| 1-Jan  | 271          |
>| 2-Jan  | 150.9        |
>| .....  | .....        |
>| 1-Feb  | 199.3        |
>| ...    | ....         |
>
>
>The Product Sales dataset is based on the dataset “Shampoo Sales Over a Three Year Period” originally sourced from DataMarket and >provided by Time Series Data Library (TSDL), created by Rob Hyndman. 
>
>“Shampoo Sales Over a Three Year Period” Dataset Licensed Under the DataMarket Default Open License:
>
>License
>You are allowed to copy and redistribute the data as long as you clearly indicate the data provider and DataMarket as the original >source.
>
>License summary
>You may copy and redistribute the data. You may make derivative works from the data. You may use the data for commercial purposes. You >may not sublicence the data when redistributing it. You may not redistribute the data under a different license. Source attribution on >any use of this data: Must refer source.

