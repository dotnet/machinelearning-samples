module Clustering_Iris

open System
open System.IO

open Microsoft.ML.Runtime.Data
open Microsoft.ML.Runtime.Api
open Microsoft.ML.Runtime.KMeans


let AppPath = Path.Combine(__SOURCE_DIRECTORY__, "../../../..")
let DataPath = Path.Combine(AppPath, "datasets", "iris-full.txt")
let modelPath = Path.Combine(AppPath, "IrisClustersModel.zip")



/// Describes Iris flower. Used as an input to prediction function.
[<CLIMutable>]
type IrisData = {
    SepalLength : float32
    SepalWidth: float32
    PetalLength : float32
    PetalWidth : float32
} 

/// Represents result of prediction - the cluster to which Iris flower has been classified.
[<CLIMutable>]
type IrisPrediction = {
    [<ColumnName("PredictedLabel")>] SelectedClusterId : uint32
    [<ColumnName("Score")>] Distance : float32[]
}



module Pipeline =
    open Microsoft.ML.Core.Data

    let textTransform (inputColumn : string) outputColumn env =
        TextTransform(env, inputColumn, outputColumn)

    let concatEstimator name source env =
        ConcatEstimator(env,name, source)

    let append (estimator : IEstimator<'b>) (pipeline : IEstimator<ITransformer>)  = 
        pipeline.Append estimator
        
    let fit (dataView : IDataView) (pipeline : EstimatorChain<'a>) =
        pipeline.Fit dataView


let saveModelAsFile env (model : TransformerChain<'a>) =
    use fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
    model.SaveTo(env, fs)

    printfn "The model is saved to %s" modelPath


let predictWithModelLoadedFromFile() =

    let sampleIrisData = 
        { 
            SepalLength = 3.3f
            SepalWidth = 1.6f
            PetalLength = 0.2f
            PetalWidth = 5.1f 
        }

    // Test with Loaded Model from .zip file

    use env = new LocalEnvironment()
    use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
    let loadedModel = TransformerChain.LoadFrom(env, stream)

    // Create prediction engine and make prediction.

    let predictionFunc = loadedModel.MakePredictionFunction<IrisData, IrisPrediction> env
    let prediction = predictionFunc.Predict sampleIrisData

    printfn ""
    printfn "Clusters assigned for setosa flowers: %d" prediction.SelectedClusterId
    printfn ""


[<EntryPoint>]
let main argv =
    
    //1. Create ML.NET context/environment
    use env = new LocalEnvironment()

    //2. Create DataReader with data schema mapped to file's columns
    let reader = 
        TextLoader(
            env, 
            TextLoader.Arguments(
                Separator = "tab", 
                HasHeader = true, 
                Column = 
                    [|
                        TextLoader.Column("Label", Nullable DataKind.R4, 0)
                        TextLoader.Column("SepalLength", Nullable DataKind.R4, 1)
                        TextLoader.Column("SepalWidth", Nullable DataKind.R4, 2)
                        TextLoader.Column("PetalLength", Nullable DataKind.R4, 3)
                        TextLoader.Column("PetalWidth", Nullable DataKind.R4, 4)
                    |]
                )
            )

    //Load training data
    let trainingDataView = MultiFileSource(DataPath) |> reader.Read
    
    // Create and train the model            
    printfn "=============== Create and Train the Model ==============="

    let model = 
        env
        //3.Create a flexible pipeline (composed by a chain of estimators) for creating/traing the model.
        |> Pipeline.concatEstimator "Features" [| "SepalLength"; "SepalWidth"; "PetalLength"; "PetalWidth" |]
        |> Pipeline.append (KMeansPlusPlusTrainer(env, "Features", clustersCount = 3))
        //4. Create and train the model            
        |> Pipeline.fit trainingDataView

    printfn "=============== End of training ==============="
    printfn ""

    saveModelAsFile env model

    predictWithModelLoadedFromFile()
    
    
    0 // return an integer exit code
