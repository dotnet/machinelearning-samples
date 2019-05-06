# ML.NET Model Builder Guide 

## Introduction

Model Builder is a simple UI tool for developers to build, train and ship custom machine learning models in their applications. 

Developers with no ML expertise can use this simple visual interface to connect to their data stored in files, SQL Server and more for training the model.
Model Builder leverages best in class automated machine learning to evaluate different models. It produces the best model for your scenario without any tuning required from the developer.

At the end, developers can generate code for training and consuming this model in their applications.

## Scenario 

**Which Machine Learning scenario is right for me?**

Model Builder allows you to solve many real life scenarios by supporting a wide variety of machine learning tasks. 

The table below describes a brief description of these scenarios along with the machine learning tasks and datasets associated with them. 

Try out these datasets with Model Builder to get started! 

<table align="middle" width=100%> 
  <tr>
    <td align="middle"> Scenario
    </td>
    <td align="middle"> Scenario Description 
    </td>
    <td align="middle"> ML Task 
    </td>
    <td align="middle"> Dataset 
    </td>
    <td align="middle"> Column to Predict (Label)
    </td>
  </tr>
  <tr>
    <td align="middle">
    Price Prediction   
    </td>
    <td align="middle">
    Predict the price for a particular item           
    </td>
    <td align="middle">
    Regression    
    </td>
    <td align="middle">
      <a href="https://github.com/dotnet/machinelearning-samples/blob/master/datasets/taxi-fare-train.csv">Link</a>
    </td>
    <td align="middle">
    fare_amount  
    </td>
  </tr> 
  <tr>
    <td align="middle">
    Sales Forecast   
    </td>
    <td align="middle">
    Forecast the sales for items this month          
    </td>
    <td align="middle">
    Regression    
    </td>
    <td align="middle">
      <a href="https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/AnomalyDetection_Sales/SpikeDetection/Data/product-sales.csv">Link</a>
    </td>
    <td align="middle">
    ProductSales
    </td>
  </tr> 
    <tr>
    <td align="middle">
    Sentiment Analysis  
    </td>
    <td align="middle">
    Determine the sentiment for customer reviews as positive of negative        
    </td>
    <td align="middle">
    Binary-Classification 
    </td>
    <td align="middle">
      <a href="https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/BinaryClassification_SentimentAnalysis/SentimentAnalysis/Data/wikiDetoxAnnotated40kRows.tsv">Link</a>
    </td>
    <td align="middle">
    Label
    </td>
  </tr> 
  <tr>
    <td align="middle">
   Spam Detection 
    </td>
    <td align="middle">
    Determine whether a particular email is a scam or not          
    </td>
    <td align="middle">
    Binary-Classification 
    </td>
    <td align="middle">
      <a href="https://archive.ics.uci.edu/ml/machine-learning-databases/00228/smsspamcollection.zip">Link</a>
    </td>
    <td align="middle">
    Label
    </td>
  </tr> 
    <tr>
    <td align="middle">
   Fraud Detection 
    </td>
    <td align="middle">
     Determine whether a particular transaction is fraud or not!         
    </td>
    <td align="middle">
    Binary-Classification 
    </td>
    <td align="middle">
      <a href="https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection/CreditCardFraudDetection.Trainer/assets/input/creditcardfraud-dataset.zip">Link</a>
    </td>
    <td align="middle">
    Label
    </td>
  </tr> 
   <tr>
    <td align="middle">
  Issue Classification
    </td>
    <td align="middle">
   Tag different issues or tickets filed into particular area tags       
    </td>
    <td align="middle">
    Multi-Classification 
    </td>
    <td align="middle">
      <a href="https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/end-to-end-apps/MulticlassClassification-GitHubLabeler/GitHubLabeler/Data/corefx-issues-train.tsv">Link</a>
    </td>
    <td align="middle">
    Area
    </td>
  </tr> 
 </table>
                     
## Train

**How long should I train for?**

Model Builder uses AutoML to explore multiple models to find you the best performing model. 

In general longer training periods will allow AutoML to explore more models with multiple trainers and settings. The table below summarizes the average time taken to get good performance for the datasets we tested with. 

Dataset Size  | Dataset Type       | Avg. Time to train  
------------- | ------------------ | --------------
0 - 10 Mb     | Numeric and Text   | 10 sec
10 - 100 Mb   | Numeric and Text   | 10 min 
100 - 500 Mb  | Numeric and Text   | 30 min 
500 - 1 Gb    | Numeric and Text   | 60 min 
1 Gb+         | Numeric and Text   | 3 hour+ 

The exact time to train is a function of a few parameters e.g. the number of features or columns being used to predict, the type of columns i.e. text vs. numeric and the type of machine learning task. 

We have tested Model Builder with even 1TB dataset but building a high quality model for that size of dataset can take upto four days. 

## Evaluate 

**How do I understand my model performance?**

Model Builder by default splits the data you provide into train and test data respectively. The train data (80% split) is used to train your model and the test data (20% split) is used to evaluate your model. 

When using the Model Builder each scenario maps to a machine learning task. Each ML task has it’s own set of evaluation metrics. The table below describes these mappings of scenario and ML tasks. 

#### Regression (e.g. Price Prediction)

The default metric for regression problems is r-squared, the value of r-square ranges between 0 and 1. 1 is the best possible value or in other words the closer the value of r-square to 1 the better your model is performing. 

Other metrics reported such as absolute-loss, squared-loss and RMS loss are additional metrics which can be used to understand how your model is performing or comparing it against other regression models. 

#### Binary Classification (e.g. Sentiment Analysis)

The default metric for classification problems is accuracy. Accuracy defines the proportion of correct predictions your model is making over the test dataset. The closer to 100% or 1.0 the better it is. 

Other metrics reported such as AUC (Area under the curve) which measures the true positive rate vs. the false positive rate should be greater than 0.50 for models to be acceptable. 

Additional metrics like F1 score can be used to control the balance between Precision and Recall. 

#### Multi-Class Classification (e.g. Issue Classification) 
The default metric for Multi-class classification is Micro Accuracy. The closer the Micro Accuracy to 100% or 1.0 the better it is.  

Another important metric for Multi-class classification is Macro-accuracy, similar to Micro-accuracy the closer to 1.0 the better it is. A good way to think about these two is:

* Micro-accuracy -- how often does an incoming ticket get classified to the right team?

* Macro-accuracy -- for an average team, how often is an incoming ticket correct for their team?

For [more details on understanding model evaluation metrics please refer to this guide](https://aka.ms/mlnet-metrics) which provides details on each of these metrics.

