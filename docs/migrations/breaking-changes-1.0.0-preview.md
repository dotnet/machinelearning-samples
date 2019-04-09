# Breaking changes from ML.NET v0.11 to v1.0.0-preview impacting ML.NET Samples.md 

0. DefaultColumnNames is now internal, not for user's usage. You need to use the column names between "".
See PR and discussion in this PR:
https://github.com/dotnet/machinelearning/pull/2842 
If you have any comment, do so in that PR.

1. Error    CS0234    The type or namespace name 'Data' does not exist in the namespace 'Microsoft' (are you missing an assembly reference?)    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\common\ConsoleHelper.cs    6    Active 

 

Fix: Remove the namespace  using Microsoft.Data.DataView; 

 

2. Error    CS0246    The type or namespace name 'MultiClassClassifierMetrics' could not be found (are you missing a using directive or an assembly reference?)    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\common\ConsoleHelper.cs    59    Active 

 

 

mlContext.Regression.Trainers.FastTree() is not accessible  

 

Fix: add nuget package for Microsoft.ML.FastTree 

 

3. Error: Severity    Code    Description    Project    File    Line    Suppression State 

 

PoissonRegression has been changed as LbfgsPoissonRegression 

4. Error    CS1061    'RegressionCatalog.RegressionTrainers' does not contain a definition for 'PoissonRegression' and no accessible extension method 'PoissonRegression' accepting a first argument of type 'RegressionCatalog.RegressionTrainers' could be found (are you missing a using directive or an assembly reference?)    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Regression_BikeSharingDemand\BikeSharingDemand\BikeSharingDemandConsoleApp\Program.cs    51    Active 

Fix: change PoissonRegression to  LbfgsPoissonRegression 

5. We need to pass schema of training data  while Saving model to file using  mlContext.Model.Save() method 

Error: Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1501    No overload for method 'Save' takes 2 arguments    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Regression_BikeSharingDemand\BikeSharingDemand\BikeSharingDemandConsoleApp\Program.cs    77    Active 

Fix: use mlContext.Model.Save(trainedModel, trainingDataView.Schema, fs); 

6. mlContext.Model.Load() method signature is changed such that it accepts an out variable  I.e when you load the model, it  outputs schema of training data in out variable. 

Error: 

Severity    Code    Description    Project    File    Line    Suppression State 

 Error    CS1501    No overload for method 'Load' takes 1 arguments    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Regression_BikeSharingDemand\BikeSharingDemand\BikeSharingDemandConsoleApp\Program.cs    95    Active 

Fix: change mlContext.Model.Load(stream) to mlContext.Model.Load(stream, out  intpuSchema_variable) 

7. CreatePredictionEngineFunction moved from ITransformer to MLContext. 

Error  :  CS1061    'ITransformer' does not contain a definition for 'CreatePredictionEngine' and no accessible extension method 'CreatePredictionEngine' accepting a first argument of type 'ITransformer' could be found (are you missing a using directive or an assembly reference?)    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Regression_BikeSharingDemand\BikeSharingDemand\BikeSharingDemandConsoleApp\Program.cs    99    Active 

Fix: change trainedModel.PredictionEngine(mlContext) to  mlContext.Model.CreatePredictionEngine<DemandObservation, DemandPrediction>(trainedModel); 

8. The property names of classes RegressionMetrics, CalibratedBinaryClassificationMetrics, ClusteringMetrics , MulticlassClassificationMetrics are changed to have more readable name as below. 

LossFn  -  LossFunction 

AUC – AreadUnderCurve 

AUPRC - AreaUnderPrecisionRecallCurve 

L1-  MeanAbsoluteError 

L2 - MeanSquaredError 

RMS - RootMeanSquaredError 

AvgMinScore –AverageDistance 

Dbi - DaviesBouldinIndex 

AccuracyMicro - MicroAccuracy 

AccuracyMacro - 
 

9. MlContext object is removed form method signature I.e IDataView.GetColumn<t>(mlContext, columnName) 
 
Error    CS1501    No overload for method 'GetColumn' takes 2 arguments    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\common\ConsoleHelper.cs    207    Active 

Fix: remove mlContext from method call IDataView.GetColumn<t>() 

10. Change the name from MultiClassClassifierMetrics to MulticlassClassificationMetrics 

Error: Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS0246    The type or namespace name 'MultiClassClassifierMetrics' could not be found (are you missing a using directive or an assembly reference?)    BikeSharingDemand    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\common\ConsoleHelper.cs    58    Active 

