# Azure Functions Sentiment Analysis Sample 

This sample highlights dependency injection in conjunction with the **.NET Core Integration Package** to build a scalable, serverless Azure Functions application. 


| ML.NET version | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1           | Up-to-date | Azure Functions | Single data sample | Sentiment Analysis | Binary   Classification | Linear Classification |

For a detailed explanation of how to build this application, see the accompanying [how-to guide](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/serve-model-serverless-azure-functions-ml-net) on the Microsoft Docs site.

# Goal

The goal is to be able to predict sentiment using an HTTP triggered Azure Functions serverless application.

# Problem

The problem with running/scoring an ML.NET model in multi-threaded applications comes when you want to do single predictions with the PredictionEngine object and you want to cache that object (i.e. as Singleton) so it is being reused by multiple Http requests (therefore it would be accessed by multiple threads). This is a problem because **the Prediction Engine is not thread-safe** ([ML.NET issue, Nov 2018](https://github.com/dotnet/machinelearning/issues/1718))

# Solution

This is an Azure Functions application optimized for scalability and performance when running/scoring an ML.NET model. It uses dependency injection and the .NET Core Integration Package.

## Use dependency injection in Azure Functions

**Package name:** Microsoft.Azure.Functions.Extensions

**Package version:** 1.0.0

To use dependency injection in Azure Functions, you need to create a class called `Startup` inside your Azure Functions application:

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

## Use the new '.NET Core Integration Package'

**Package name:** Microsoft.Extensions.ML

**Package version:** 0.15.1

The new **.NET Core Integration Package** implements Object Pooling of PredictionEngine objects for you.

Basically, with this component, you register the `PredictionEnginePool` in a single line in the `Configure` method of the `Startup` class, like the following:

```csharp
 builder.Services.AddPredictionEnginePool<SentimentData, SentimentPrediction>()
	.FromFile(modelName: "SentimentAnalysisModel", filePath:"MLModels/sentiment_model.zip", watchForChanges: true);
```

In the example above, by setting the `watchForChanges` parameter to `true`, the `PredictionEnginePool` starts a `FileSystemWatcher` that listens to the file system change notifications and raises events when there is a change to the file. This prompts the `PredictionEnginePool` to automatically reload the model without having to redeploy the application. The model is also given a name using the `modelName` parameter. In the event you have multiple models hosted in your application, this is a way of referencing them. 

Then you just need to need to inject the `PredictionEnginePool` inside the respective Azure Function constructor:

```csharp
private readonly PredictionEnginePool<SentimentData, SentimentPrediction> _predictionEnginePool;

public AnalyzeSentiment(PredictionEnginePool<SentimentData, SentimentPrediction> predictionEnginePool)
{
    _predictionEnginePool = predictionEnginePool;
}
```

Once injected, you can call the `Predict` method from the injected `PredictionEnginePool` inside any Azure Function:

```csharp
SentimentPrediction prediction = _predictionEnginePool.Predict(modelName: "SentimentAnalysisModel", example: data);
```

For a much more detailed explanation of a PredictionEngine object pool comparable to the implementation done in the '.NET Core Integration Package', including design diagrams, read the following blog post:

[How to optimize and run ML.NET models on scalable ASP.NET Core WebAPIs or web apps](https://devblogs.microsoft.com/cesardelatorre/how-to-optimize-and-run-ml-net-models-on-scalable-asp-net-core-webapis-or-web-apps/)

**NOTE:** You don't need to make the implementation explained in the blog post. Precisely that functionality is implemented for you in the .NET Integration Package. 

## Test the application locally

1. Run the application
2. Open PowerShell and enter the code into the prompt where PORT is the port your application is running on. Typically the port is 7071.

```csharp
Invoke-RestMethod "http://localhost:<PORT>/api/AnalyzeSentiment" -Method Post -Body (@{SentimentText="This is a very bad steak"} | ConvertTo-Json) -ContentType "application/json"
```

If successful, the output should look similar to the text below:

```text
Negative
```