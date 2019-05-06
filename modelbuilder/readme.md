# ML.NET Model Builder Guide 

## Introduction

Model Builder is a simple UI tool for developers to build, train and ship custom machine learning models in their applications. 

Developers with no ML expertise can use this simple visual interface to connect to their data stored in files, SQL Server and more for training the model.
Model Builder leverages best in class automated machine learning to evaluate different models. It produces the best model for your scenario without any tuning required from the developer.

At the end, developers can generate code for training and consuming this model in their applications.


## Evaluate 

**How to understand model performance**

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