11. TrainTestSplit should be inside MLContext.Data 

Error    CS0246    The type or namespace name 'TrainTestData' could not be found (are you missing a using directive or an assembly reference?)    CreditCardFraudDetection.Trainer    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\BinaryClassification_CreditCardFraudDetection\CreditCardFraudDetection.Trainer\Program.cs    65    Active 
 
Fix: change mlContext.BinaryClassification.TrainTestSplit() to mlContext.Data.TrainTestSplit() 

12. The class TrainTestData is been moved from TrainCatalogBase to DataOperationsCatalog 

Error    CS0426    The type name 'TrainTestData' does not exist in the type 'TrainCatalogBase'    Clustering_Iris    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Clustering_Iris\IrisClustering\IrisClusteringConsoleApp\Program.cs    45    Active 

13. Error    CS0103    The name 'NormalizerMode' does not exist in the current context    CreditCardFraudDetection.Trainer    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\BinaryClassification_CreditCardFraudDetection\CreditCardFraudDetection.Trainer\Program.cs    102    Active 

Fix: change to NormalizationCatalog.NormalizeMeanVariance 

14. install nuget package  Microsoft.ML.FastTree v.1.0.0-preview
Error    CS1061    'BinaryClassificationCatalog.BinaryClassificationTrainers' does not contain a definition for 'FastTree' and no accessible extension method 'FastTree' accepting a first argument of type 'BinaryClassificationCatalog.BinaryClassificationTrainers' could be found (are you missing a using directive or an assembly reference?)    CreditCardFraudDetection.Trainer    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\BinaryClassification_CreditCardFraudDetection\CreditCardFraudDetection.Trainer\Program.cs    109    Active 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1739    The best overload for 'Evaluate' does not have a parameter named 'label'    CreditCardFraudDetection.Trainer    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\BinaryClassification_CreditCardFraudDetection\CreditCardFraudDetection.Trainer\Program.cs    135    Active 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1061    'TransformsCatalog' does not contain a definition for 'Projection' and no accessible extension method 'Projection' accepting a first argument of type 'TransformsCatalog' could be found (are you missing a using directive or an assembly reference?)    CustomerSegmentation.Train    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Clustering_CustomerSegmentation\CustomerSegmentation.Train\Program.cs    42    Active 

 

Fix: Remove Projection from API call mlContext.Transforms.Projection.ProjectToPrincipalComponents()   I.e use   mlContext.Transforms.ProjectToPrincipalComponents() 

OneHotEncoding Method signature is changed  which is simple now. 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS0122    'OneHotEncodingEstimator.ColumnOptions' is inaccessible due to its protection level    CustomerSegmentation.Train    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Clustering_CustomerSegmentation\CustomerSegmentation.Train\Program.cs    44    Active 

Fix:  call the API similar to below. 

 

mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "LastNameKey", inputColumnName: nameof(PivotData.LastName), OneHotEncodingEstimator.OutputKind.Indicator) 

 

 

Parameter names are changed 

NumClusters - numberOfClusters 

 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1061    'BinaryClassificationCatalog.BinaryClassificationTrainers' does not contain a definition for 'FastTree' and no accessible extension method 'FastTree' accepting a first argument of type 'BinaryClassificationCatalog.BinaryClassificationTrainers' could be found (are you missing a using directive or an assembly reference?)    HeartDiseasePredictionConsoleApp    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\BinaryClassification_HeartDiseasePrediction\HeartDiseasePrediction-Solution\Program.cs    43    Active 

