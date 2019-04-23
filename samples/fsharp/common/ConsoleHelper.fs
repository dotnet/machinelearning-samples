namespace Common

module ConsoleHelper =
    open System
    open Microsoft.ML
    open Microsoft.ML.Data
    //open Microsoft.ML.Api
    open System.Reflection

    let printPrediction prediction =
        printfn "*************************************************"
        printfn "Predicted : %s" prediction
        printfn "*************************************************"

    let printRegressionPredictionVersusObserved predictionCount observedCount =
        printfn "-------------------------------------------------"
        printfn "Predicted : %.4f" predictionCount
        printfn "Actual:     %s" observedCount
        printfn "-------------------------------------------------"

    let printRegressionMetrics name (metrics : RegressionMetrics) =
        printfn "*************************************************"
        printfn "*       Metrics for %s regression model      " name
        printfn "*------------------------------------------------"
        printfn "*       LossFn:        %.2f" metrics.LossFunction
        printfn "*       R2 Score:      %.2f" metrics.RSquared
        printfn "*       Absolute loss: %.2f" metrics.MeanAbsoluteError
        printfn "*       Squared loss:  %.2f" metrics.MeanSquaredError
        printfn "*       RMS loss:      %.2f" metrics.RootMeanSquaredError
        printfn "*************************************************"
    
    let printBinaryClassificationMetrics name (metrics : CalibratedBinaryClassificationMetrics) =
        printfn"************************************************************"
        printfn"*       Metrics for %s binary classification model      " name
        printfn"*-----------------------------------------------------------"
        printfn"*       Accuracy: %.2f%%" (metrics.Accuracy * 100.)
        printfn"*       Area Under Curve:      %.2f%%" (metrics.AreaUnderRocCurve * 100.)
        printfn"*       Area under Precision recall Curve:    %.2f%%" (metrics.AreaUnderPrecisionRecallCurve * 100.)
        printfn"*       F1Score:  %.2f%%" (metrics.F1Score * 100.)
 
        printfn"*       LogLogg:  %.2f%%" (metrics.LogLoss)
        printfn"*       LogLossreduction:  %.2f%%" (metrics.LogLossReduction)
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


    let private calculateStandardDeviation (values : float array) =
        let average = values |> Array.average
        let sumOfSquaresOfDifferences = values |> Array.map(fun v -> (v - average) * (v - average)) |> Array.sum
        let standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / float (values.Length-1))
        standardDeviation;

    let calculateConfidenceInterval95 (values : float array) =
        let confidenceInterval95 = 1.96 * calculateStandardDeviation(values) / Math.Sqrt(float (values.Length-1));
        confidenceInterval95

    let printMulticlassClassificationFoldsAverageMetrics algorithmName (crossValResults : TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>[]) =
        
        let metricsInMultipleFolds = crossValResults |> Array.map(fun r -> r.Metrics)

        let microAccuracyValues  = metricsInMultipleFolds |> Array.map(fun m -> m.MicroAccuracy)
        let microAccuracyAverage = microAccuracyValues |> Array.average
        let microAccuraciesStdDeviation = calculateStandardDeviation microAccuracyValues
        let microAccuraciesConfidenceInterval95 = calculateConfidenceInterval95 microAccuracyValues

        let macroAccuracyValues = metricsInMultipleFolds |> Array.map(fun m -> m.MicroAccuracy)
        let macroAccuracyAverage = macroAccuracyValues |> Array.average
        let macroAccuraciesStdDeviation = calculateStandardDeviation macroAccuracyValues
        let macroAccuraciesConfidenceInterval95 = calculateConfidenceInterval95 macroAccuracyValues

        let logLossValues = metricsInMultipleFolds |> Array.map (fun m -> m.LogLoss)
        let logLossAverage = logLossValues |> Array.average
        let logLossStdDeviation = calculateStandardDeviation logLossValues
        let logLossConfidenceInterval95 = calculateConfidenceInterval95 logLossValues

        let logLossReductionValues = metricsInMultipleFolds |> Array.map (fun m -> m.LogLossReduction)
        let logLossReductionAverage = logLossReductionValues |> Array.average
        let logLossReductionStdDeviation = calculateStandardDeviation logLossReductionValues
        let logLossReductionConfidenceInterval95 = calculateConfidenceInterval95 logLossReductionValues

        printfn "*************************************************************************************************************"
        printfn "*       Metrics for %s Multi-class Classification model      " algorithmName
        printfn "*------------------------------------------------------------------------------------------------------------"
        printfn "*       Average MicroAccuracy:    %.3f  - Standard deviation: (%.3f)  - Confidence Interval 95%%: (%.3f)" microAccuracyAverage microAccuraciesStdDeviation microAccuraciesConfidenceInterval95
        printfn "*       Average MacroAccuracy:    %.3f  - Standard deviation: (%.3f)  - Confidence Interval 95%%: (%.3f)" macroAccuracyAverage macroAccuraciesStdDeviation macroAccuraciesConfidenceInterval95
        printfn "*       Average LogLoss:          %.3f  - Standard deviation: (%.3f)  - Confidence Interval 95%%: (%.3f)" logLossAverage logLossStdDeviation logLossConfidenceInterval95
        printfn "*       Average LogLossReduction: %.3f  - Standard deviation: (%.3f)  - Confidence Interval 95%%: (%.3f)" logLossReductionAverage logLossReductionStdDeviation logLossReductionConfidenceInterval95
        printfn "*************************************************************************************************************"

    let printClusteringMetrics name (metrics : ClusteringMetrics) =
        printfn "*************************************************"
        printfn "*       Metrics for %s clustering model      " name
        printfn "*------------------------------------------------"
        printfn "*       Average Distance: %.15f" metrics.AverageDistance
        printfn "*       Davies Bouldin Index is: %.15f" metrics.DaviesBouldinIndex
        printfn "*************************************************"

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


    let peekDataViewInConsole<'TObservation when 'TObservation : (new : unit -> 'TObservation) and 'TObservation : not struct> (mlContext : MLContext) (dataView : IDataView) (pipeline : IEstimator<ITransformer>) numberOfRows =
        
        let msg = sprintf "Peek data in DataView: Showing %d rows with the columns" numberOfRows
        consoleWriteHeader msg

        //https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md#how-do-i-look-at-the-intermediate-data
        let transformer = pipeline.Fit dataView
        let transformedData = transformer.Transform dataView

        // 'transformedData' is a 'promise' of data, lazy-loading. call Preview  
        //and iterate through the returned collection from preview.

        transformedData.Preview(numberOfRows).RowView
        |> Seq.iter 
            (fun row ->
                row.Values
                |> Array.map (function KeyValue(k,v) -> sprintf "| %s:%O" k v)
                |> Array.fold (+) "Row--> "
                |> printfn "%s\n"
            )


    let peekVectorColumnDataInConsole (mlContext : MLContext) columnName (dataView : IDataView) (pipeline : IEstimator<ITransformer>) numberOfRows =
        let msg = sprintf "Peek data in DataView: : Show %d rows with just the '%s' column" numberOfRows columnName
        consoleWriteHeader msg

        let transformer = pipeline.Fit dataView
        let transformedData = transformer.Transform dataView

        // Extract the 'Features' column.
        let someColumnData = 
            transformedData.GetColumn<float32[]>(columnName)
            |> Seq.take numberOfRows
            |> Seq.toList

        // print to console the peeked rows
        someColumnData
        |> List.iter(fun row -> 
            let concatColumn = 
                row
                |> Array.map string
                |> String.concat ""
            printfn "%s" concatColumn
        )
                        
        someColumnData;

    let consoleWriterSection (lines : string array) =
        let defaultColor = Console.ForegroundColor
        Console.ForegroundColor <- ConsoleColor.Blue
        printfn " "
        lines
        |> Array.iter (printfn "%s")

        let maxLength = lines |> Array.map(fun x -> x.Length) |> Array.max
        printfn "%s" (new string('-', maxLength))
        Console.ForegroundColor <- defaultColor
    
    let consolePressAnyKey () =
        let defaultColor = Console.ForegroundColor
        Console.ForegroundColor <- ConsoleColor.Green
        printfn " "
        printfn "Press any key to finish."
        Console.ForegroundColor <- defaultColor
        Console.ReadKey() |> ignore
    
    let consoleWriteException (lines : string array) =
        let defaultColor = Console.ForegroundColor
        Console.ForegroundColor <- ConsoleColor.Red
        let exceptionTitle = "EXCEPTION"
        printfn " "
        printfn "%s" exceptionTitle
        printfn "%s" (new string('#', exceptionTitle.Length))
        Console.ForegroundColor <- defaultColor
        lines
        |> Array.iter (printfn "%s")

    let consoleWriteWarning (lines : string array) =
        let defaultColor = Console.ForegroundColor
        Console.ForegroundColor <-  ConsoleColor.DarkMagenta
        let warningTitle = "WARNING"
        printfn " "
        printfn "%s" warningTitle
        printfn "%s" (new string('#', warningTitle.Length))
        Console.ForegroundColor <- defaultColor
        lines
        |> Array.iter (printfn "%s")
