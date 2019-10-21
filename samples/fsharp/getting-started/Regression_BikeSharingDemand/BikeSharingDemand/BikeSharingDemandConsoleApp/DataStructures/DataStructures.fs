module DataStructures

open Microsoft.ML.Data


[<CLIMutable>]
type DemandObservation =
    {
        [<LoadColumn(2)>]
        Season : float32
        [<LoadColumn(3)>]
        Year : float32
        [<LoadColumn(4)>]
        Month : float32
        [<LoadColumn(5)>]
        Hour : float32
        [<LoadColumn(6)>]
        Holiday : float32
        [<LoadColumn(7)>]
        Weekday : float32
        [<LoadColumn(8)>]
        WorkingDay : float32
        [<LoadColumn(9)>]
        Weather : float32
        [<LoadColumn(10)>]
        Temperature : float32
        [<LoadColumn(11)>]
        NormalizedTemperature : float32
        [<LoadColumn(12)>]
        Humidity : float32
        [<LoadColumn(13)>]
        Windspeed : float32
        [<LoadColumn(16); ColumnName("Label")>]
        Count : float32   // This is the observed count, to be used a "label" to predict
    }

[<CLIMutable>]
type DemandPrediction =
    {
        [<ColumnName("Score")>]
        PredictedCount : float32
    }
