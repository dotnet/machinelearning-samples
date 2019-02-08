namespace MulticlassClassification_Iris

module DataStructures =
    open Microsoft.ML.Data

    /// Holds information about Iris flower to be classified.
    [<CLIMutable>]
    type IrisData = {
        [<LoadColumn(0)>]
        Label : float32

        [<LoadColumn(1)>]
        SepalLength : float32
        
        [<LoadColumn(2)>]
        SepalWidth : float32
        
        [<LoadColumn(3)>]
        PetalLength : float32
        
        [<LoadColumn(4)>]
        PetalWidth : float32
    } 

    /// Result of Iris classification. The array holds probability of the flower to be one of setosa, virginica or versicolor.
    [<CLIMutable>]
    type IrisPrediction = {
            Score : float32 []
        }    


    module SampleIrisData =
        let Iris1 = { Label = 0.f; SepalLength = 5.1f; SepalWidth = 3.3f; PetalLength = 1.6f; PetalWidth= 0.2f }
        let Iris2 = { Label = 0.f; SepalLength = 6.4f; SepalWidth = 3.1f; PetalLength = 5.5f; PetalWidth = 2.2f }
        let Iris3 = { Label = 0.f; SepalLength = 4.4f; SepalWidth = 3.1f; PetalLength = 2.5f; PetalWidth = 1.2f }

