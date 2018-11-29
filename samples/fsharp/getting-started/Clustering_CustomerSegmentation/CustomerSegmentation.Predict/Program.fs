// Learn more about F# at http://fsharp.org

open System
open Common.ConsoleHelper
open System.IO
open Microsoft.ML
open CustomerSegmentation.Model


[<EntryPoint>]
let main argv =
    let assetsPath = "assets"
    let pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv")
    let modelZipFilePath = Path.Combine(assetsPath, "inputs", "retailClustering.zip")
    let plotSvg = Path.Combine(assetsPath, "outputs", "customerSegmentation.svg")
    let plotCsv = Path.Combine(assetsPath, "outputs", "customerSegmentation.csv")

    try
        let mlContext = MLContext(seed = Nullable(1)) //seed set to any number of so you have a deterministic results

        //Create the clusters: Create data files and plot a chart
        let clusteringModelScorer = new ClusteringModelScorer(mlContext, pivotCsv, plotSvg, plotCsv, modelZipFilePath)

        clusteringModelScorer.CreateCustomerClusters()

    with exn -> consoleWriteException [|exn.Message|]

    consolePressAnyKey()
    0
