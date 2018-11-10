﻿namespace MulticlassClassification_Iris

module DataStructures =

    /// Holds information about Iris flower to be classified.
    [<CLIMutable>]
    type IrisData = {
        SepalLength : float32
        SepalWidth : float32
        PetalLength : float32
        PetalWidth : float32
    } 

    /// Result of Iris classification. The array holds probability of the flower to be one of setosa, virginica or versicolor.
    [<CLIMutable>]
    type IrisPrediction = {
            Score : float32 []
        }    


    module TestIrisData =
        let Iris1 = { SepalLength = 5.1f; SepalWidth = 3.3f; PetalLength = 1.6f; PetalWidth= 0.2f }
        let Iris2 = { SepalLength = 6.4f; SepalWidth = 3.1f; PetalLength = 5.5f; PetalWidth = 2.2f }
        let Iris3 = { SepalLength = 4.4f; SepalWidth = 3.1f; PetalLength = 2.5f; PetalWidth = 1.2f }

