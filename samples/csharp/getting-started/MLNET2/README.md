# ML.NET 2.0 Samples

This directory contains samples for ML.NET 2.0.

## Data

The samples in this directory use the following datasets:

- [Taxi Fare](https://raw.githubusercontent.com/luisquintanilla/machinelearning-samples/main/datasets/taxi-fare-train.csv)
- Luna
- [Yelp Reviews](http://archive.ics.uci.edu/ml/machine-learning-databases/00331/sentiment%20labelled%20sentences.zip)
- [Home Depot](https://www.kaggle.com/competitions/home-depot-product-search-relevance/data)

## Samples

### AutoML

- **AutoMLQuickStart** - C# console application that shows how to get started with the AutoML API.
- **AutoMLAdvanced** - C# console application that shows the following concepts:
  - Modifying column inference results
  - Excluding trainers
  - Configuring monitoring
  - Choosing tuners
  - Cancelling experiments
- **AutoMLEstimators** - C# console application that shows how to:
  - Customize search spaces
  - Create sweepable estimators
- **AutoMLTrialRunner** - C# console application that shows how to create your own trial runner.

### Natural Language Processing (NLP)

- **TextClassification** - C# console app that shows how to use the [Text Classification API](https://devblogs.microsoft.com/dotnet/introducing-the-ml-dotnet-text-classification-api-preview/). Model is trained using [Model Builder](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet/model-builder).
- **SentenceSimilarity** - C# console app that shows how to use the Sentence Similarity API. Like the Text Classification API, the Sentence Similarity API uses a NAS-BERT transformer-based deep learning model built with [TorchSharp](https://github.com/dotnet/torchsharp) to compare how similar two pieces of text are.