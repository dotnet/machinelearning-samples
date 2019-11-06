

# ASP.NET Core WebAPI sample optimized for scalability and performance when running/scoring an ML.NET model


| ML.NET version | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4           | Up-to-date | ASP.NET Core 2.2 WebAPI | Single data sample | Sentiment Analysis | Binary   classification | Linear Classification |

**IMPORTANT NOTE: This sample uses an older approach by implementing all the 'plumbing' related to the PredictionEngine Object Pool. This custom implementation is no longer required since the release of the PredictionEnginePool API provided since May 2019.
Check this other sample for the preferred and much simpler approach:**

https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/ScalableMLModelOnWebAPI-IntegrationPkg

---

**This posts explains how to optimize your code when running an ML.NET model on an ASP.NET Core WebAPI service.** The code would be very similar when running it on an ASP.NET Core MVC or Razor web app, too.

For a much more detailed explanation, including design diagrams, read the following blog post:

**Detailed Blog Post** for further documentation:

[How to optimize and run ML.NET models on scalable ASP.NET Core WebAPIs or web apps](https://devblogs.microsoft.com/cesardelatorre/how-to-optimize-and-run-ml-net-models-on-scalable-asp-net-core-webapis-or-web-apps/)


# Goal

**The goal is to be able to make predictions with an ML.NET model while optimizing the executions by sharing objects across Http requests and implementing very simple code to be used by the user when predicting**, like the following line of code that you could write on any ASP.NET Core controller's method or custom service class:

```cs
SamplePrediction prediction = _modelEngine.Predict(sampleData);
```

As simple as a single line. The object _modelEngine will be injected in the controller's constructor or into your custom class. 

Internally, it is optimized so the object dependencies are cached and shared across Http requests with minimum overhead when creating those objects.

# Problem

The problem running/scoring an ML.NET model in multi-threaded applications comes when you want to do single predictions with the PredictionEngine object and you want to cache that object (i.e. as Singleton) so it is being reused by multiple Http requests (therefore it would be accessed by multiple threads). That's is a problem because **the Prediction Engine is not thread-safe** ([ML.NET issue, Nov 2018](https://github.com/dotnet/machinelearning/issues/1718)).

# Solution

## Use Object Pooling for PredictionEngine objects  

Since a PredictionEngine object cannot be singleton because it is not 'thread safe', a good solution for being able to have 'ready to use' PredictionEngine objects is to use an object pooling-based approach.

When it is necessary to work with a number of objects that are particularly expensive to instantiate and each object is only needed for a short period of time, the performance of an entire application may be adversely affected. This issue will happen if you instantiate a Prediction Engine object whenever you get an Http request.

An object pool design pattern can be very effective in such cases.

The [object pool pattern](https://en.wikipedia.org/wiki/Object_pool_pattern) is a design pattern that uses a set of initialized objects kept ready to use (a 'pool') rather than allocating and destroying them on demand. 

This solution's implementation is based on a higher-level custom class (named **MLModelEngine**) which is instantiated as singleton and creates the needed infrastructure for such an object pool solution.
