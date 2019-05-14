# Heart disease prediction

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.0.0           | Dynamic API | Up-to-date | Console app | .txt files | Heart disease classification | Binary classification | FastTree |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to predict type of heart disease. In the world of machine learning, this type of prediction is known as **binary classification**.

## Dataset
The dataset used is this: [UCI Heart disease] (https://archive.ics.uci.edu/ml/datasets/heart+Disease)
This database contains 76 attributes, but all published experiments refer to using a subset of 14 of them. 

Citation for this dataset is available at [DataSets-Citation](./HeartDiseaseDetection/Data/DATASETS-CITATION.txt)

## Problem
This problem is centered around predicting the presence of hearth disease based on 14 attributes. To solve this problem, we will build an ML model that takes as inputs 4 parameters: 
Attribute Information:

* (age) - Age
* (sex) -  (1 = male; 0 = female) 
* (cp)  chest pain type  -- Value 1: typical angina  -- Value 2: atypical angina  -- Value 3: non-anginal pain -- Value 4: asymptomatic 
* (trestbps) - resting blood pressure (in mm Hg on admission to the hospital) 
* (chol) - serum cholestoral in mg/dl 
* (fbs)  -  (fasting blood sugar > 120 mg/dl) (1 = true; 0 = false) 
* (restecg) - esting electrocardiographic results -- Value 0: normal -- Value 1: having ST-T wave abnormality (T wave inversions and/or ST elevation or depression of > 0.05 mV) -- Value 2: showing probable or definite left ventricular hypertrophy by Estes' criteria 
* (thalach) - maximum heart rate achieved 
* (exang) - exercise induced angina (1 = yes; 0 = no) 
* (oldpeak) - ST depression induced by exercise relative to rest 
* (slope) - the slope of the peak exercise ST segment -- Value 1: upsloping -- Value 2: flat -- Value 3: downsloping  
* (ca) - number of major vessels (0-3) colored by flourosopy
* (thal) - 3 = normal; 6 = fixed defect; 7 = reversable defect 
* (num) (the predicted attribute) diagnosis of heart disease (angiographic disease status) -- Value 0: < 50% diameter narrowing -- Value 1: > 50% diameter narrowing (in any major vessel: attributes 59 through 68 are vessels) 

and predicts the presence of heart disease in the patient with integer values from 0 to 4:
Experiments with the Cleveland database (dataset used for this example) have concentrated on simply attempting to distinguish presence (value 1) from absence (value 0). 


## ML task - Binary classification
The generalized problem of **binary classification** is to classify items into items into one of the two classes (classifying items into more than two classes is called **multiclass classification**).

* predict if an insurance claim is valid or not.
* predict if a plane will be delayed or will arrive on time.
* predict if a face ID (photo) belongs to the owner of a device.

The common feature for all those examples is that the parameter we want to predict can take only one of two values. In other words, this value is represented by `boolean` type.

## Solution
To solve this problem, first we will build an ML model. Then we will train the model on existing data, evaluate how good it is, and lastly we'll consume the model to predict a sentiment for new reviews.

![Build -> Train -> Evaluate -> Consume](../shared_content/modelpipeline.png)

### 1. Build model

Building a model includes: 

* Define the data's schema maped to the datasets to load (`HeartTraining.tsv` and `HeartTest.csv`) with a TextLoader.

* Create an Estimator by concatenateing the features into single 'features' column

* Choosing a trainer/learning algorithm (such as `FastTree`) to train the model with. 

The initial code is similar to the following:

```CSharp
// STEP 1: Common data loading configuration
var trainingDataView = mlContext.Data.LoadFromTextFile<HeartData>(TrainDataPath, hasHeader: true, separatorChar: ';');
var testDataView = mlContext.Data.LoadFromTextFile<HeartData>(TestDataPath, hasHeader: true, separatorChar: ';');

// STEP 2: Concatenate the features and set the training algorithm
var pipeline = mlContext.Transforms.Concatenate("Features", "Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca", "Thal")
                .Append(mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));                         

```

### 2. Train model
Training the model is a process of running the chosen algorithm on a training data (with known sentiment values) to tune the parameters of the model. It is implemented in the `Fit()` method from the Estimator object. 

To perform training you need to call the `Fit()` method while providing the training dataset in a DataView object.

```CSharp
ITransformer trainedModel = pipeline.Fit(trainingDataView);
```

Note that ML.NET works with data with a lazy-load approach, so in reality no data is really loaded in memory until you actually call the method .Fit().

### 3. Evaluate model

We need this step to conclude how accurate our model operates on new data. To do so, the model from the previous step is run against another dataset that was not used in training (`wikipedia-detox-250-line-test.tsv`). This dataset also contains known sentiments. 

`Evaluate()` compares the predicted values for the test dataset and produces various metrics, such as accuracy, you can explore.

```CSharp
var predictions = trainedModel.Transform(testDataView);
var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
```

### 4. Consume model

After the model is trained, you can use the `Predict()` API to predict if heart disease is present for a list of heart data set. 

```CSharp
// Create prediction engine related to the loaded trained model
var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(trainedModel);                   

foreach (var heartData in HeartSampleData.heartDataList)
            {
                var prediction = predictionEngine.Predict(heartData);

                Console.WriteLine($"=============== Single Prediction  ===============");
                Console.WriteLine($"Age: {heartData.Age} ");
                Console.WriteLine($"Sex: {heartData.Sex} ");
                Console.WriteLine($"Cp: {heartData.Cp} ");
                Console.WriteLine($"TrestBps: {heartData.TrestBps} ");
                Console.WriteLine($"Chol: {heartData.Chol} ");
                Console.WriteLine($"Fbs: {heartData.Fbs} ");
                Console.WriteLine($"RestEcg: {heartData.RestEcg} ");
                Console.WriteLine($"Thalac: {heartData.Thalac} ");
                Console.WriteLine($"Exang: {heartData.Exang} ");
                Console.WriteLine($"OldPeak: {heartData.OldPeak} ");
                Console.WriteLine($"Slope: {heartData.Slope} ");
                Console.WriteLine($"Ca: {heartData.Ca} ");
                Console.WriteLine($"Thal: {heartData.Thal} ");
                Console.WriteLine($"Prediction Value: {prediction.Prediction} ");
                Console.WriteLine($"Prediction: {(prediction.Prediction ? "A disease could be present" : "Not present disease" )} ");
                Console.WriteLine($"Probability: {prediction.Probability} ");
                Console.WriteLine($"==================================================");
                Console.WriteLine("");
                Console.WriteLine("");
            }

```


