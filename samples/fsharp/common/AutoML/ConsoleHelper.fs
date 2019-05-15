module Common.ConsoleHelper

open System
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.AutoML

let width = 114

let printRegressionMetrics name (metrics : RegressionMetrics) =
    printfn "*************************************************"
    printfn "*       Metrics for {name} regression model      "
    printfn "*------------------------------------------------"
    printfn "*       LossFn:        %.2f" metrics.LossFunction
    printfn "*       R2 Score:      %.2f" metrics.RSquared
    printfn "*       Absolute loss: %.2f" metrics.MeanAbsoluteError
    printfn "*       Squared loss:  %.2f" metrics.MeanSquaredError
    printfn "*       RMS loss:      %.2f" metrics.RootMeanSquaredError
    printfn "*************************************************"

let printBinaryClassificationMetrics name (metrics : BinaryClassificationMetrics) =
    printfn"************************************************************"
    printfn"*       Metrics for %s binary classification model      " name
    printfn"*-----------------------------------------------------------"
    printfn"*       Accuracy: %.2f%%" (metrics.Accuracy * 100.)
    printfn"*       Area Under Curve:      %.2f%%" (metrics.AreaUnderRocCurve * 100.)
    printfn"*       Area under Precision recall Curve:    %.2f%%" (metrics.AreaUnderPrecisionRecallCurve * 100.)
    printfn"*       F1Score:  %.2f%%" (metrics.F1Score * 100.)
    printfn"*       PositivePrecision:      %.2f" (metrics.PositivePrecision)
    printfn"*       PositiveRecall:      %.2f" (metrics.PositiveRecall)
    printfn"*       NegativePrecision:      %.2f" (metrics.NegativePrecision)
    printfn"*       NegativeRecall:      %.2f" (metrics.NegativeRecall)
    printfn"************************************************************"

let printMultiClassClassificationMetrics name (metrics : MulticlassClassificationMetrics) =
    printfn "************************************************************"
    printfn "*    Metrics for %s multi-class classification model   " name
    printfn "*-----------------------------------------------------------"
    printfn "    AccuracyMacro = %.4f, a value between 0 and 1, the closer to 1, the better" metrics.MacroAccuracy
    printfn "    AccuracyMicro = %.4f, a value between 0 and 1, the closer to 1, the better" metrics.MacroAccuracy
    printfn "    LogLoss = %.4f, the closer to 0, the better" metrics.LogLoss
    printfn "    LogLoss for class 1 = %.4f, the closer to 0, the better" metrics.PerClassLogLoss.[0]
    printfn "    LogLoss for class 2 = %.4f, the closer to 0, the better" metrics.PerClassLogLoss.[1]
    printfn "    LogLoss for class 3 = %.4f, the closer to 0, the better" metrics.PerClassLogLoss.[2]
    printfn "************************************************************"


let createRow width (message : string) = sprintf "|%s|" (message.PadRight(width - 2))

let printRow l = 
    l
    |> List.map (fun (p,msg : string) -> if p > 0 then msg.PadLeft(p, ' ') else msg.PadRight(-p, ' '))
    |> String.concat " "
    |> createRow width 
    |> printfn "%s"
    

let regressionMetricsHeader = 
    [
        -4, ""
        -35, "Trainer"
        8, "RSquared"
        13, "Absolute-loss"
        12, "Squared-loss"
        8, "RMS-loss"
        9, "Duration"
    ]

let printRegressionMetricsHeader() = printRow regressionMetricsHeader

let printRegressionIterationMetrics iteration trainerName (metrics : RegressionMetrics) runtimeInSeconds = 
    [
        string iteration
        trainerName
        sprintf "%0.4f" metrics.RSquared
        sprintf "%0.2f" metrics.MeanAbsoluteError
        sprintf "%0.2f" metrics.MeanSquaredError
        sprintf "%0.2f" metrics.RootMeanSquaredError
        sprintf "%0.1f" runtimeInSeconds
    ]
    |> List.zip (regressionMetricsHeader |> List.map fst)
    |> printRow

let binaryMetricsHeader = 
    [
        -4, ""
        -35, "Trainer"
        9, "Accuracy"
        8, "AUC"
        8, "AUPRC"
        9, "F1-score"
        9, "Duration"
    ]

let printBinaryMetricsHeader() = printRow binaryMetricsHeader

let printBinaryIterationMetrics iteration trainerName (metrics : BinaryClassificationMetrics) runtimeInSeconds = 
    [
        string iteration
        trainerName
        sprintf "%0.4f" metrics.Accuracy
        sprintf "%0.2f" metrics.AreaUnderRocCurve
        sprintf "%0.2f" metrics.AreaUnderPrecisionRecallCurve
        sprintf "%0.2f" metrics.F1Score
        sprintf "%0.1f" runtimeInSeconds
    ]
    |> List.zip (binaryMetricsHeader |> List.map fst)
    |> printRow

