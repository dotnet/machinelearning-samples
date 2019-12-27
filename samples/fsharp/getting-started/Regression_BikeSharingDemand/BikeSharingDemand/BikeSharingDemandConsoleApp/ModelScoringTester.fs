module ModelScoringTester

open System
open System.IO
open DataStructures
open Microsoft.ML
open System.Globalization

let parseSingle (s:string) = Single.Parse(s, CultureInfo.InvariantCulture)

let readSampleDataFromCsvFile dataLocation numberOfRecordsToRead =
    File.ReadLines dataLocation
    |> Seq.skip 1
    |> Seq.filter (fun x -> not (String.IsNullOrEmpty x))
    |> Seq.map (fun x ->
        let fields = x.Split(',')
        { Season = parseSingle fields.[2]
          Year = parseSingle fields.[3]
          Month = parseSingle fields.[4]
          Hour = parseSingle fields.[5]
          Holiday = parseSingle fields.[6]
          Weekday = parseSingle fields.[7]
          WorkingDay = parseSingle fields.[8]
          Weather = parseSingle fields.[9]
          Temperature = parseSingle fields.[10]
          NormalizedTemperature = parseSingle fields.[11]
          Humidity = parseSingle fields.[12]
          Windspeed = parseSingle fields.[13]
          Count = parseSingle fields.[16] })
    |> Seq.take numberOfRecordsToRead
    |> Seq.toList

let visualizeSomePredictions testDataLocation (predEngine : PredictionEngine<DemandObservation, DemandPrediction>) numberOfPredictions =
    //Make a few prediction tests
    // Make the provided number of predictions and compare with observed data from the test dataset

    let testData = readSampleDataFromCsvFile testDataLocation numberOfPredictions
    for row in testData do
        //Score
        let resultprediction = predEngine.Predict row
        Common.ConsoleHelper.printRegressionPredictionVersusObserved
            (float resultprediction.PredictedCount)
            (row.Count.ToString())
