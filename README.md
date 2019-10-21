# ML.NET Samples

[ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) is a cross-platform open-source machine learning framework that makes machine learning accessible to .NET developers. 

In this GitHub repo, we provide samples which will help you get started with ML.NET and how to infuse ML into existing and new .NET apps. 

**Note:** Please open issues related to [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) framework in the [Machine Learning repository](https://github.com/dotnet/machinelearning/issues). Please create the issue in this repo only if you face issues with the samples in this repository.

There are two types of samples/apps in the repo:

* Getting Started  ![](./images/app-type-getting-started-term-cursor.png) : ML.NET code focused samples for each ML task or area, usually implemented as simple console apps.

* End-End apps ![](./images/app-type-e2e-black.png) : End-user sample web and desktop apps infused with Machine Learning models based on ML.NET.

The official ML.NET samples are divided in multiple categories depending on the scenario and machine learning problem/task, accessible through the following tables:

<table align="middle" width=100%>  
  <tr>
    <td align="middle" colspan="3">Binary classification</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/sentiment-analysis.png" alt="Binary classification chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Sentiment Analysis<br><a href="samples/csharp/getting-started/BinaryClassification_SentimentAnalysis">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/BinaryClassification_SentimentAnalysis">F#</a></b></td>
    <td align="middle"><img src="images/spam-detection.png" alt="Movie Recommender chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Spam Detection<br><a href="samples/csharp/getting-started/BinaryClassification_SpamDetection">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/BinaryClassification_SpamDetection">F#</a></b></td>
    <td align="middle"><img src="images/anomaly-detection.png" alt="Power Anomaly detection chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Credit Card Fraud Detection<br>(Binary Classification)<br><a href="samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/BinaryClassification_CreditCardFraudDetection">F#</a></b></td>
  </tr> 
  <tr>
    <td align="middle"><img src="images/disease-detection.png" alt="disease detection chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Heart Disease Prediction <br><a href="samples/csharp/getting-started/BinaryClassification_HeartDiseaseDetection">C#</a></td>
    <td></td>
    <td></td>
  </tr> 
  <tr>
    <td align="middle" colspan="3">Multi-class classification</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/issue-labeler.png" alt="Issue Labeler chart"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Issues Classification  <br> <a href="samples/csharp/end-to-end-apps/MulticlassClassification-GitHubLabeler">C#</a>&nbsp;&nbsp;<a href="samples/fsharp/end-to-end-apps/MulticlassClassification-GitHubLabeler">F#</a></b></td>
    <td align="middle"><img src="images/flower-classification.png" alt="Movie Recommender chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Iris Flowers Classification <br><a href="samples/csharp/getting-started/MulticlassClassification_Iris">C#</a> &nbsp; &nbsp;<a href="samples/fsharp/getting-started/MulticlassClassification_Iris">F#</a></b></td>
    <td align="middle"><img src="images/handwriting-classification.png" alt="Movie Recommender chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>MNIST<br><a href="samples/csharp/getting-started/MulticlassClassification_MNIST">C#</a></b></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Recommendation</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/product-recommendation.png" alt="Product Recommender chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Product Recommendation<br><a href="samples/csharp/getting-started/MatrixFactorization_ProductRecommendation">C#</a></h4></td>
    <td align="middle"><img src="images/movie-recommendation.png" alt="Movie Recommender chart" ><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Movie Recommender <br>(Matrix Factorization)<b><br><a href="samples/csharp/getting-started/MatrixFactorization_MovieRecommendation">C#</a></b></td>
    <td align="middle"><img src="images/movie-recommendation.png" alt="Movie Recommender chart"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Movie Recommender <br>(Field Aware Factorization Machines)<br><a href="samples/csharp/end-to-end-apps/Recommendation-MovieRecommender">C#</a></b></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Regression</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/price-prediction.png" alt="Price Prediction chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Price Prediction<br><a href="samples/csharp/getting-started/Regression_TaxiFarePrediction">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Regression_TaxiFarePrediction">F#</a></b></td>
    <td align="middle"><br><img src="images/sales-forcasting.png" alt="Sales ForeCasting chart"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Sales Forecasting (Regression)<br><a href="samples/csharp/end-to-end-apps/Forecasting-Sales">C#</a><br><br></b></td>
    <td align="middle"><img src="images/demand-prediction.png" alt="Demand Prediction chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Demand Prediction<br><a href="samples/csharp/getting-started/Regression_BikeSharingDemand">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/Regression_BikeSharingDemand">F#</a></b></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Time Series Forecasting</td>
  </tr>
  <tr>
    <td align="middle"><br><img src="images/sales-forcasting.png" alt="Sales ForeCasting chart"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Sales Forecasting (Time Series)<br><a href="samples/csharp/end-to-end-apps/Forecasting-Sales">C#</a><br><br></b></td>
    <td></td>
    <td></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Anomaly Detection</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/spike-detection.png" alt="Spike detection chart"><br><br><b>Sales Spike Detection<br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon">&nbsp;<a href="samples/csharp/getting-started/AnomalyDetection_Sales">C#</a>&nbsp&nbsp;&nbsp;&nbsp;&nbsp;
      <img src="images/app-type-e2e-black.png" alt="End-to-end app icon">&nbsp;<a href="samples/csharp/end-to-end-apps/AnomalyDetection-Sales">C#</a><b></td>
    <td align="middle"><img src="images/spike-detection.png" alt="Spike detection chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Power Anomaly Detection<br><a href="samples/csharp/getting-started/AnomalyDetection_PowerMeterReadings">C#</a><b></td>
      <td align="middle"><img src="images/anomaly-detection.png" alt="Power Anomaly detection chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Credit Card Fraud Detection<br>(Anomaly Detection)<br><a href="samples/csharp/getting-started/AnomalyDetection_CreditCardFraudDetection">C#</a><b></td>
  </tr> 
  <tr>
    <td align="middle" colspan="3">Clustering</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/customer-segmentation.png" alt="Customer Segmentation chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Customer Segmentation<br><a href="samples/csharp/getting-started/Clustering_CustomerSegmentation">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_CustomerSegmentation">F#</a></b></td>
    <td align="middle"><img src="images/clustering.png" alt="IRIS Flowers chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>IRIS Flowers Clustering<br><a href="samples/csharp/getting-started/Clustering_Iris">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_Iris">F#</a></b></td>
    <td></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Ranking</td>
  </tr>
  <tr>
    <td align="middle"><img src="images/ranking-numbered.png" alt="Ranking chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Rank Search Engine Results<br><a href="samples/csharp/getting-started/Ranking_Web">C#</a><b></td>
      <td></td>
      <td></td>
  </tr>
  <tr>
    <td align="middle" colspan="3">Computer Vision</td>
  </tr>
  <tr>
      <td align="middle"><img src="images/image-classification.png" alt="Image Classification chart"><br><b>Image Classification Training<br>    (High-Level API)<br>
    <img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon">&nbsp;<a href="samples/csharp/getting-started/DeepLearning_ImageClassification_Training">C#</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    </td>
    <td align="middle"><img src="images/image-classification.png" alt="Image Classification chart"><br><b>Image Classification Predictions<br>(Pretrained TensorFlow model scoring)<br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon">&nbsp;<a href="samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow">C#</a> &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_ImageClassification_TensorFlow">F#</a>&nbsp;&nbsp&nbsp&nbsp&nbsp;&nbsp;
      <img src="images/app-type-e2e-black.png" alt="End-to-end app icon">&nbsp;<a href="samples/csharp/end-to-end-apps/DeepLearning_ImageClassification_TensorFlow">C#</a><b></td><b></td>
    <td align="middle"><img src="images/image-classification.png" alt="Image Classification chart"><br><b>Image Classification Training<br>    (TensorFlow Featurizer Estimator)<br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon">&nbsp;<a href="samples/csharp/getting-started/DeepLearning_TensorFlowEstimator">C#</a> &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_TensorFlowEstimator">F#</a><b></td>
  </tr> 
  <tr>
    <td align="middle"><br><img src="images/object-detection.png" alt="Object Detection chart"><br><b>Object Detection<br>    (ONNX model scoring)<br>
    <img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon">&nbsp;<a href="samples/csharp/getting-started/DeepLearning_ObjectDetection_Onnx">C#</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <img src="images/app-type-e2e-black.png" alt="End-to-end app icon">&nbsp;<a href="/samples/csharp/end-to-end-apps/ObjectDetection-Onnx">C#</a><b></td>
  </tr> 
</table>

<br>
<br>

<table >
  <tr>
    <td align="middle" colspan="3">Cross Cutting Scenarios</td>
  </tr>
  <tr>
  <td align="middle"><img src="images/web.png" alt="web image" ><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Scalable Model on WebAPI<br><a href="samples/csharp/end-to-end-apps/ScalableMLModelOnWebAPI-IntegrationPkg">C#</a><b></td>
  <td align="middle"><img src="images/web.png" alt="web image" ><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Scalable Model on Razor web app<br><a href="samples/modelbuilder/BinaryClassification_Sentiment_Razor">C#</a><b></td>
  <td align="middle"><img src="images/azure-functions-20.png" alt="Azure functions logo"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Scalable Model on Azure Functions<br><a href="samples/csharp/end-to-end-apps/ScalableMLModelOnAzureFunction">C#</a><b></td>
</tr>
<tr>
  <td align="middle"><img src="images/smile.png" alt="Database chart"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Scalable Model on Blazor web app<br><a href="samples/csharp/end-to-end-apps/ScalableSentimentAnalysisBlazorWebApp">C#</a><b></td>
  <td align="middle"><img src="images/large-data-set.png" alt="large file chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Large Datasets<br><a href="samples/csharp/getting-started/LargeDatasets">C#</a><b></td>
  <td align="middle"><img src="images/database.png" alt="Database chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Loading data with DatabaseLoader<br><a href="samples/csharp/getting-started/DatabaseLoader">C#</a><b></td>
  </tr>
  <tr>
  <td align="middle"><img src="images/database.png" alt="Database chart"><br><img src="images/app-type-getting-started-term-cursor.png" alt="Getting started icon"><br><b>Loading data with  LoadFromEnumerable<br><a href="samples/csharp/getting-started/DatabaseIntegration">C#</a><b></td>
  <td align="middle"><img src="images/model-explain-smaller.png" alt="Model explainability chart"><br><img src="images/app-type-e2e-black.png" alt="End-to-end app icon"><br><b>Model Explainability<br><a href="samples/csharp/end-to-end-apps/Model-Explainability">C#</a></b></td>
  </tr>
</table>


# Automate ML.NET models generation (Preview state)

The previous samples show you how to use the ML.NET API 1.0 (GA since May 2019). 

However, we're also working on simplifying ML.NET usage with additional technologies that automate the creation of the model for you so you don't need to write the code by yourself to train a model, you simply need to provide your datasets. The "best" model and the code for running it will be generated for you.

These additional technologies for automating model generation are in PREVIEW state and currently only support *Binary-Classification, Multiclass Classification and Regression*. In upcoming versions we'll be supporting additional ML Tasks such as *Recommendations, Anomaly Detection, Clustering, etc.*.

## CLI samples: (Preview state)

The ML.NET CLI (command-line interface) is a tool you can run on any command-prompt (Windows, Mac or Linux) for generating good quality ML.NET models based on training datasets you provide. In addition, it also generates sample C# code to run/score that model plus the C# code that was used to create/train it so you can research what algorithm and settings it is using.

| CLI (Command Line Interface) samples                  |
|----------------------------------|
| [Binary Classification sample](/samples/CLI/BinaryClassification_CLI)   |
| [MultiClass Classification sample](/samples/CLI/MulticlassClassification_CLI) |
| [Regression sample](/samples/CLI/Regression_CLI)                |


## AutoML API samples: (Preview state)

ML.NET AutoML API is basically a set of libraries packaged as a NuGet package you can use from your .NET code. AutoML eliminates the task of selecting different algorithms, hyperparameters. AutoML will intelligently generate many combinations of algorithms and hyperparameters and will find high quality models for you.

| AutoML API samples                    |
|----------------------------------|
| [Binary Classification sample](/samples/csharp/getting-started/BinaryClassification_AutoML)   |
| [MultiClass Classification sample](/samples/csharp/getting-started/MulticlassClassification_AutoML) |
| [Regression sample](/samples/csharp/getting-started/Regression_AutoML)                |
| [Advanced experiment sample](/samples/csharp/getting-started/AdvancedExperiment_AutoML)                |


-------------------------------------------------------

# Additional ML.NET Community Samples

In addition to the ML.NET samples provided by Microsoft, we're also highlighting samples created by the community showcased in this separated page:
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