let multiclassMetricsHeader = 
    [
        -4, ""
        -35, "Trainer"
        14, "MicroAccuracy"
        14, "MacroAccuracy"
        9, "Duration"
    ]

let printMulticlassMetricsHeader() = printRow multiclassMetricsHeader

let printMulticlassIterationMetrics iteration trainerName (metrics : MulticlassClassificationMetrics) runtimeInSeconds = 
    [
        string iteration
        trainerName
        sprintf "%0.4f" metrics.MicroAccuracy
        sprintf "%0.2f" metrics.MacroAccuracy
        sprintf "%0.1f" runtimeInSeconds
    ]
    |> List.zip (multiclassMetricsHeader |> List.map fst)
    |> printRow

let print (results : ColumnInferenceResults) = 
    let dataTypes = results.TextLoaderOptions.Columns |> Seq.map (fun x -> x.Name, string x.DataKind) |> dict
    let row purpose name = 
        match name with 
        | null -> None 
        | _ -> Some [name; dataTypes.[name]; purpose]
    let header = ["Name"; "Data Type"; "Purpose"]
    let rows = 
        [
            yield Some header
            yield row "Label" results.ColumnInformation.LabelColumnName 
            yield row "Weight" results.ColumnInformation.ExampleWeightColumnName 
            yield row "Sampling Key" results.ColumnInformation.SamplingKeyColumnName
            yield! results.ColumnInformation.CategoricalColumnNames |> Seq.map (row "Categorical")
            yield! results.ColumnInformation.NumericColumnNames |> Seq.map (row "Numeric")
            yield! results.ColumnInformation.TextColumnNames |> Seq.map (row "Text")
            yield! results.ColumnInformation.IgnoredColumnNames |> Seq.map (row "Ignored")
        ]
        |> List.choose id
    let lengths = List.init 3 (fun i -> rows |> List.map (fun x -> x.[i].Length) |> List.max)
    let rowLength = 
        let length = lengths |> Seq.sum
        length + 8
    let bar = String.replicate rowLength "-" |> sprintf "  %s"
    let innerBar = String.replicate rowLength "-" |> sprintf " |%s|"
    let fmtRow (l : string list) = 
        let l = l |> List.mapi (fun i x -> x.PadRight(lengths.[i], ' '))
        sprintf " | %s | %s | %s |" l.[0] l.[1] l.[2]
    [
        yield "Inferred dataset columns --"
        yield bar
        yield fmtRow header
        yield innerBar
        yield! rows.Tail |> List.map fmtRow
        yield bar
        yield ""
    ]
    |> List.iter (printfn "%s")

let consoleWriteHeader line =
    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Yellow
    printfn " "
    printfn "%s" line
    let maxLength = line.Length
    printfn "%s" (new string('#', maxLength))
    Console.ForegroundColor <- defaultColor

let downcastPipeline (pipeline : IEstimator<'a>) =
    match pipeline with
    | :? IEstimator<ITransformer> as p -> p
    | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

let showDataViewInConsole (mlContext : MLContext) (dataView : IDataView) numberOfRows =
    
    let msg = sprintf "Show data in DataView: Showing %d rows with the columns" numberOfRows
    consoleWriteHeader msg

    dataView.Preview(numberOfRows).RowView
    |> Seq.iter 
        (fun row ->
            row.Values
            |> Array.map (function KeyValue(k,v) -> sprintf "| %s:%O" k v)
            |> Array.fold (+) "Row--> "
            |> printfn "%s\n"
        )


let printIterationException (ex : exn) = 
    printf "Exception during AutoML iteration: %O" ex

let progressHandler printHeader printIterMetrics =
    let mutable iterIndex = 0
    {new IProgress<RunDetail<'a>> with
         member this.Report(value: RunDetail<'a>): unit = 
            if iterIndex = 0 then 
                printHeader()
            iterIndex <- iterIndex + 1
            match value.Exception with 
            | null -> 
                printIterMetrics iterIndex value.TrainerName value.ValidationMetrics value.RuntimeInSeconds
            | ex -> 
                printIterationException ex
    }

let regressionExperimentProgressHandler() = progressHandler printRegressionMetricsHeader printRegressionIterationMetrics
let binaryExperimentProgressHandler() = progressHandler printBinaryMetricsHeader printBinaryIterationMetrics
let multiclassExperimentProgressHandler() = progressHandler printMulticlassMetricsHeader printMulticlassIterationMetrics
