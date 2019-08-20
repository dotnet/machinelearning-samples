# Azure Functions sentiment analysis sample optimized for scalability and performance when running/scoring an ML.NET model (Using dependency injection and the '.NET Core Integration Package')

This sample highlights dependency injection in conjunction with the **.NET Core Integration Package** to build a scalable serverless Azure Functions application. 


| ML.NET version | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1           | Up-to-date | Azure Functions | Single data sample | Sentiment Analysis | Binary   classification | Linear Classification |


# Goal

The goal is to be able to predict sentiment using an HTTP triggered Azure Functions serverless application.

# Problem

The problem running/scoring an ML.NET model in multi-threaded applications comes when you want to do single predictions with the PredictionEngine object and you want to cache that object (i.e. as Singleton) so it is being reused by multiple Http requests (therefore it would be accessed by multiple threads). That's is a problem because **the Prediction Engine is not thread-safe** ([ML.NET issue, Nov 2018](https://github.com/dotnet/machinelearning/issues/1718))

# Solution

## Use dependency injection in Azure Functions

Package name: **Microsoft.Azure.Functions.Extensions**

Package version: 1.0.0

To use dependency injection in Azure Functions, you have to create a class called Startup inside your Azure Functions application.

```csharp
[assembly: FunctionsStartup(typeof(Startup))]
namespace SentimentAnalysisFunctionsApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

        }
    }
}
```

## Use the new '.NET Core Integration Package' that implements Object Pooling of PredictionEngine objects for you 

Package name: **Microsoft.Extensions.ML**

Package version: 0.15.1

Basically, with this component, you register the `PredictionEnginePool` in a single line in the `Configure` methopd of the `Startup` class, like the following:

```csharp
builder.Services.AddPredictionEnginePool<SentimentData, SentimentPrediction>()
    .FromFile("MLModels/sentiment_model.zip");
```

Then you just need to need to inject the `PredictionEnginePool` inside the respective Azure Function constructor.

```csharp
private readonly PredictionEnginePool<SentimentData, SentimentPrediction> _predictionEnginePool;

public AnalyzeSentiment(PredictionEnginePool<SentimentData, SentimentPrediction> predictionEnginePool)
{
    _predictionEnginePool = predictionEnginePool;
}
```

Once injected, you can call the `Predict` method from the injected `PredictionEnginePool` inside any Azure Function:

```csharp
SentimentPrediction prediction = _predictionEnginePool.Predict(data);
```

For a much more detailed explanation of a PredictionEngine object pool comparable to the implementation done in the new '.NET Core Integration Package', including design diagrams, read the following blog post:

**Detailed Blog Post** for further documentation:

[How to optimize and run ML.NET models on scalable ASP.NET Core WebAPIs or web apps](https://devblogs.microsoft.com/cesardelatorre/how-to-optimize-and-run-ml-net-models-on-scalable-asp-net-core-webapis-or-web-apps/)

NOTE: YOU DON'T NEED TO MAKE THAT IMPLEMENTATION EXPLAINED IN THE BLOG POST.
PRECISELY THAT IS IMPLEMENTED FOR YOU IN THE '.NET INTEGRATION PACKAGE'.