Fix: Install Nuget package Microsoft.ML.FastTree 

 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1061    'MulticlassClassificationCatalog.MulticlassClassificationTrainers' does not contain a definition for 'StochasticDualCoordinateAscent' and no accessible extension method 'StochasticDualCoordinateAscent' accepting a first argument of type 'MulticlassClassificationCatalog.MulticlassClassificationTrainers' could be found (are you missing a using directive or an assembly reference?)    MulticlassClassification_Iris    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\MulticlassClassification_Iris\IrisClassification\IrisClassificationConsoleApp\Program.cs    59    Active 

 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1503    Argument 2: cannot convert from 'System.Collections.Generic.IReadOnlyList<Microsoft.ML.TrainCatalogBase.CrossValidationResult<Microsoft.ML.Data.MulticlassClassificationMetrics>>' to 'System.Collections.Generic.IReadOnlyList<Microsoft.ML.TrainCatalogBase.CrossValidationResult<Microsoft.ML.Data.MulticlassClassificationMetrics>>[]'    GitHubLabeler    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\end-to-end-apps\MulticlassClassification-GitHubLabeler\GitHubLabeler\GitHubLabelerConsoleApp\Program.cs    112    Active 

 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1061    'RegressionMetrics' does not contain a definition for 'Rms' and no accessible extension method 'Rms' accepting a first argument of type 'RegressionMetrics' could be found (are you missing a using directive or an assembly reference?)    MovieRecommendation    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\MatrixFactorization_MovieRecommendation\MovieRecommendation\Program.cs    63    Active 

 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS0104    'IDataView' is an ambiguous reference between 'Microsoft.Data.DataView.IDataView' and 'Microsoft.ML.IDataView'    PowerAnomalyDetection    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\TimeSeries_PowerAnomalyDetection\PowerAnomalyDetection\Program.cs    33    Active 

Fix:  

 

Removed Microsoft.Data.DataView package. 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS0234    The type or namespace name 'Data' does not exist in the namespace 'Microsoft' (are you missing an assembly reference?)    PowerAnomalyDetection    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\TimeSeries_PowerAnomalyDetection\PowerAnomalyDetection\Program.cs    5    Active 

 

C  

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS0246    The type or namespace name 'LoadColumnAttribute' could not be found (are you missing a using directive or an assembly reference?)    PowerAnomalyDetection    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\TimeSeries_PowerAnomalyDetection\PowerAnomalyDetection\Program.cs    12    Active 

Fix: Include Microsoft.ML.Data name space 

 

C 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS1501    No overload for method 'GetColumn' takes 2 arguments    TaxiFarePrediction    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\Regression_TaxiFarePrediction\TaxiFarePrediction\TaxiFarePredictionConsoleApp\Program.cs    57    Active 

 

Fix: remove mlcontext parameter in  var cnt2 = trainingDataView.GetColumn<float>(mlContext, nameof(TaxiTrip.FareAmount)).Count(); 

 

Severity    Code    Description    Project    File    Line    Suppression State 

Error    CS0234    The type or namespace name 'ImageAnalytics' does not exist in the namespace 'Microsoft.ML' (are you missing an assembly reference?)    ImageClassification.Score    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\DeepLearning_ImageClassification_TensorFlow\ImageClassification\ModelScorer\TFModelScorer.cs    12    Active 

Error    CS1061    'DataOperationsCatalog' does not contain a definition for 'ReadFromTextFile' and no accessible extension method 'ReadFromTextFile' accepting a first argument of type 'DataOperationsCatalog' could be found (are you missing a using directive or an assembly reference?)    ImageClassification.Score    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\DeepLearning_ImageClassification_TensorFlow\ImageClassification\ModelScorer\TFModelScorer.cs    70    Active 

Error    CS1739    The best overload for 'LoadImages' does not have a parameter named 'columns'    ImageClassification.Score    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\DeepLearning_ImageClassification_TensorFlow\ImageClassification\ModelScorer\TFModelScorer.cs    72    Active 

Error    CS1061    'TransformsCatalog' does not contain a definition for 'Resize' and no accessible extension method 'Resize' accepting a first argument of type 'TransformsCatalog' could be found (are you missing a using directive or an assembly reference?)    ImageClassification.Score    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\DeepLearning_ImageClassification_TensorFlow\ImageClassification\ModelScorer\TFModelScorer.cs    73    Active 

Error    CS0246    The type or namespace name 'ImagePixelExtractorTransformer' could not be found (are you missing a using directive or an assembly reference?)    ImageClassification.Score    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\DeepLearning_ImageClassification_TensorFlow\ImageClassification\ModelScorer\TFModelScorer.cs    74    Active 

Error    CS1061    'TransformsCatalog' does not contain a definition for 'ScoreTensorFlowModel' and no accessible extension method 'ScoreTensorFlowModel' accepting a first argument of type 'TransformsCatalog' could be found (are you missing a using directive or an assembly reference?)    ImageClassification.Score    C:\GitRepos\machinelearning-samples-v1.0.0-Preview\samples\csharp\getting-started\DeepLearning_ImageClassification_TensorFlow\ImageClassification\ModelScorer\TFModelScorer.cs    75    Active 

 

 

 

 

 

 
