module Clustering_Iris

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Runtime.Api
open Microsoft.ML.Data
open Microsoft.ML.Trainers
open Microsoft.ML.Transforms

let AppPath = Path.Combine(__SOURCE_DIRECTORY__, "../../../..")
let DataPath = Path.Combine(AppPath, "datasets", "iris-full.txt")
let ModelPath = Path.Combine(AppPath, "IrisClustersModel.zip")

type IrisData() = 
    [<Column("0")>]
    member val Label: float32 = 0.0f with get,set

    [<Column("1")>]
    member val SepalLength: float32 = 0.0f with get, set

    [<Column("2")>]
    member val SepalWidth: float32 = 0.0f with get, set

    [<Column("3")>]
    member val PetalLength: float32 = 0.0f with get, set

    [<Column("4")>]
    member val PetalWidth: float32 = 0.0f with get, set

type ClusterPrediction() = 
    [<ColumnName("PredictedLabel")>]
    member val SelectedClusterId: uint32 = 0u with get, set

    [<ColumnName("Score")>]
    member val Distance: float32[] = null with get, set

let Train() =
    // LearningPipeline holds all steps of the learning process: data, transforms, learners.
    let pipeline = LearningPipeline()
    // The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
    // all the column names and their types.
    pipeline.Add(TextLoader(DataPath).CreateFrom<IrisData>(useHeader=true))
    // ColumnConcatenator concatenates all columns into Features column
    pipeline.Add(ColumnConcatenator("Features",
                                    "SepalLength",
                                    "SepalWidth",
                                    "PetalLength",
                                    "PetalWidth"))
    // KMeansPlusPlusClusterer is an algorithm that will be used to build clusters. We set the number of clusters to 3.
    pipeline.Add(KMeansPlusPlusClusterer(K = 3))

    Console.WriteLine("=============== Training model ===============")
    let model = pipeline.Train<IrisData, ClusterPrediction>()
    Console.WriteLine("=============== End training ===============")
    
    // Saving the model as a .zip file.
    model.WriteAsync(ModelPath) |> Async.AwaitTask |> Async.RunSynchronously
    Console.WriteLine("The model is saved to {0}", ModelPath)
   
    model

module TestIrisData = 
    let Setosa1 = IrisData(SepalLength = 5.1f, SepalWidth = 3.3f, PetalLength = 1.6f, PetalWidth = 0.2f)
    let Setosa2 = IrisData(SepalLength = 0.2f, SepalWidth = 5.1f, PetalLength = 3.5f, PetalWidth = 1.4f)
    let Virginica1 = IrisData(SepalLength = 6.4f, SepalWidth = 3.1f, PetalLength = 5.5f, PetalWidth = 2.2f)
    let Virginica2 = IrisData(SepalLength = 2.5f, SepalWidth = 6.3f, PetalLength = 3.3f, PetalWidth = 6.0f)
    let Versicolor1 = IrisData(SepalLength = 6.4f, SepalWidth = 3.1f, PetalLength = 4.5f, PetalWidth = 1.5f)
    let Versicolor2 = IrisData(SepalLength = 7.0f, SepalWidth = 3.2f, PetalLength = 4.7f, PetalWidth = 1.4f)

// STEP 1: Create a model
let model = Train()
        
// STEP 2: Make a prediction
Console.WriteLine()
let prediction1 = model.Predict(TestIrisData.Setosa1)
let prediction2 = model.Predict(TestIrisData.Setosa2)          
Console.WriteLine(sprintf "Clusters assigned for setosa flowers:")
Console.WriteLine(sprintf "                                        {%d}" prediction1.SelectedClusterId)
Console.WriteLine(sprintf "                                        {%d}" prediction2.SelectedClusterId)

let prediction3 = model.Predict(TestIrisData.Virginica1)
let prediction4 = model.Predict(TestIrisData.Virginica2)
Console.WriteLine(sprintf "Clusters assigned for virginica flowers:")
Console.WriteLine(sprintf "                                        {%d}" prediction3.SelectedClusterId)
Console.WriteLine(sprintf "                                        {%d}" prediction4.SelectedClusterId)

let prediction5 = model.Predict(TestIrisData.Versicolor1)
let prediction6 = model.Predict(TestIrisData.Versicolor2)
Console.WriteLine(sprintf "Clusters assigned for versicolor flowers:")
Console.WriteLine(sprintf "                                        {%d}" prediction5.SelectedClusterId)
Console.WriteLine(sprintf "                                        {%d}" prediction6.SelectedClusterId)
Console.ReadLine() |> ignore

