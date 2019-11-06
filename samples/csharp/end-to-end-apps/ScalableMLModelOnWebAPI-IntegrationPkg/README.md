

# ASP.NET Core WebAPI sample optimized for scalability and performance when running/scoring an ML.NET model (Using the new '.NET Core Integration Package')


| ML.NET version | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4           | Up-to-date | ASP.NET Core 2.2 WebAPI | Single data sample | Sentiment Analysis | Binary   classification | Linear Classification |


**This posts explains how to optimize your code when running an ML.NET model on an ASP.NET Core WebAPI service.** The code would be very similar when running it on an ASP.NET Core MVC or Razor web app, too.

This code has been very much simplified by **using a new '.NET Core Integration Package'** that the .NET team has created (in experimental state) which implements all the 'plumbing' for doing object pooling for the PredictionEngine.


# Goal

**The goal is to be able to make predictions with an ML.NET model while optimizing the executions by sharing the needed ML.NET scoring objects across Http requests and implementing very simple code to be used by the user when predicting**, like the following line of code that you could write on any ASP.NET Core controller's method or custom service class:

```cs
SamplePrediction prediction = _predictionEnginePool.Predict(sampleData);
```

As simple as a single line. The object _predictionEnginePool will be injected in the controller's constructor or into your custom class. 

Internally, it is optimized so the object dependencies are cached and shared across Http requests with minimum overhead when creating those objects.

# Problem

The problem running/scoring an ML.NET model in multi-threaded applications comes when you want to do single predictions with the PredictionEngine object and you want to cache that object (i.e. as Singleton) so it is being reused by multiple Http requests (therefore it would be accessed by multiple threads). That's is a problem because **the Prediction Engine is not thread-safe** ([ML.NET issue, Nov 2018](https://github.com/dotnet/machinelearning/issues/1718)).

# Solution

## Use the new '.NET Core Integration Package' that implements Object Pooling of PredictionEngine objects for you 

**'.NET Core Integration Package' NuGet feed**

NuGet package name: **Microsoft.Extensions.ML**

Preview NuGet feed at **MyGet: https://dotnet.myget.org/F/dotnet-core/api/v3/index.json**

Basically, with this component, you inject/use the PredictionEngine object pooling in a single line in your Startup.cs, like the following:

```CSharp
services.AddPredictionEnginePool<SampleObservation, SamplePrediction>()
        .FromFile(Configuration["MLModel:MLModelFilePath"]);

```

Then you just need to call the Predict() function from the injected PredictionEnginePool, like the following code you can implement on any controller:

```CSharp

//Predict sentiment
SamplePrediction prediction = _predictionEnginePool.Predict(sampleData);

```

It is that simple.

For a much more detailed explanation of a PredictionEngine Object Pool comparable to the implementation done in the new '.NET Core Integration Package', including design diagrams, read the following blog post:

**Detailed Blog Post** for further background documentation:

[How to optimize and run ML.NET models on scalable ASP.NET Core WebAPIs or web apps](https://devblogs.microsoft.com/cesardelatorre/how-to-optimize-and-run-ml-net-models-on-scalable-asp-net-core-webapis-or-web-apps/)

NOTE: YOU DON'T NEED TO MAKE THAT IMPLEMENTATION EXPLAINED IN THE BLOG POST.
PRECISELY THAT IS IMPLEMENTED FOR YOU IN THE '.NET INTEGRATION PACKAGE'.
