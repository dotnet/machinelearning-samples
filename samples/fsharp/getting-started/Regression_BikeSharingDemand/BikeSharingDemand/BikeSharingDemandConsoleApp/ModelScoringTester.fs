module ModelScoringTester

open System
open System.IO
open DataStructures

let readSampleDataFromCsvFile dataLocation numberOfRecordsToRead =
    File.ReadLines(dataLocation)
    |> Seq.skip 1
    |> Seq.filter(fun x -> not(String.IsNullOrEmpty(x)))
    |> Seq.map(fun x -> x.Split(','))
    |> Seq.map(fun x -> {
        Season = Single.Parse(x.[2])
        Year = Single.Parse(x.[3])
        Month = Single.Parse(x.[4])
        Hour = Single.Parse(x.[5])
        Holiday = Single.Parse(x.[6])
        Weekday = Single.Parse(x.[7])
        WorkingDay = Single.Parse(x.[8])
        Weather = Single.Parse(x.[9])
        Temperature = Single.Parse(x.[10])
        NormalizedTemperature = Single.Parse(x.[11])
        Humidity = Single.Parse(x.[12])
        Windspeed = Single.Parse(x.[13])
        Count = Single.Parse(x.[16])
    })
    |> Seq.take numberOfRecordsToRead
    |> Seq.toList


let visualizeSomePredictions testDataLocation modelScorer numberOfPredictions =
    
    //Make a few prediction tests 
    // Make the provided number of predictions and compare with observed data from the test dataset
    let testData = readSampleDataFromCsvFile testDataLocation numberOfPredictions

    for i = 0 to numberOfPredictions-1 do

        let prediction = 
            modelScorer 
            |> Common.ModelScorer.predictSingle testData.[i]

        Common.ConsoleHelper.printRegressionPredictionVersusObserved (int prediction.PredictedCount) (testData.[i].Count.ToString())
    
