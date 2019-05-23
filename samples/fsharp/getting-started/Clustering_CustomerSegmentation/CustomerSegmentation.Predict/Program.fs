open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open OxyPlot.Series
open OxyPlot
open System.Diagnostics

let dataRoot = FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)

let printHeader lines =
    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Yellow
    printfn " "
    lines |> Seq.iter (printfn "%s")
    let maxLength = lines |> Seq.map (fun x -> x.Length) |> Seq.max
    printfn "%s" (String('#', maxLength))
    Console.ForegroundColor <- defaultColor

let printExn lines =
    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Red
    printfn " "
    printfn "EXCEPTION"
    printfn "#########"
    Console.ForegroundColor <- defaultColor
    lines |> Seq.iter (printfn "%s")

let savePivotData offersCsv transactionsCsv pivotCsv =
    printHeader ["Preprocess input files"]
    printfn "Offers file: %s"  offersCsv
    printfn "Transactions file: %s"  transactionsCsv
    let pivotData =
        File.ReadAllLines(transactionsCsv)
        |> Seq.skip 1 //skip header
        |> Seq.map
            (fun x ->
                let fields = x.Split ','
                fields.[0] , int fields.[1] // Name, Offer #
            )
        |> Seq.groupBy fst
        |> Seq.map
            (fun (k, xs) ->
                let offers = xs |> Seq.map snd |> Set.ofSeq
                [
                    yield! Seq.init 32 (fun i -> if Seq.contains (i + 1) offers then "1" else "0")
                    yield k
                ]
                |> String.concat ","
            )
    File.WriteAllLines(pivotCsv,
        seq {
            yield [
                yield! Seq.init 32 (fun i -> sprintf "C%d" (i + 1))
                yield "LastName"
            ] |> String.concat ","
            yield! pivotData
        })

[<CLIMutable>]
type ClusteringPrediction =
    {
        [<ColumnName("PredictedLabel")>]
        SelectedClusterId : uint32
        [<ColumnName("Score")>]
        Distance : float32 []
        [<ColumnName("PCAFeatures")>]
        Location : float32 []
        [<ColumnName("LastName")>]
        LastName : string
    }

let savePlot (predictions : ClusteringPrediction []) (plotSvg : string) =
    printHeader ["Plot Customer Segmentation"]
    let pm = PlotModel(Title = "Customer Segmentation", IsLegendVisible = true)
    predictions
    |> Seq.groupBy (fun x -> x.SelectedClusterId)
    |> Seq.sortBy fst
    |> Seq.iter
        (fun (cluster,xs) ->
            let scatter =
                ScatterSeries
                    (MarkerType = MarkerType.Circle,
                     MarkerStrokeThickness = 2.0,
                     Title = sprintf "Cluster: %d" cluster,
                     RenderInLegend = true )
            xs
            |> Seq.map (fun x -> ScatterPoint(double x.Location.[0], double x.Location.[1]))
            |> scatter.Points.AddRange
            pm.Series.Add scatter
        )
    pm.DefaultColors <- OxyPalettes.HueDistinct(pm.Series.Count).Colors
    let exporter = SvgExporter(Width = 600.0, Height = 400.0)
    use f = File.OpenWrite(plotSvg)
    exporter.Export(pm,f)
    printfn "Plot location: %s" plotSvg

[<EntryPoint>]
let main _argv =
    let assetsPath = Path.Combine(dataRoot.Directory.FullName, @"..\..\..\assets")
    let pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv")
    let modelZipFilePath = Path.Combine(assetsPath, "inputs", "retailClustering.zip")
    let plotSvg = Path.Combine(assetsPath, "outputs", "customerSegmentation.svg")
    let plotCsv = Path.Combine(assetsPath, "outputs", "customerSegmentation.csv")
    try
        let mlContext = MLContext(seed = Nullable 1);  //Seed set to any number so you have a deterministic result

        //Create the clusters: Create data files and plot a char
        let model, inputSchema =
            use f = new FileStream(modelZipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            mlContext.Model.Load(f)

        let data =
            mlContext.Data.LoadFromTextFile(
                pivotCsv,
                columns =
                    [|
                        TextLoader.Column("Features", DataKind.Single, [| TextLoader.Range(0, Nullable 31) |])
                        TextLoader.Column("LastName", DataKind.String, 32)
                    |],
                hasHeader = true,
                separatorChar = ',')

        //Apply data transformation to create predictions/clustering
        let predictions = mlContext.Data.CreateEnumerable<ClusteringPrediction>(model.Transform(data),false) |> Seq.toArray

        //Generate data files with customer data grouped by clusters
        printHeader ["CSV Customer Segmentation"]
        File.WriteAllLines(plotCsv,
            seq {
                yield "LastName,SelectedClusterId"
                yield! predictions |> Seq.map (fun x -> sprintf "%s,%d" x.LastName x.SelectedClusterId)
            })
        printfn "CSV location: %s" plotCsv

        //Plot/paint the clusters in a chart and open it with the by-default image-tool in Windows
        savePlot predictions plotSvg

        printfn "Showing chart..."
        ProcessStartInfo(plotSvg, UseShellExecute = true)
        |> Process.Start
        |> ignore

    with
    | ex -> printExn [ex.ToString()]

    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Green
    printfn " "
    printfn "Press any key to finish."
    Console.ForegroundColor <- defaultColor
    Console.ReadKey() |> ignore
    0