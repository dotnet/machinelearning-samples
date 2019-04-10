# ML.NET Samples
ML.NET is still new, and as we are developing it, we would love to get your feedback! Please fill out the brief survey below and help shape the future of ML.NET by telling us about your usage and interest in Machine Learning and ML.NET.

<a href="https://www.research.net/r/mlnet-survey">Take the survey now!</a>

At the end of the survey, you can leave your name and e-mail address (optional) so that an engineer on the .NET team can reach out to you to talk a little bit more about your experiences and thoughts. We appreciate your contribution!

-------------------------------------------------------
[![](https://dotnet.visualstudio.com/_apis/public/build/definitions/9ee6d478-d288-47f7-aacc-f6e6d082ae6d/22/badge)](https://dotnet.visualstudio.com/public/_build/index?definitionId=22 )
[ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) is a cross-platform open-source machine learning framework that makes machine learning accessible to .NET developers. In this GitHub repo, we provide samples which will help you get started with ML.NET and how to infuse ML into existing and new .NET apps. 

There are two types of samples/apps in the repo:

* ![](https://github.com/dotnet/machinelearning-samples/blob/features/samples-new-api/images/app-type-getting-started.png)  Getting Started - ML.NET code focused samples for each ML task or area, usually implemented as simple console apps.

* ![](https://github.com/dotnet/machinelearning-samples/blob/features/samples-new-api/images/app-type-e2e.png)  End-End apps - Real world examples of web, desktop, mobile, and other applications infused with Machine Learning using ML.NET

The official ML.NET samples are divided in multiple categories depending on the scenario and machine learning problem/task, accessible through the following table:

<style type="text/css">
.tg  {border-collapse:collapse;border-spacing:0;}
.tg td{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:black;}
.tg th{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:black;}
.tg .tg-34fe{background-color:#c0c0c0;border-color:inherit;text-align:center;vertical-align:top}
.tg .tg-c3ow{border-color:inherit;text-align:center;vertical-align:top}
.tg .tg-uys7{border-color:inherit;text-align:center}
.tg .tg-mn4z{background-color:#c0c0c0;border-color:inherit;text-align:center}
.tg .tg-0pky{border-color:inherit;text-align:left;vertical-align:top}
</style>
<table class="tg">
  <tr>
    <th class="tg-mn4z" colspan="3">Recommendation</th>
  </tr>
  <tr>
    <td class="tg-uys7"><img src="images/product-recommendation.png" alt="Binary classification chart" align="middle"><br><br>Product Recommendation<br><br><a href="samples/csharp/getting-started/MatrixFactorization_ProductRecommendation">C#</a><img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-c3ow"><img src="images/movie-recommendation.png" alt="Movie Recommender chart" align="middle"><br><br>Movie Recommender<br><br><a href="samples/csharp/getting-started/MatrixFactorization_MovieRecommendation">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-c3ow"><img src="images/movie-recommendation.png" alt="Movie Recommender chart" align="middle"><br><br>Movie Recommender<br><br>(End-to-end app)<br><br><a href="samples/csharp/end-to-end-apps/Recommendation-MovieRecommender">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></td>
  </tr>
  <tr>
    <td class="tg-mn4z" colspan="3">Regression</td>
  </tr>
  <tr>
    <td class="tg-c3ow"><img src="images/price-prediction.png" alt="Price Prediction chart" align="middle"><br><br>Price Prediction<br><br><a href="samples/csharp/getting-started/Regression_TaxiFarePrediction">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Regression_TaxiFarePrediction">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-c3ow"><br><img src="images/sales-forcasting.png" alt="Sales ForeCasting chart" align="middle"><br><br>Sales ForeCasting<br><br><a href="samples/csharp/end-to-end-apps/Regression-SalesForecast">C#</a>  &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"><br><br></td>
    <td class="tg-c3ow"><img src="images/demand-prediction.png" alt="Demand Prediction chart" align="middle"><br><br>Demand Prediction<br><br><a href="samples/csharp/getting-started/Regression_BikeSharingDemand">C#</a> &nbsp;&nbsp;&nbsp;<a href="samples/fsharp/getting-started/Regression_BikeSharingDemand">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
  </tr>
  <tr>
    <td class="tg-34fe" colspan="3">Clustering</td>
  </tr>
  <tr>
    <td class="tg-uys7"><img src="images/price-prediction.png" alt="Customer Segmentation chart" align="middle"><br><br>Customer Segmentation<br><br><a href="samples/csharp/getting-started/Clustering_CustomerSegmentation">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_CustomerSegmentation">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-uys7"><img src="images/flower-classification.png" alt="IRIS Flowers chart" align="middle"><br><br>IRIS Flowers clustering<br><br><a href="samples/csharp/getting-started/Clustering_Iris">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/Clustering_Iris">F#</a>&nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-uys7">image<br><br>TBD<br><br>C#   F#</td>
  </tr>
  <tr>
    <td class="tg-34fe" colspan="3">Deep Learning</td>
  </tr>
  <tr>
    <td class="tg-uys7"><img src="images/image-classification.png" alt="Image Classification chart" align="middle"><br><br>Image Classification<br>    (scoring)<br><br><a href="samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_ImageClassification_TensorFlow">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-uys7"><img src="images/image-classification.png" alt="Image Classification chart" align="middle"><br><br>Image Classification<br>    (Transfer Learning)<br><br><a href="samples/csharp/getting-started/DeepLearning_TensorFlowEstimator">C#</a> &nbsp; &nbsp; <a href="samples/fsharp/getting-started/DeepLearning_TensorFlowEstimator">F#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-uys7"><img src="images/object-detection.png" alt="Object Detection chart" align="middle"><br><br>Object Detection<br><br><a href="samples/csharp/getting-started\DeepLearning_ObjectDetection_Onnx">C#</a> &nbsp; &nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
  </tr>
  <tr>
    <td class="tg-c3ow"><img src="images/image-style-transfer.png" alt="style transfer chart" align="middle"><br><br><br>Style Transfer<br><br>Coming soon</td>
    <td class="tg-0pky"></td>
    <td class="tg-0pky"></td>
  </tr>
  <tr>
    <td class="tg-34fe" colspan="3">Anomaly Detection</td>
  </tr>
  <tr>
    <td class="tg-c3ow"><img src="images/spike-detection.png" alt="spike detection chart" align="middle"><br><br>Shampoo Sales Spike Detection<br><br><a href="samples/csharp/getting-started/SpikeDetection_ShampooSales">C#</a> &nbsp; &nbsp; <img src="images/app-type-getting-started.png" alt="Getting started icon">
      <a href="samples/csharp/end-to-end-apps/SpikeDetection-ShampooSales-WinForms">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-e2e.png" alt="End-to-end app icon"></td>
    <td class="tg-c3ow"><img src="images/anomaly-detection.png" alt="Power Anomaly detection chart" align="middle"><br><br>Power Anomaly Detection<br><br><a href="samples/csharp/getting-started/TimeSeries_PowerAnomalyDetection">C#</a> &nbsp; &nbsp; <img src="images/app-type-getting-started.png" alt="Getting started icon"></td>
    <td class="tg-0pky"></td>
  </tr>
  <tr>
    <td class="tg-34fe" colspan="3">Ranking</td>
  </tr>
  <tr>
    <td class="tg-0pky">Coming soon</td>
    <td class="tg-0pky"></td>
    <td class="tg-0pky"></td>
  </tr>
  <tr>
    <td class="tg-34fe" colspan="3">Cross Cutting</td>
  </tr>
  <tr>
  <td class="tg-0pky"><img src="images/web.png" alt="web image" align="middle"><br><br>Scalable Model on WebAPI<br><a href="samples/csharp/end-to-end-apps/ScalableMLModelOnWebAPI">C#</a> &nbsp; &nbsp; <img src="images/app-type-e2e.png" alt="Getting started icon"></td>
    <td class="tg-0pky"><br>Using Database Sample<br>Coming soon</td>
    <td class="tg-0pky"><br>Using very large sets<br>Coming soon</td>
  </tr>
</table>



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
      <h4>Heart disease detection &nbsp;&nbsp;&nbsp;<a href="samples/csharp/getting-started/BinaryClassification_HeartDiseasePrediction">C#</a> &nbsp;&nbsp;&nbsp;<img src="images/app-type-getting-started.png" alt="Getting started icon"></h4>
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
 
**Real Time ML.Net Samples:** The below samples are created for real time scenarios like Scalable WebAPI services, Datasets stored in Database etc.

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

-------------------------------------------------------

## Additional Community Samples

In addition to the ML.NET samples provided by Microsoft, we're also highlighting samples created by the community shocased in this separated page:
[ML.NET Community Samples](https://github.com/dotnet/machinelearning-samples/blob/master/docs/COMMUNITY-SAMPLES.md)

Those Community Samples are not maintained by Microsoft but by their owners.
If you have created any cool ML.NET sample, please, add its info into this [REQUEST issue](https://github.com/dotnet/machinelearning-samples/issues/86) and we'll publish its information in the mentioned page, eventually.

## Translations of Samples:
- [Chinese Simplified](https://github.com/feiyun0112/machinelearning-samples.zh-cn)

## Learn more

See [ML.NET Guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/) for detailed information on tutorials, ML basics, etc.

## API reference

Check out the [ML.NET API Reference](https://docs.microsoft.com/dotnet/api/?view=ml-dotnet) to see the breadth of APIs available.

## Contributing

We welcome contributions! Please review our [contribution guide](CONTRIBUTING.md).

## Community

Please join our community on Gitter [![Join the chat at https://gitter.im/dotnet/mlnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/mlnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community.
For more information, see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## License

[ML.NET Samples](https://github.com/dotnet/machinelearning-samples) are licensed under the [MIT license](LICENSE).
