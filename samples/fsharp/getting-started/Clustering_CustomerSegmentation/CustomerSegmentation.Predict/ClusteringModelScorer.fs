namespace CustomerSegmentation.Model

open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Runtime.Api
open Microsoft.ML.Runtime.Data
open OxyPlot
open OxyPlot.Series
open System
open System.Collections.Generic
open System.IO
open System.Diagnostics
open System.Linq
open CustomerSegmentation.DataStructures
open Common.ConsoleHelper


type ClusteringModelScorer (mlContext: MLContext, pivotDataLocation : string, plotLocation : string, csvlocation : string, modelPath : string) =

    let loadModelFromZipFile = 
        use stream = new FileStream (modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        mlContext.Model.Load stream

    let SaveCustomerSegmentationCSV (predictions : IEnumerable<ClusteringPrediction>) (csvlocation: string) = 
        consoleWriteHeader "CSV Customer Segmentation"
        use w = new StreamWriter(csvlocation)
        w.WriteLine("LastName,SelectedClusterId")
        w.Flush()
        predictions |> Seq.iter (fun p -> w.WriteLine(sprintf "%s,%s" p.LastName (p.SelectedClusterId.ToString())  ); w.Flush())

        printfn "CSV Location %s" csvlocation

    let SaveCustomerSegmentationPlotChart (predictions : IEnumerable<ClusteringPrediction>) (plotLocation : string) = 
        consoleWriteHeader "Plot Customer Segmentation"
        let plot = new PlotModel(
                        Title = "Customer Segmentation",
                        IsLegendVisible = true
                    )
        let clusters = predictions |> Seq.map (fun p -> p.SelectedClusterId) |> Seq.distinct |> Seq.sort

        clusters 
            |> Seq.iter (fun c -> 
                    let scatter = new ScatterSeries(MarkerType = MarkerType.Circle, MarkerStrokeThickness = 2., Title = sprintf "Cluster: %i" c, RenderInLegend = true)
                    let series = predictions |> Seq.where (fun p -> p.SelectedClusterId = c) |> Seq.map (fun p -> new ScatterPoint(p.Location.[0] |> float ,p.Location.[1] |> float)) |> Seq.toArray
                    scatter.Points.AddRange series
                    plot.Series.Add scatter
                )
        plot.DefaultColors <- OxyPalettes.HueDistinct(plot.Series.Count).Colors

        let exporter = new SvgExporter(Width = 600., Height = 400.)
        let fs = new FileStream(plotLocation, FileMode.Create)
        exporter.Export(plot, fs)
        printfn "Plot Loation: %s" plotLocation

    let OpenChartInDefaultWindow (plotLocation: string) = 
        printfn "Showing Chart..."
        let p = new Process()
        let psi = new ProcessStartInfo(plotLocation)
        psi.UseShellExecute <- true
        p.StartInfo <- psi
        p.Start() |> ignore
        ()


    member __.CreateCustomerClusters () =
        let reader = 
            TextLoader(mlContext, TextLoader.Arguments (
                                Column = [|
                                    TextLoader.Column("Features", DataKind.R4 |> Nullable, [|TextLoader.Range(0,31 |> Nullable)|])
                                    TextLoader.Column("LastName", DataKind.Text |> Nullable, 32)
                                |],
                                HasHeader = true,
                                Separator = ","
                    ))

        let data : IDataView = reader.Read(MultiFileSource (pivotDataLocation) :> IMultiStreamSource)

        //Apply data transformation to create prediction / clustering
        let predictions = loadModelFromZipFile.Transform(data).AsEnumerable<ClusteringPrediction>(mlContext, false).ToArray()

        //Generate data files with customer data grouped by clusters
        SaveCustomerSegmentationCSV predictions csvlocation

        //Plot/paint the clusters in a chart and open it with the by-default image-tool in Windows
        SaveCustomerSegmentationPlotChart predictions plotLocation
        OpenChartInDefaultWindow plotLocation
        