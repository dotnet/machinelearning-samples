module DataStructures

open Microsoft.ML.Runtime.Api

[<CLIMutable>]
type DemandObservation =
    {
        Season : float32
        Year : float32
        Month : float32
        Hour : float32
        Holiday : float32
        Weekday : float32
        WorkingDay : float32
        Weather : float32
        Temperature : float32
        NormalizedTemperature : float32
        Humidity : float32
        Windspeed : float32
        Count : float32   // This is the observed count, to be used a "label" to predict
    }

[<CLIMutable>]
type DemandPrediction =
    {
        [<ColumnName("Score")>]
        PredictedCount : float32
    }
