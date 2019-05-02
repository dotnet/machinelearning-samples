# Advanced Taxi Fare Prediction

## Automated ML
Automated machine learning (AutoML) automates the end-to-end process of applying machine learning to real-world problems. Given a dataset, AutoML iterates over different data featurizations, machine learning algorithms, hyperparamters, etc. to select the best model based on training scores.

## Advance Sample
The Advanced Taxi Fare Prediction sample explores the available experiment setting configurations given by automated ML.

### Column Inferencing

With ML.NET's inferencing, you can have the ML.NET infer the column type automatically. It can recognize the type of the data in each of the columns in your dataset.

```C#
ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(TrainDataPath, LabelColumnName, groupColumns: false);
```

### Data loading

```C#
TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);

TrainDataView = textLoader.Load(TrainDataPath);
TestDataView = textLoader.Load(TestDataPath);
```

## Machine Learning Experiment

### Pre-featurize

Build a pre-featurizer for use in the AutoML experiment. Internally, AutoML uses one or more train/validation data splits to evaluate the models it produces. The pre-featurizer is fit only on the training data split to produce a trained transform. Then, the trained transform is applied to both the train and validation data splits.

```C#
IEstimator<ITransformer> preFeaturizer = mlContext.Transforms.Conversion.MapValue("is_cash",
    new[] { new KeyValuePair<string, bool>("CSH", true) }, "payment_type");
```

### Customize Infered Columns
 AutoML allows you to customize column information returned by InferColumns API.

```C#
ColumnInformation columnInformation = columnInference.ColumnInformation;
columnInformation.CategoricalColumnNames.Remove("payment_type");
columnInformation.IgnoredColumnNames.Add("payment_type");
```

### Experimentation Settings

Create an AutoML experiment by specifying experiment settings. Currently you can create 3 kinds of experiments. Binary Classification, Multiclass Classification & Regression. You can specify how long the experiment should run and what metric it should optimize. You can also explore what learners are available by looking at experimentSettings.Trainers collection and change it if need be. You can also set a progress handler that will receive notifications as and when new models are trained. You can use a cancellation token that lets you cancel the experiment before it is scheduled to finish. The following code shows how to specify settings for a regression experiment.

```C#
var experimentSettings = new RegressionExperimentSettings();

experimentSettings.MaxExperimentTimeInSeconds = 3600;
experimentSettings.CancellationToken = cts.Token;

// Set the metric that AutoML will try to optimize over the course of the experiment.
experimentSettings.OptimizingMetric = RegressionMetric.RootMeanSquaredError;

// Set the cache directory to null.
// This will cause all models produced by AutoML to be kept in memory 
// instead of written to disk after each run, as AutoML is training.
experimentSettings.CacheDirectory = null;

// Don't use LbfgsPoissonRegression and OnlineGradientDescent trainers during this experiment.
experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression);
experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent);

// Cancel experiment after the user presses any key
CancelExperimentAfterAnyKeyPress(cts);
```

### Create an experiment
The simplest way to create an experiment is by specifying MaxExperimentTime.

```C#
var experiment = mlContext.Auto().CreateRegressionExperiment(60);
```

However if you want to specify more settings as shown above section, then you can use the following overload.

```C#
var experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings);
```

### Execute the experiment

Once you have the experiment set with the parameters, you execute the experiment. Executing essentially triggers data preprocessing, a learning algorithm and hyperparameters. AutoML will continue to generate combinations of learning algorithms and hyperparameters and keeps training machine learning models until the MaxExperimentTimeInSeconds is reached.

```C#            
 ExperimentResult<RegressionMetrics> experimentResult = experiment.Execute(TrainSmallDataView, columnInformation, preFeaturizer, progressHandler);
```

### Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`taxi-fare-test.csv`). This dataset also contains known fares. `Regression.Evaluate()` calculates the difference between known fares and values predicted by the model in various metrics.

```C#
IDataView predictions = model.Transform(TestDataView);
var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
ConsoleHelper.PrintRegressionMetrics(trainerName, metrics);
```

### Consume model
```C#
var taxiTripSample = new TaxiTrip()
{
    VendorId = "VTS",
    RateCode = 1,
    PassengerCount = 1,
    TripTime = 1140,
    TripDistance = 3.75f,
    PaymentType = "CRD",
    FareAmount = 0 // To predict. Actual/Observed = 15.5
};

ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

// Create prediction engine related to the loaded trained model
var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

// Score
var resultprediction = predEngine.Predict(taxiTripSample);
```