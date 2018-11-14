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
    [<ColumnName("Score")>] Distance : float []
    [<ColumnName("PCAFeatures")>] Location : float []
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
    C1 : float
    C2 : float
    C3 : float
    C4 : float
    C5 : float
    C6 : float
    C7 : float
    C8 : float
    C9 : float
    C10 : float
    C11 : float
    C12 : float
    C13 : float
    C14 : float
    C15 : float
    C16 : float
    C17 : float
    C18 : float
    C19 : float
    C20 : float
    C21 : float
    C22 : float
    C23 : float
    C24 : float
    C25 : float
    C26 : float
    C27 : float
    C28 : float
    C29 : float
    C30 : float
    C31 : float
    C32 : float
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
                        C1 = lookup.["1"].Sum() |> float
                        C2 = lookup.["2"].Sum() |> float
                        C3 = lookup.["3"].Sum() |> float
                        C4 = lookup.["4"].Sum() |> float
                        C5 = lookup.["5"].Sum() |> float
                        C6 = lookup.["6"].Sum() |> float
                        C7 = lookup.["7"].Sum() |> float
                        C8 = lookup.["8"].Sum() |> float
                        C9 = lookup.["9"].Sum() |> float
                        C10 = lookup.["10"].Sum() |> float
                        C11 = lookup.["11"].Sum() |> float
                        C12 = lookup.["12"].Sum() |> float
                        C13 = lookup.["13"].Sum() |> float
                        C14 = lookup.["14"].Sum() |> float
                        C15 = lookup.["15"].Sum() |> float
                        C16 = lookup.["16"].Sum() |> float
                        C17 = lookup.["17"].Sum() |> float
                        C18 = lookup.["18"].Sum() |> float
                        C19 = lookup.["19"].Sum() |> float
                        C20 = lookup.["20"].Sum() |> float
                        C21 = lookup.["21"].Sum() |> float
                        C22 = lookup.["22"].Sum() |> float
                        C23 = lookup.["23"].Sum() |> float
                        C24 = lookup.["24"].Sum() |> float
                        C25 = lookup.["25"].Sum() |> float
                        C26 = lookup.["26"].Sum() |> float
                        C27 = lookup.["27"].Sum() |> float
                        C28 = lookup.["28"].Sum() |> float
                        C29 = lookup.["29"].Sum() |> float
                        C30 = lookup.["30"].Sum() |> float
                        C31 = lookup.["31"].Sum() |> float
                        C32 = lookup.["23"].Sum() |> float
                    }  
            } |> Seq.toArray

        printfn "Total rows: %i" pivotDataArray.Length

        pivotDataArray

    let PreProcessAndSave offersDataLocation transactionsDataLocation pivotDataLocation =
        let preProcessData = PreProcess offersDataLocation transactionsDataLocation
        PivotData.SaveToCsv preProcessData pivotDataLocation
        preProcessData 