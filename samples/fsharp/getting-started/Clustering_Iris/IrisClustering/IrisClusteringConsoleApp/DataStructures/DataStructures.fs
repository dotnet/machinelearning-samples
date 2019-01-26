namespace Clustering_Iris.DataStructures

module DataStructures =
    open Microsoft.ML.Data

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
