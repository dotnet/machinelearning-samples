# Movie Recommender 

| ML.NET version | API type          | Status                        | App Type    | Data sources | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.7 | Dynamic API | Up-to-date | End-End app | .csv | Movie Recommendation | Recommendation | FieldAwareFactorizationMachines |

This is an end-end sample on how you can enhance your existing ASP.NET apps with recommendations. 

## Overview



## Training 
There are multiple ways to build recommendation models for your application. Choose the best example for the task based upon your scenario. With ML.NET we support the following three recommendation scenarios, depending upon your scenario you can pick either of the three from the list below.

With ML.NET we support the following three recommendation scenarios, depending upon your scenario you can pick either of the three from the list below. 

| Scenario | Algorithm | Link To Sample
| --- | --- | --- | 
| You want to use more attributes (features) beyond UserId, ProductId and Ratings like Product Description, Product Price etc. for your recommendation engine | Field Aware Factorization Machines | This sample | 
| You have  UserId, ProductId and Ratings available to you for what users bought and rated.| Matrix Factorization | 
<a href="samples/csharp/getting-started/MatrixFactorization_MovieRecommendation" alt="Matrix Factorization based Recommendation">| 
| You only have UserId and ProductId's the user bought available to you but not ratings. This is  common in datasets from online stores where you might only have access to purchase history for your customers. With this style of recommendation you can build a recommendation engine which recommends frequently bought items. | One Class Matrix Factorization | Coming Soon | 
