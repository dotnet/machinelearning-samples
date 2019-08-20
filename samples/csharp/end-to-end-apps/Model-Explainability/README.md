# Taxi Fare Prediction

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1           | Dynamic API | Up-to-date | Console app | .csv files | Feature importance | Regression | Sdca Regression |

This sample's goal is about Model Explainability and feature importance.

## Problem: Model Explainability

In some cases you would want to see what features the model deems important when it finds a pattern in the data. ML.NET's model explainability will help us do that. This sample shows how to use model explainability API in ML.NET to get a better idea of how important each feature is for each row of data when making a prediction. 

## Solution: Implementation and related sample app

This sample derives or is based on the [Taxi fares predictions sample](/samples/csharp/getting-started/Regression_TaxiFarePrediction) which creates a regression-based model, available in this same repo. Review that sample first if you want to understand about the ML problem (Regression) and its particular scenario (dataset and taxi fare predictions).

However, in this sample we extend that original sample and focus on [Model Explainability and Feature Importance](https://medium.com/@Zelros/a-brief-history-of-machine-learning-models-explainability-f1c3301be9dc) by using the MLNET model explainability API applied when building the model training pipeline so it gathers information about 'feature importance'.

Finally, there's a WinForms app used to show the feature importance when making sample predictions, as shown in the following screenshot:

![Feature importance](images/Feature-Importance-Chart.png)

The `mlContext.Transforms.CalculateFeatureContribution(trainedModel.LastTransformer)` method calculates scores for each feature row within the input.

### 1. Use API to extend the model pipeline to gather feature importance information

In the 'TaxiFarePrediction' project which builds the pipeline to train a regression-based model, you need to use the following API in order to gather feature importance information by appending the feature contribution calculator in the pipeline. This will be used at prediction time for model explainability. 


```CSharp
//Program.cs --> TrainModel() method

private static ITransformer TrainModel(MLContext mlContext)
{
    // Training pipeline definition code...
    // Call Fit() to train the base model...

    // Append the Feature Contribution Calculator (FCC) in the pipeline. 
    // This will be used at prediction time for explainability. 

    var fccModel = trainedModel.Append(mlContext.Transforms
                        .CalculateFeatureContribution(trainedModel.LastTransformer)
                        .Fit(dataProcessPipeline.Fit(trainingDataView).Transform(trainingDataView)));

    //Test and save the model code...
}

```

### 2. Use API to find out the the feature importance information per prediction

In the 'TaxiFarePrediction.Explainability' WinForms project, the API that extracts the feature importance per prediction is the following:

```CSharp
//Predictor.cs --> RunMultiplePredictions() method

public List<DataStructures.TaxiFarePrediction> RunMultiplePredictions(int numberOfPredictions)
{
    // Load several samples to predict with...
    //... 

    // For each prediction, get the feature contributions information
    prediction = predictionEngine.Predict(testData);
    DataStructures.TaxiFarePrediction explainedPrediction = 
        new DataStructures.TaxiFarePrediction(prediction.FareAmount, 
                                              prediction.GetFeatureContributions(
                                                                model.GetOutputSchema(inputDataForPredictions.Schema)));

}

```


## Troubleshooting

### Error when running the explainability WinForms app from Visual Studio F5

If you get the following exception:

```
DllNotFoundException: Unable to load DLL 'CpuMathNative': The specified module could not be found.
```

**Cause:** ML.NET for this particular math operation only supports x64 and VS is attempting to use `AnyCPU`. 

**Solution:** Make sure you are running the app as x64 in VS as in the following screenshot:

![Feature importance](images/cpu-arch-exception_3.png)
