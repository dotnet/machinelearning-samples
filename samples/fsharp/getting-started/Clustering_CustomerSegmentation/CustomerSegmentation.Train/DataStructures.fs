namespace CustomerSegmentation.DataStructures
open Microsoft.ML.Runtime.Api
open System.IO
open System
open System.Collections.Generic
open Common
open System.Linq

[<CLIMutable>]
type ClusteringPrediction = {
    [<ColumnName("PredictedLabel")>] SelectedClusterId : uint32
    [<ColumnName("Score")>] Distance : float32 []
    [<ColumnName("PCAFeatures")>] Location : float32 []
    [<ColumnName("LastName")>] LastName : string
}

[<CLIMutable>]
type PivotObservation = {
    Features : float []
    LastName : string
}


[<CLIMutable>]
type Offer = {
    OfferId : string
    Campaign : string
    Varietal : string
    Minimum : float
    Discount : float
    Origin :string
    LastPeak :string
} with 
    static member ReadFromCsv file = 
        File
            .ReadAllLines(file)
            .Skip(1) // skip header
            .Select(fun x -> x.Split(','))
            .Select(fun x -> {
                                OfferId = x.[0]
                                Campaign = x.[1]
                                Varietal = x.[2]
                                Minimum = float x.[3]
                                Discount = float x.[4]
                                Origin = x.[5]
                                LastPeak = x.[6]
                            })


[<CLIMutable>]
type Transaction = {
    LastName : string
    OfferId : string
} with
    static member ReadFromCsv file = 
        File
            .ReadAllLines(file)
            .Skip(1) //skip header
            .Select(fun x -> x.Split(','))
            .Select(fun x -> {
                                LastName = x.[0]
                                OfferId = x.[1]
                            })

[<CLIMutable>]
type ClusterData = {
    OfferId : string
    Campaign : string
    Discount : float     
    LastName : string 
    LastPeak : string
    Minimum : float 
    Origin : string
    Varietal : string
    Count : int
}


[<CLIMutable>]
type PivotData = {
    C1 : float32
    C2 : float32
    C3 : float32
    C4 : float32
    C5 : float32
    C6 : float32
    C7 : float32
    C8 : float32
    C9 : float32
    C10 : float32
    C11 : float32
    C12 : float32
    C13 : float32
    C14 : float32
    C15 : float32
    C16 : float32
    C17 : float32
    C18 : float32
    C19 : float32
    C20 : float32
    C21 : float32
    C22 : float32
    C23 : float32
    C24 : float32
    C25 : float32
    C26 : float32
    C27 : float32
    C28 : float32
    C29 : float32
    C30 : float32
    C31 : float32
    C32 : float32
    LastName : string
} with 
    override x.ToString() = 
        sprintf "%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%s" 
                x.C1 x.C2 x.C3 x.C4 x.C5 x.C6 x.C7 x.C8 x.C9 x.C10 
                x.C11 x.C12 x.C13 x.C14 x.C15 x.C16 x.C17 x.C18 x.C19 x.C20 
                x.C21 x.C22 x.C23 x.C24 x.C25 x.C26 x.C27 x.C28 x.C29 x.C30
                x.C31 x.C32 x.LastName
    static member SaveToCsv (salesData: seq<PivotData> ) file = 
        let columns = "C1,C2,C3,C4,C5,C6,C7,C8,C9,C10" + 
                      "C11,C12,C13,C14,C15,C16,C17,C18,C19,C20" +
                      "C21,C22,C23,C24,C25,C26,C27,C28,C29,C30" +
                      "C31,C32" +  "LastName"  //TODO: sprintf "%s" nameof(LastName) 
        let data = salesData.Select(fun x -> x.ToString()).Prepend(columns)
        File.WriteAllLines(file, data)


module DataHelper = 
    let PreProcess offersDataLocation transactionsDataLocation =
        ConsoleHelper.consoleWriteHeader "Preprocess input files"
        printfn "Offers file: %s" offersDataLocation
        printfn "Transactions file: %s" transactionsDataLocation

        let offers = Offer.ReadFromCsv(offersDataLocation)
        let transactions = Transaction.ReadFromCsv(transactionsDataLocation)

        let clusterData = 
            query {
                    for o in offers do 
                    join t in transactions 
                        on (o.OfferId = t.OfferId)
                    select {
                        OfferId = o.OfferId 
                        Campaign = o.Campaign
                        Discount = o.Discount  
                        LastName = t.LastName
                        LastPeak = o.LastPeak
                        Minimum = o.Minimum 
                        Origin = o.Origin
                        Varietal = o.Varietal
                        Count = 1
                    }
                } |> Seq.toArray

        // pivot table (naive way)
        // based on code from https://stackoverflow.com/a/43091570

        let pivotDataArray = 
            query {
                for c in clusterData do 
                    groupBy c.LastName into gcs
                    let lookup = gcs.ToLookup(
                                               (fun x -> x.OfferId),
                                               (fun x -> x.Count)
                                            )
                    select {
                        LastName = gcs.Key
                        C1 = lookup.["1"].Sum() |> float32
                        C2 = lookup.["2"].Sum() |> float32
                        C3 = lookup.["3"].Sum() |> float32
                        C4 = lookup.["4"].Sum() |> float32
                        C5 = lookup.["5"].Sum() |> float32
                        C6 = lookup.["6"].Sum() |> float32
                        C7 = lookup.["7"].Sum() |> float32
                        C8 = lookup.["8"].Sum() |> float32
                        C9 = lookup.["9"].Sum() |> float32
                        C10 = lookup.["10"].Sum() |> float32
                        C11 = lookup.["11"].Sum() |> float32
                        C12 = lookup.["12"].Sum() |> float32
                        C13 = lookup.["13"].Sum() |> float32
                        C14 = lookup.["14"].Sum() |> float32
                        C15 = lookup.["15"].Sum() |> float32
                        C16 = lookup.["16"].Sum() |> float32
                        C17 = lookup.["17"].Sum() |> float32
                        C18 = lookup.["18"].Sum() |> float32
                        C19 = lookup.["19"].Sum() |> float32
                        C20 = lookup.["20"].Sum() |> float32
                        C21 = lookup.["21"].Sum() |> float32
                        C22 = lookup.["22"].Sum() |> float32
                        C23 = lookup.["23"].Sum() |> float32
                        C24 = lookup.["24"].Sum() |> float32
                        C25 = lookup.["25"].Sum() |> float32
                        C26 = lookup.["26"].Sum() |> float32
                        C27 = lookup.["27"].Sum() |> float32
                        C28 = lookup.["28"].Sum() |> float32
                        C29 = lookup.["29"].Sum() |> float32
                        C30 = lookup.["30"].Sum() |> float32
                        C31 = lookup.["31"].Sum() |> float32
                        C32 = lookup.["23"].Sum() |> float32
                    }  
            } |> Seq.toArray

        printfn "Total rows: %i" pivotDataArray.Length

        pivotDataArray

    let PreProcessAndSave offersDataLocation transactionsDataLocation pivotDataLocation =
        let preProcessData = PreProcess offersDataLocation transactionsDataLocation
        PivotData.SaveToCsv preProcessData pivotDataLocation
        preProcessData 