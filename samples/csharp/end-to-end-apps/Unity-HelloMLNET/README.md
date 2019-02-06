# Loading ML.NET Models in Unity 

## Overview

This Unity Package shows you how to load a ML.NET model into your Unity Game using a simple form based UI application.

![Alt Text](https://github.com/dotnet/machinelearning-samples/blob/master/images/HelloML.NET_Unity.png)

## Features
* Unity Package File  
    * Simple Unity Package File which has a simple form based UI component which predicts Toxicity of input sentiments using a ML.NET model     

## Known Workarounds
* Create a plugins folder in assets and add core ML.NET Nuget binaries along with all nested
  dependencies (e.g. Sytem.Memory) and inherent native dependencies (e.g. CPUMathNative)

* Use lower-level API to score ML.NET models and avoid the high level convinience APIs e.g.
  PredictionEngine as they currently make use of reflection emit which throws up.

```CSharp

        ITransformer loadedModel;
        using (var stream = new FileStream(".\\Assets\\Models\\model.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            loadedModel = ctx.Model.Load(stream);
        }
        Debug.Log("Model Loaded");


        var SentimentText = SentimentField.text;

        ArrayDataViewBuilder arrayDataViewBuilder = new ArrayDataViewBuilder(ctx);
        arrayDataViewBuilder.AddColumn("Text", SentimentText);
        var testissues = arrayDataViewBuilder.GetDataView(rowCount: 1);

        var transformedIssues = loadedModel.Transform(testissues);

        var cursor = transformedIssues.GetRowCursor((i) => true);
        ValueGetter<bool> results = cursor.GetGetter<bool>(transformedIssues.Schema.GetColumnOrNull("PredictedLabel").Value.Index);

        while (cursor.MoveNext())
        {
            bool result = false;
            results(ref result);
            OutputSentiment.text = "Predicted Label for (" + SentimentText + ") is: " + result;
            Debug.Log("Predicted Label for (" + SentimentText + ") is: " + result);
        }
```

* Target .NET 4.x API compatibility level (default for 2018.3.4f1)
