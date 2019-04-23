# ML.NET Samples

[![](https://dotnet.visualstudio.com/_apis/public/build/definitions/9ee6d478-d288-47f7-aacc-f6e6d082ae6d/22/badge)](https://dotnet.visualstudio.com/public/_build/index?definitionId=22 )
[ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) is a cross-platform open-source machine learning framework that makes machine learning accessible to .NET developers. 

In this GitHub repo, we provide samples which will help you get started with ML.NET and how to infuse ML into existing and new .NET apps. 

There are two types of samples/apps in the repo:

* Getting Started  ![](https://raw.githubusercontent.com/dotnet/machinelearning-samples/master/images/app-type-getting-started.png) : ML.NET code focused samples for each ML task or area, usually implemented as simple console apps.

* End-End apps ![](https://github.com/dotnet/machinelearning-samples/raw/master/images/app-type-e2e.png) : Real world examples of web, desktop, mobile, and other applications infused with Machine Learning using ML.NET

The official ML.NET samples are divided in multiple categories depending on the scenario and machine learning problem/task, accessible through the following table:

<<<<<<< HEAD
<table align="middle" width=100%>  
  <tr>
    <td align="middle" colspan="3">Binary classification</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/sentiment-analysis.png" alt="Binary classification chart"><br><br><b>Sentiment analysis <br><a href="samples/csharp/getting-started/BinaryClassification_SentimentAnalysis">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/BinaryClassification_SentimentAnalysis">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td align="middle"><img src="images/spam-detection.png" alt="Movie Recommender chart"><br><br><b>Spam Detection<br><a href="samples/csharp/getting-started/BinaryClassification_SpamDetection">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/BinaryClassification_SpamDetection">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td align="middle"><img src="images/fraud-detection.png" alt="Movie Recommender chart"><br><br><b>Fraud detection<br><a href="samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/BinaryClassification_CreditCardFraudDetection">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
  </tr> 
  <tr>
    <td align="middle"><img src="images/disease-detection.png" alt="disease detection chart"><br><br><b>Heart Disease Prediction <br><a href="samples/csharp/getting-started/BinaryClassification_HeartDiseasePrediction">C#</a><img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td></td>
    <td></td>
  </tr> 
  <tr>
    <td align="middle" colspan="3">Multi-class classification</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/issue-labeler.png" alt="ssue Labeler chart"><br><br><b>Issues classification  <br> <a href="samples/csharp/end-to-end-apps/MulticlassClassification-GitHubLabeler">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/end-to-end-apps/MulticlassClassification-GitHubLabeler">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></b></td>
    <td align="middle"><img src="images/flower-classification.png" alt="Movie Recommender chart"><br><br><b>Iris flowers classification <br><a href="samples/csharp/getting-started/MulticlassClassification_Iris">C#</a> &nbsp; &nbsp;<a href="samples/fsharp/getting-started/MulticlassClassification_Iris">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td align="middle"><img src="images/handwriting-classification.png" alt="Movie Recommender chart"><br><br><b>MNIST<br><a href="samples/csharp/getting-started/MulticlassClassification_mnist">C#</a> &nbsp; &nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Recommendation</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/product-recommendation.png" alt="Product Recommender chart"><br><br><b>Product Recommendation<br><a href="samples/csharp/getting-started/MatrixFactorization_ProductRecommendation">C#</a><img src="images/app-type-getting-started.png" alt="Getting started icon"></h4></td>
    <td align="middle"><img src="images/movie-recommendation.png" alt="Movie Recommender chart" ><br><br><b>Movie Recommender<b><br><a href="samples/csharp/getting-started/MatrixFactorization_MovieRecommendation">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td align="middle"><img src="images/movie-recommendation.png" alt="Movie Recommender chart"><br><br><b>Movie Recommender (E2E app)<br><a href="samples/csharp/end-to-end-apps/Recommendation-MovieRecommender">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></b></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Regression</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/price-prediction.png" alt="Price Prediction chart"><br><br><b>Price Prediction<br><a href="samples/csharp/getting-started/Regression_TaxiFarePrediction">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Regression_TaxiFarePrediction">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td align="middle"><br><img src="images/sales-forcasting.png" alt="Sales ForeCasting chart"><br><br><b>Sales ForeCasting<br><a href="samples/csharp/end-to-end-apps/Regression-SalesForecast">C#</a>  &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"><br><br></b></td>
    <td align="middle"><img src="images/demand-prediction.png" alt="Demand Prediction chart"><br><br><b>Demand Prediction<br><a href="samples/csharp/getting-started/Regression_BikeSharingDemand">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/Regression_BikeSharingDemand">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Clustering</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/customer-segmentation.png" alt="Customer Segmentation chart"><br><br><b>Customer Segmentation<br><a href="samples/csharp/getting-started/Clustering_CustomerSegmentation">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_CustomerSegmentation">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td align="middle"><img src="images/clustering.png" alt="IRIS Flowers chart"><br><br><b>IRIS Flowers clustering<br><a href="samples/csharp/getting-started/Clustering_Iris">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_Iris">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></b></td>
    <td></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Anomaly Detection</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/spike-detection.png" alt="spike detection chart"><br><br><b>Sales Spike Detection<br><a href="samples/csharp/getting-started/SpikeDetection_ShampooSales">C#</a> &nbsp; &nbsp; <img src="images/app-type-getting-started.png" alt="Getting started icon"> &nbsp;
      <a href="samples/csharp/end-to-end-apps/SpikeDetection-ShampooSales-WinForms">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"><b></td>
    <td align="middle"><img src="images/anomaly-detection.png" alt="Power Anomaly detection chart"><br><br><b>Power Anomaly Detection<br><a href="samples/csharp/getting-started/TimeSeries_PowerAnomalyDetection">C#</a> &nbsp; &nbsp; <img src="images/app-type-getting-started.png" alt="Getting started icon"><b></td>
     <td></td>
  </tr> 
  <tr>
    <td align="middle" colspan="3">Deep Learning</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/image-classification.png" alt="Image Classification chart"><br><br><b>Image Classification<br>    (TensorFlow model scoring)<br><a href="samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_ImageClassification_TensorFlow">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"><b></td>
    <td align="middle"><img src="images/image-classification.png" alt="Image Classification chart"><br><br><b>Image Classification<br>    (TensorFlow Estimator)<br><a href="samples/csharp/getting-started/DeepLearning_TensorFlowEstimator">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_TensorFlowEstimator">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"><b></td>
    <td align="middle"><img src="images/object-detection.png" alt="Object Detection chart"><br><br><b>Object Detection<br>    (ONNX model scoring)<br><a href="samples/csharp/getting-started\DeepLearning_ObjectDetection_Onnx">C#</a> &nbsp; &nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"><b></td>
  </tr> 
  <tr>
    <td align="middle" colspan="3">Cross Cutting concerns</td>
  </tr>
  <tr>
  <td align="middle"><img src="images/web.png" alt="web image" ><br><b>Scalable Model on WebAPI<br><a href="samples/csharp/end-to-end-apps/ScalableMLModelOnWebAPI">C#</a> &nbsp; &nbsp; <img src="images/app-type-e2e.png" alt="Getting started icon"><b></td>
  
  <td align="middle"><img src="images/generic-icon.PNG" alt="TBD chart"><br><b>Using Database Sample</b><br>Coming soon</td>
  <td align="middle"><img src="images/generic-icon.PNG" alt="TBD chart"><br><b>Using very large sets</b><br>Coming soon</td>
  </tr>
</table>

# Automate ML.NET models generation (Preview state)

The previous samples show you how to use the ML.NET API 1.0 (GA since May 2019). 
However, we're also working on simplifying ML.NET usage with additional technologies that automate the creation of the model for you so you don't need to write the code by yourself to train a model, you simply need to provide your datasets, the model and the code for running the model will be generated for you.

These additional technologies for automating model generation are in PREVIEW state and currently only support *Binary-Classification, Multiclass Classification and Regression*. In upcoming versions we'll be supporting additional ML Tasks such as *Recommendations, Anomaly Detection, Clustering, etc.*.

## CLI samples: (Preview state)

The ML.NET CLI (command-line interface) is a tool you can run on any command-prompt (Windows, Mac or Linux) for generating good quality ML.NET models based on training datasets you provide. In addition, it also generates sample C# code to run/score that model plus the C# code that was used to create/train it so you can research what algorithm and settings it is using.

| CLI (Command Line Interface)                   |
|----------------------------------|
| [Binary Classification sample](/samples/CLI/BinaryClassification_CLI)   |
| [MultiClass Classification sample](/samples/CLI/MulticlassClassification_CLI) |
| [Regression sample](/samples/CLI/Regression_CLI)                |


## AutoML API samples: (Preview state)

ML.NET AutoML API is basically a set of libraries packaged as a NuGet package you can use from your .NET code. AutoML eliminates the task of selecting different algorithms, hyperparameters. AutoML will intelligently generate many combinations of algorithms and hyperparameters and will find the "best models" for you.

| AutoML API                    |
|----------------------------------|
| [Binary Classification sample](/samples/csharp/getting-started/BinaryClassification_AutoML)   |
| [MultiClass Classification sample](/samples/csharp/getting-started/MulticlassClassification_AutoML) |
| [Regression sample](/samples/csharp/getting-started/Regression_AutoML)                |
| [Cross-cutting topics sample](/samples/csharp/getting-started/Crosscutting_AutoML)                |

=======
<table>
 <tr>
   <td width="25%">
      <h3><b>ML Task</b></h3>
  </td>
  <td>
      <h3 width="35%"><b>Description</b></h3>
  </td>
  <td>
      <h3><b>Scenarios</b></h3>
  </td>
 </tr>
 <tr>
   <td width="25%">
      <h3>Binary classification</h3>
      <img src="images/sentiment-analysis.png" alt="Binary classification chart" align="middle">
  </td>
  <td width="35%">
  Task of classifying the elements of a given set into two groups, predicting which group each one belongs to.
  </td>
    <td>
      <h4>Sentiment analysis &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/BinaryClassification_SentimentAnalysis">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/BinaryClassification_SentimentAnalysis">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Spam Detection &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/BinaryClassification_SpamDetection">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/BinaryClassification_SpamDetection">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Fraud detection &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/BinaryClassification_CreditCardFraudDetection">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Heart disease detection &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/BinaryClassification_HeartDiseaseDetection">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
  </td>
 </tr>
 <tr>
   <td width="25%">
      <h3>Multi-class classification</h3>
      <img src="images/issue-labeler.png" alt="Multi-class classification" align="middle">
  </td>
  <td width="35%">
  Task of classifying instances into one of three or more classes, predicting which group each one belongs to.
  </td>
  <td>
      <h4>Issues classification &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/end-to-end-apps/MulticlassClassification-GitHubLabeler">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/end-to-end-apps/MulticlassClassification-GitHubLabeler">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></h4>
      <h4>Iris flowers classification &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/MulticlassClassification_Iris">C#</a> &nbsp; &nbsp;<a href="samples/fsharp/getting-started/MulticlassClassification_Iris">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>      
      <h4>MNIST &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/MulticlassClassification_mnist">C#</a> &nbsp; &nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
  </td>
 </tr>
 <tr>
   <td width="25%">
      <h3>Regression</h3>
      <img src="images/price-prediction.png" alt="regression icon" align="middle">
  </td>
  <td width="35%">
  The task is to predict a numeric value with given input variable data. It is widely used for forecasting and 'how much / how many' predictions.
  </td>
  <td>
      <h4>Price prediction &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/Regression_TaxiFarePrediction">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Regression_TaxiFarePrediction">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Sales forecast &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/end-to-end-apps/Regression-SalesForecast">C#</a>  &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></h4>
      <h4>Demand prediction &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/Regression_BikeSharingDemand">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/Regression_BikeSharingDemand">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
  </td>
 </tr>
 <tr>
   <td width="25%">
      <h3>Recommendation</h3>
      <img src="images/product-recommendation.png" alt="Recommendations icon" align="middle">
  </td>
  <td width="35%">
  Recommender systems are typically based on content based and collaborative filtering methods. A collaborative method predicts what items/products a user might like based on his past actions/likes/ratings compared to other users. 
  </td>
  <td>
      <h4>Movie recommender &nbsp;&nbsp;&nbsp;
        <a href="samples/csharp/getting-started/MatrixFactorization_MovieRecommendation">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon">
        <a href="samples/csharp/end-to-end-apps/Recommendation-MovieRecommender">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"> </h4>
       <h4>Product recommender &nbsp;&nbsp;&nbsp;
        <a href="samples/csharp/getting-started/MatrixFactorization_ProductRecommendation">C#</a><img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
  </td>
 </tr>
  <tr>
   <td width="25%">
      <h3>Clustering</h3>
      <img src="images/clustering.png" alt="Clustering plotting" align="middle">
  </td>
  <td width="35%">
  ML task of grouping a set of objects in such a way that objects in the same group (called a cluster) are more similar to each other than to those in other groups. It is an exploratory task. It does not classify items across particular labels.
  </td>
  <td>
      <h4>Customer segmentation &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/Clustering_CustomerSegmentation">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_CustomerSegmentation">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Clustering Iris flowers &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/Clustering_Iris">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_Iris">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
  </td>
 </tr>
  <tr>
   <td width="25%">
      <h3>Anomaly detection</h3>
      <img src="images/anomaly-detection.png" alt="anomaly detection chart" lign="middle">
  </td>
  <td width="35%">
  Task's goal is the identification of rare items, events or observations which raise suspicions by differing significantly from the majority of the data.Usually problems such as bank fraud, structural defects or medical problems
  </td>
  <td>
      <h4>Spike Detection-Shampoo Sales &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/SpikeDetection_ShampooSales">C#</a> &nbsp; &nbsp; <img src="images/app-type-getting-started.png" alt="Getting started icon">
      <a href="samples/csharp/end-to-end-apps/SpikeDetection-ShampooSales-WinForms">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></h4>      
     <h4>Spike Detection-PowerMeter Readings &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/getting-started/TimeSeries_PowerAnomalyDetection">C#</a> &nbsp; &nbsp; <img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
  </td>
 </tr>
  <tr>
   <td width="25%">
      <h3>Ranking</h3>
      <img src="images/ranking.png" alt="Ranking logo" align="middle">
  </td>
  <td width="35%">
  Construction of ranking models for information retrieval systems so the items are ordered/ranked based on user's input variables such as likes/dislike, context, interests, etc.
  </td>
  <td>
      <h4>Coming soon</h4>
  </td>
 </tr>
  <tr>
   <td width="25%">
      <h3>Deep Learning</h3>
      <img src="images/image-classification.png" alt="DeepLearning logo"align="middle">
  </td>
  <td width="35%">
  Deep learning is a subset of machine learning. Deep learning architectures such as deep neural networks, are usually applied to fields such as computer vision (object detection, image classification, style transfer), speech recognition, natural language processing and audio recognition. 
  </td>
  <td>
      <h4>TensorFlow(ML.NET Scoring) &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_ImageClassification_TensorFlow">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>TensorFlow(ML.NET Estimator) &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/DeepLearning_TensorFlowEstimator">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_TensorFlowEstimator">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Object detection with ONNX model &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started\DeepLearning_ObjectDetection_Onnx">C#</a> &nbsp; &nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
      <h4>Style Transfer  Coming soon &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></h4>      
  </td>
 </tr> 
 </table>
 
**Cross Cutting ML.Net Samples:** The below samples are created for cross cutting scenarios like Scalable WebAPI services, Datasets stored in Database etc.

 <table>
 <tr>
   <td width="25%">
      <h3><b>Sample Name</b></h3>
  </td>
  <td>
      <h3 width="35%"><b>Description</b></h3>
  </td>
  <td>
      <h3><b>ML Scenarios</b></h3>
  </td>
 </tr>
 <tr>
   <td width="25%">
      <h3>Scalable WebAPI</h3>
      <img src="images/web.png" alt="Binary classification chart" align="middle">
  </td>
  <td width="35%">
   This sample explains how to optimize your code when running an ML.NET model on an ASP.NET Core WebAPI service
  </td>
    <td>
      <h4>Sentiment analysis &nbsp;&nbsp;&nbsp;
      <a href="samples/csharp/end-to-end-apps/ScalableMLModelOnWebAPI">C#</a> &nbsp; &nbsp; <img src="images/app-type-e2e.png" alt="Getting started icon"></h4>
  </td>
 </tr>
 </table>

**NuGet feed configuration:** Usually you just need to use the regular NuGet feed (https://api.nuget.org/v3/index.json), however, during a few days before releasing a minor release (such as 0.9, 0.10, 0.11, etc.) we'll be using Preview NuGet packages available in MyGet (such as 0.11.0-preview-27128-1), not available in the regular NuGet feed.

If that is the case, please use this MyGet feed in Visual Studio or your NuGet feed configuration:

https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
>>>>>>> origin/master

-------------------------------------------------------

# Additional ML.NET Community Samples

In addition to the ML.NET samples provided by Microsoft, we're also highlighting samples created by the community shocased in this separated page:
[ML.NET Community Samples](https://github.com/dotnet/machinelearning-samples/blob/master/docs/COMMUNITY-SAMPLES.md)

Those Community Samples are not maintained by Microsoft but by their owners.
If you have created any cool ML.NET sample, please, add its info into this [REQUEST issue](https://github.com/dotnet/machinelearning-samples/issues/86) and we'll publish its information in the mentioned page, eventually.

## Translations of Samples:
- [Chinese Simplified](https://github.com/feiyun0112/machinelearning-samples.zh-cn)

# Learn more

See [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) for detailed information on tutorials, ML basics, etc.

# API reference

Check out the [ML.NET API Reference](https://docs.microsoft.com/dotnet/api/?view=ml-dotnet) to see the breadth of APIs available.

# Contributing

We welcome contributions! Please review our [contribution guide](CONTRIBUTING.md).

# Community

Please join our community on Gitter [![Join the chat at https://gitter.im/dotnet/mlnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/mlnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community.
For more information, see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

# License

[ML.NET Samples](https://github.com/dotnet/machinelearning-samples) are licensed under the [MIT license](LICENSE).
