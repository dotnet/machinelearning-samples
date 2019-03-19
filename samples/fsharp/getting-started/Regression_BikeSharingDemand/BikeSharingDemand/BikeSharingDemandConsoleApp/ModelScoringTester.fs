module ModelScoringTester

open System
open System.IO
open DataStructures
open Microsoft.ML
open System.Globalization

let readSampleDataFromCsvFile dataLocation numberOfRecordsToRead =
    File.ReadLines(dataLocation)
    |> Seq.skip 1
    |> Seq.filter(fun x -> not(String.IsNullOrEmpty(x)))
    |> Seq.map(fun x -> x.Split(','))
    |> Seq.map(fun x -> {
        Season = Single.Parse(x.[2], CultureInfo.InvariantCulture)
        Year = Single.Parse(x.[3], CultureInfo.InvariantCulture)
        Month = Single.Parse(x.[4], CultureInfo.InvariantCulture)
        Hour = Single.Parse(x.[5], CultureInfo.InvariantCulture)
        Holiday = Single.Parse(x.[6], CultureInfo.InvariantCulture)
        Weekday = Single.Parse(x.[7], CultureInfo.InvariantCulture)
        WorkingDay = Single.Parse(x.[8], CultureInfo.InvariantCulture)
        Weather = Single.Parse(x.[9], CultureInfo.InvariantCulture)
        Temperature = Single.Parse(x.[10], CultureInfo.InvariantCulture)
        NormalizedTemperature = Single.Parse(x.[11], CultureInfo.InvariantCulture)
        Humidity = Single.Parse(x.[12], CultureInfo.InvariantCulture)
        Windspeed = Single.Parse(x.[13], CultureInfo.InvariantCulture)
        Count = Single.Parse(x.[16], CultureInfo.InvariantCulture)
    })
    |> Seq.take numberOfRecordsToRead
    |> Seq.toList


let visualizeSomePredictions testDataLocation (predEngine : PredictionEngine<DemandObservation, DemandPrediction>) numberOfPredictions =
    //Make a few prediction tests 
    // Make the provided number of predictions and compare with observed data from the test dataset

    let testData = readSampleDataFromCsvFile testDataLocation numberOfPredictions
    for i in 0 .. (numberOfPredictions - 1) do
        //Score
        let resultprediction = predEngine.Predict testData.[i]
        Common.ConsoleHelper.printRegressionPredictionVersusObserved (float resultprediction.PredictedCount) (testData.[i].Count.ToString())
    
