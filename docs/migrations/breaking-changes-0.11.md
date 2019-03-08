# Breaking changes from ML.NET v0.10 to v0.11 impacting ML.NET Samples.md 

1. Namespace **Microsoft.ML.Core** is been removed 

 

    **Error:**   CS0234    The type or namespace name 'Core' does not exist in the namespace 'Microsoft.ML' (are you missing an assembly reference?)    

    **Fix:** Remove the name space **Microsoft.ML.Core**.

    

2. The name of datatypes have been modified in enum **DataKind**. 

 

    **Error:**     CS0117        'DataKind' does not contain a definition for 'BL'         

    **Error:**    CS0117        'DataKind' does not contain a definition for 'R4' 

    **Fix:**  see the changes below.

    **Datakind.BL   - DataKind.Boolean** 

    **DataKInd.R4 - DataKind.Single**

    **DataKind.TX  -  DataKind.String** 

3. DataLoading using TextLoader has been changed. No need to create specific **Arguments** type in TextLoader pass the arguments directly in API method call.

    **Error:**      CS0426        The type name 'Arguments' does not exist in the type 'TextLoader'         

    **Error:**     CS1729        'TextLoader' does not contain a constructor that takes 2 arguments 

    Fix: Use **mlContext.Data.CreateTextLoader(arguments,seperatorChar,hasHeader)**  

4. Object Creation of MachineLearning tasks like BinaryClassification, Multiclass Classifications are wrapped inside MLContext object. (For example BinaryClassification object is created using MLContext.BinaryClassification property). 

    **Error:**        CS1729        'BinaryClassificationCatalog' does not contain a constructor that takes 1 arguments 

    **Fix:** use **BinaryClassificationCatalog classification = mlContext.BinaryClassification** statement to create BianryClassification. 

 

5. Named Arguments are renamed.  

    **Error:** CS1739        The best overload for 'KMeans' does not have a parameter named 'featureColumn'         

    **Fix:** Rename **featureColumn** as **featureColumnName** 

 

6. Creation of OneHotEncodingEstimator object is wrapped inside MLContext.Transformers class.

    **Error:**        CS1729        'OneHotEncodingEstimator' does not contain a constructor that takes 2 arguments         

    **Fix:** Use **mlContext.Transforms.Categorical.OneHotEncoding()** instead of **new  OneHotEncodingEstimator()** syntax 

 

7. Creation of **PrincipalComponentAnalysisEstimator** object moved to MLContext class. 

    **Error:**        CS1729        'PrincipalComponentAnalysisEstimator' does not contain a constructor that takes 4 arguments        CustomerSegmentation.Train         

    **Fix:** Use **mlContext.Transforms.Projection.ProjectToPrincipalComponents()** instead of **new PrincipalComponentAnalysisEstimator()** 

 

8. Replace **suportSparse** with **allowSparse** in ReadFromTextFilemethod(). 

 

9. Change Method Signature. 

    **Error:**        CS1503        Argument 2: cannot convert from 'Microsoft.ML.TrainCatalogBase.CrossValidationResult<Microsoft.ML.Data.RegressionMetrics>[]' to '(Microsoft.ML.Data.RegressionMetrics metrics, Microsoft.ML.ITransformer model, Microsoft.ML.TrainCatalogBase.CrossValidationResult<Microsoft.ML.Data.RegressionMetrics>)[]' 

    

    **Fix:** Change the method signature from  

    ```
    PrintRegressionFoldsAverageMetrics(string algorithmName, 

                                                                (RegressionMetrics metrics, 

                                                                ITransformer model, 

                                                                CrossValidationResult<RegressionMetrics>)[] crossValidationResults 

                                                                ) 
    ```
    To 

  

    ```
    PrintRegressionFoldsAverageMetrics(string algorithmName,CrossValidationResult<RegressionMetrics>[] crossValidationResults) 
    ```

10. Namespaces have been changed.  

    **Error:**       CS0234        The type or namespace name 'Normalizers' does not exist in the namespace 'Microsoft.ML.Transforms' (are you missing an assembly reference?)         

    **Fix:**  Remove the namespace using statement **Microsoft.ML.Transforms.Normalizers.NormalizingEstimator** 

  

11. Remove Uncecessary name spaces .

    **Error:**        CS0122        'Categorical' is inaccessible due to its protection level    

    **Fix:** Remove namespace **Microsoft.ML.Transforms.Categorical** 

  

12. Namespaces are changed. 

    **Error:**        CS0103        The name 'NormalizerMode' does not exist in the current context     

    **Fix:**  Add using **static Microsoft.ML.Transforms.NormalizingEstimator** 

 