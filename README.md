# ML.NET Samples
[![](https://dotnet.visualstudio.com/_apis/public/build/definitions/9ee6d478-d288-47f7-aacc-f6e6d082ae6d/22/badge)](https://dotnet.visualstudio.com/public/_build/index?definitionId=22 )
# Overview

[ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) is a cross-platform open-source machine learning framework that makes machine learning accessible to .NET developers.

In this, Microsoft provides ML.NET samples that help you to get started with machine learning by using .NET, not just to run/score pre-trained models but fundamentally enabling you to create and train your own custom ML models in .NET. 

# NuGet packages to use
Until ML.NET is released as final v1.0, most of the samples in this repo will be using preview released versions (i.e. v0.6, v0.7, v0.8, etc.) available at **NuGet** by using the  released Microsoft.ML NuGet packages available here:
https://www.nuget.org/packages/Microsoft.ML/

However, a few of the samples might also be using nightly releases available at **MyGet** using this alternate NuGet feed: https://dotnet.myget.org/F/dotnet-core/api/v3/index.json. 

# Samples Gallery classified by ML task 

All the samples are in C# and some of them also support a F# version. We encourage the community to help us migrating samples to F# and newer versions of ML.NET.

There are two types of samples/apps in the repo:

* **Getting started samples (C# and F#)** - ML.NET code focused samples for each ML task or area, usually implemented as simple console apps.
* **End-to-end apps (C#)** - "Real world" examples of web, desktop, mobile, and other applications infused with ML solutions via [ML.NET APIs](https://docs.microsoft.com/dotnet/api/?view=ml-dotnet).

The official ML.NET samples are divided in multiple categories depending on the scenario and machine learning problem/task, accessible through the following table:

-------------------------------------------------------

<table>
 <tr>
   <td>
      <h1>ML Task</h1>
  </td>
  <td>
      <h1>Scenario</h1>
  </td>
 </tr>
 <tr>
   <td>
      <h2>Two-class classification</h2>
      <!--<img src="images/binary-classification-plotting.png" alt="Binary classification chart">-->
  </td>
  <td>
    <table>
    <tr>
    <td>
        <h3>Sentiment analysis</h3>
    </td>
    <td>
        <h3><a href="TBD/Relative-URL">C#</a></h3>
    </td>
        <td>
        <h3><a href="TBD/Relative-URL">F#</a></h3>
    </td>
    </tr>
    </table>
      <h3><a href="TBD/Relative-URL">Sentiment analysis</a></h3>
      <h3><a href="TBD/Relative-URL">Survival Prediction</a></h3>
  </td>
 </tr>
 <tr>
   <td>
      <h2>Multi-class classification</h2>
      <!--<img src="images/multi-class-classification-plotting.png" alt="Multi-class classification">-->
  </td>
  <td>
      <h3><a href="TBD/Relative-URL">Issues classification</a></h3>
      <h3><a href="TBD/Relative-URL">Iris flowers classification</a></h
  </td>
 </tr>
 <tr>
   <td>
      <h2>Regression</h2>
      <!--<img src="images/regression-icons.png" alt="regression icon">-->
  </td>
  <td>
      <h3><a href="TBD/Relative-URL">Price prediction</a></h3>
      <h3><a href="TBD/Relative-URL">Sales forecast</a></h3>
      <h3><a href="TBD/Relative-URL">Demand prediction</a></h3>
  </td>
 </tr>
 <tr>
   <td>
      <h2>Recommendation</h2>
      <!--<img src="images/recommendation-icon.png" alt="Recommendations icon">-->
  </td>
  <td>
      <h3><a href="TBD/Relative-URL">Product recommender</a></h3>
      <h3><a href="TBD/Relative-URL">Movie recommender</a></h3>
  </td>
 </tr>
  <tr>
   <td>
      <h2>Clustering</h2>
      <!--<img src="images/clustering-plotting.png" alt="Clustering plotting">-->
  </td>
  <td>
      <h3><a href="TBD/Relative-URL">Customer segmentation</a></h3>
      <h3><a href="TBD/Relative-URL">Clustering Iris flowers</a></h3>
  </td>
 </tr>
  <tr>
   <td>
      <h2>Anomaly detection</h2>
      <!--<img src="images/anomaly-detection-plotting.png" alt="anomaly detection chart">-->
  </td>
  <td>
      <h3><a href="TBD/Relative-URL">Fraud detection in credit cards</a></h3>
  </td>
 </tr>
  <tr>
   <td>
      <h2>Ranking</h2>
      <!--<img src="images/ranking-chart.png" alt="xxxxxx">-->
  </td>
  <td>
      <h3>Coming soon</h3>
  </td>
 </tr>
  <tr>
   <td>
      <h2>Deep Learning</h2>
      <!--<img src="images/tensorflow-logo.png" alt="TensorFlow logo">-->
  </td>
  <td>
      <h3><a href="TBD/Relative-URL">Object detection and classification with TensorFlow model</a></h3>
      <h3><a href="TBD/Relative-URL">Image style transfer with TensorFlow model</a></h3>
      <h3><a href="TBD/Relative-URL">TensorFlow model as featurizer in ML.NET</a></h3>
      <h3>ONNX model scoring - Coming soon</h3>
  </td>
 </tr>
 </table>

-------------------------------------------------------

## Visual Basic .NET samples

For VB.NET samples, check this external repo supported by the community (Kudos for Nukepayload2):
https://github.com/Nukepayload2/machinelearning-samples/tree/master/samples/visualbasic


In addition, if you would like to explore the examples directly referencing the source code of ML.NET, check out [scenario tests](https://github.com/dotnet/machinelearning/tree/master/test/Microsoft.ML.Tests/Scenarios) in [ML.NET repository](https://github.com/dotnet/machinelearning).



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
