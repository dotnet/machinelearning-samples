open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data
open Common
open Microsoft.ML.Transforms

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
type PivotObservation =
    {
        Features : float32 []
        LastName : string
    }

[<EntryPoint>]
let main _argv =
    let assetsPath = Path.Combine(dataRoot.Directory.FullName, @"..\..\..\assets")
    let transactionsCsv = Path.Combine(assetsPath, "inputs", "transactions.csv")
    let offersCsv = Path.Combine(assetsPath, "inputs", "offers.csv")
    let pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv")
    let modelZip = Path.Combine(assetsPath, "outputs", "retailClustering.zip")
    try
        //STEP 0: Special data pre-process in this sample creating the PivotTable csv file
        savePivotData offersCsv transactionsCsv pivotCsv
        //Create the MLContext to share across components for deterministic results
        let mlContext = MLContext(seed = Nullable 1);  //Seed set to any number so you have a deterministic environment
        // STEP 1: Common data loading configuration
        let pivotDataView =
            mlContext.Data.LoadFromTextFile(pivotCsv,
                columns =
                    [|
                        TextLoader.Column("Features", DataKind.Single, [| TextLoader.Range(0, Nullable 31) |])
                        TextLoader.Column("LastName", DataKind.String, 32)
                    |],
                hasHeader = true,
                separatorChar = ',')

        //STEP 2: Configure data transformations in pipeline
        let dataProcessPipeline =
            EstimatorChain()
                .Append(mlContext.Transforms.ProjectToPrincipalComponents("PCAFeatures", "Features", rank = 2))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LastNameKey", "LastName", OneHotEncodingEstimator.OutputKind.Indicator))

        // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations
        Common.ConsoleHelper.peekDataViewInConsole<PivotObservation> mlContext pivotDataView (ConsoleHelper.downcastPipeline dataProcessPipeline) 10 |> ignore
        Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" pivotDataView (ConsoleHelper.downcastPipeline dataProcessPipeline) 10 |> ignore

        //STEP 3: Create the training pipeline
        let trainer = mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters = 3)
        let trainingPipeline = dataProcessPipeline.Append(trainer)

        //STEP 4: Train the model fitting to the pivotDataView
        printfn "=============== Training the model ==============="
        let trainedModel = trainingPipeline.Fit(pivotDataView)

        //STEP 5: Evaluate the model and show accuracy stats
        printfn "===== Evaluating Model's accuracy with Test data ====="
        let predictions = trainedModel.Transform(pivotDataView)
        let metrics = mlContext.Clustering.Evaluate(predictions, scoreColumnName = "Score", featureColumnName = "Features")

        Common.ConsoleHelper.printClusteringMetrics (string trainer) metrics

        //STEP 6: Save/persist the trained model to a .ZIP file
        do
            use fs = new FileStream(modelZip, FileMode.Create, FileAccess.Write, FileShare.Write)
            mlContext.Model.Save(trainedModel, pivotDataView.Schema, fs)

        printfn "The model is saved to %s" modelZip
    with
    | ex -> printExn [ex.ToString()]

    let defaultColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Green
    printfn " "
    printfn "Press any key to finish."
    Console.ForegroundColor <- defaultColor
    Console.ReadKey() |> ignore
    0