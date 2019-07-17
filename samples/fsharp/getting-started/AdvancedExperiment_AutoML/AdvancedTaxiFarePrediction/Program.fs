open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.ML
open Microsoft.ML.AutoML
open Microsoft.ML.Data
open PLplot
open Common


let assemblyFolderPath = Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName
let absolutePath x = Path.Combine(assemblyFolderPath, x)

let baseDatasetsRelativePath = @"Data"

let trainDataRelativePath = Path.Combine(baseDatasetsRelativePath, "taxi-fare-train.csv")
let trainDataPath = absolutePath trainDataRelativePath

let testDataRelativePath = Path.Combine(baseDatasetsRelativePath, "taxi-fare-test.csv")
let testDataPath = absolutePath testDataRelativePath

let baseModelsRelativePath = @"../../../MLModels"
let modelRelativePath = Path.Combine(baseModelsRelativePath, "TaxiFareModel.zip")
let modelPath = absolutePath modelRelativePath

let labelColumnName = "fare_amount"

[<CLIMutable>]
type TaxiTrip = 
    {
        [<ColumnName("vendor_id")>]
        VendorId : string
        [<ColumnName("rate_code")>]
        RateCode : float32
        [<ColumnName("passenger_count")>]
        PassengerCount : float32
        [<ColumnName("trip_time_in_secs")>]
        TripTime : float32
        [<ColumnName("trip_distance")>]
        TripDistance : float32
        [<ColumnName("payment_type")>]
        PaymentType : string
        [<ColumnName("fare_amount")>]
        FareAmount : float32
    }

[<CLIMutable>]
type TaxiTripFarePrediction = 
    {
        [<ColumnName("Score")>]
        FareAmount : float32
    }

let downcastPipeline (x : IEstimator<_>) = 
    match x with 
    | :? IEstimator<ITransformer> as y -> y
    | _ -> failwith "downcastPipeline: expecting a IEstimator<ITransformer>"

let readTaxiTripCsv filename = 
    File.ReadLines filename
    |> Seq.skip 1 
    |> Seq.map 
        (fun x -> 
            let f = x.Split ','
            {
                VendorId = f.[0]
                RateCode = float32 f.[1]
                PassengerCount = float32 f.[2]
                TripTime = float32 f.[3]
                TripDistance = float32 f.[4]
                PaymentType = f.[5]
                FareAmount = float32 f.[6]
            }
        )

let plotRegressionChart (mlContext : MLContext) numberOfRecordsToRead = 
    
    let args = Environment.GetCommandLineArgs()

    let trainedModel,_ = 
        use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        mlContext.Model.Load(stream)

    // Create prediction engine related to the loaded trained model
    let predFunction = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel)
    
    let chartFileName = 
        use pl = new PLStream()
        let chartFileName = 
            match args with 
            | [|_; "svg"|] -> 
                pl.sdev("svg")
                "TaxiRegressionDistribution.svg"
            | _ -> 
                pl.sdev("pngcairo")
                "TaxiRegressionDistribution.png"
        pl.sfnam chartFileName

        // Use white background with black foreground
        pl.spal0 "cmap0_alternate.pal"

        // Initialize plplot
        pl.init()
        let xMinLimit = 0.0
        let xMaxLimit = 35.0 // Rides larger than $35 are not shown in the chart
        let yMinLimit = 0.0
        let yMaxLimit = 35.0  // Rides larger than $35 are not shown in the chart
        pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes)

        // Set scaling for mail title text 125% size of default
        pl.schr(0.0, 1.25)

        // The main title
        pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction")

        // Plot using different colors
        // See: http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
        pl.col0(1)

        let testData = 
            readTaxiTripCsv testDataPath
            |> Seq.truncate numberOfRecordsToRead
            |> Seq.toArray

        let totalNumber = double testData.Length

        // This code is the symbol to paint
        let code = char 9

        // Plot using other color
        //pl.col0(9) //Light Green
        //pl.col0(4) //Red
        pl.col0(2) //Blue

        let xTotal,yTotal,xyMultiTotal,xSquareTotal =
            ((0.0,0.0,0.0,0.0),testData)
            ||> Array.fold 
                (fun (xTotal,yTotal,xyMultiTotal,xSquareTotal) i ->
                    let prediction = predFunction.Predict(i)
                    let x = double i.FareAmount
                    let y = double prediction.FareAmount
                    pl.poin([|x|], [|y|], code)
                    printfn "-------------------------------------------------"
                    printfn "Predicted : %f" y
                    printfn "Actual:    %f" x
                    printfn "-------------------------------------------------"
                    xTotal + x, yTotal + y,  xyMultiTotal + x*y, xSquareTotal + x*x
                )
     
        // Regression Line calculation explanation:
        // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example
        let minY = yTotal / totalNumber
        let minX = xTotal / totalNumber
        let minXY = xyMultiTotal / totalNumber
        let minXsquare = xSquareTotal / totalNumber

        let m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare)
        let b = minY - (m * minX)
        let y x = m*x + b
        
        pl.col0(4)
        let xs = [|1.0; 39.0|]
        pl.line(xs, xs |> Array.map y)

        // End page (writes output to disk)
        pl.eop()

        // Output version of PLplot
        printfn "PLplot version %s" (pl.gver())
        chartFileName

    // Open Chart File In Microsoft Photos App (Or default app, like browser for .svg)
    printfn "Showing chart..."
    let chartFileNamePath = @".\" + chartFileName
    let pinfo = new ProcessStartInfo(chartFileNamePath, UseShellExecute = true)
    Process.Start pinfo |> ignore



let mlContext = new MLContext()

// Infer columns in the dataset with AutoML
let columnInference = 
    ConsoleHelper.consoleWriteHeader "=============== Inferring columns in dataset ==============="
    let columnInference = mlContext.Auto().InferColumns(trainDataPath, labelColumnName, groupColumns=false)
    ConsoleHelper.print columnInference
    columnInference

// Load data from files using inferred columns
let textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions)
let trainDataView = textLoader.Load(trainDataPath)
let testDataView = textLoader.Load(testDataPath)

// Run an AutoML experiment on the dataset
let experimentResult = 

    // STEP 1: Display first few rows of the training data 
    ConsoleHelper.showDataViewInConsole mlContext trainDataView 4

    // STEP 2: Build a pre-featurizer for use in the AutoML experiment.
    // (Internally, AutoML uses one or more train/validation data splits to 
    // evaluate the models it produces. The pre-featurizer is fit only on the 
    // training data split to produce a trained transform. Then, the trained transform 
    // is applied to both the train and validation data splits.)
    let preFeaturizer = mlContext.Transforms.Conversion.MapValue("is_cash", [| KeyValuePair("CSH", true) |], "payment_type") |> downcastPipeline

    // STEP 3: Customize column information returned by InferColumns API
    let columnInformation = columnInference.ColumnInformation
    columnInformation.CategoricalColumnNames.Remove("payment_type") |> ignore
    columnInformation.IgnoredColumnNames.Add("payment_type")

    // STEP 4: Initialize a cancellation token source to stop the experiment.
    let cts = new CancellationTokenSource()

    // STEP 5: Initialize our user-defined progress handler that AutoML will 
    // invoke after each model it produces and evaluates.
    let progressHandler = ConsoleHelper.regressionExperimentProgressHandler()

    // STEP 6: Create experiment settings
    let experimentSettings = 
        let experimentSettings = new RegressionExperimentSettings()
        experimentSettings.MaxExperimentTimeInSeconds <- 3600u
        experimentSettings.CancellationToken <- cts.Token

        // Set the metric that AutoML will try to optimize over the course of the experiment.
        experimentSettings.OptimizingMetric <- RegressionMetric.RootMeanSquaredError

        // Set the cache directory to null.
        // This will cause all models produced by AutoML to be kept in memory 
        // instead of written to disk after each run, as AutoML is training.
        // (Please note: for an experiment on a large dataset, opting to keep all 
        // models trained by AutoML in memory could cause your system to run out 
        // of memory.)
        //experimentSettings.CacheDirectory <- null

        // Don't use LbfgsPoissonRegression and OnlineGradientDescent trainers during this experiment.
        // (These trainers sometimes underperform on this dataset.)
        experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression) |> ignore
        experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent) |> ignore

        experimentSettings

    // STEP 7: Run AutoML regression experiment
    let experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings)
    ConsoleHelper.consoleWriteHeader "=============== Running AutoML experiment ==============="
    printfn "Running AutoML regression experiment..."
    let stopwatch = Stopwatch.StartNew()
    // Cancel experiment after the user presses any key
    async {
        printfn "Press any key to stop the experiment run..."
        Console.ReadKey() |> ignore
        cts.Cancel()
    } |> Async.Start

    let experimentResult = experiment.Execute(trainDataView, columnInformation, preFeaturizer, progressHandler)
    printfn "%d models were returned after %0.2f seconds%s" (experimentResult.RunDetails.Count()) stopwatch.Elapsed.TotalSeconds Environment.NewLine

    // Print top models found by AutoML
    experimentResult.RunDetails
    |> Seq.filter (fun r -> not (isNull r.ValidationMetrics) && not (Double.IsNaN r.ValidationMetrics.RootMeanSquaredError))
    |> Seq.sortBy (fun x -> x.ValidationMetrics.RootMeanSquaredError)
    |> Seq.truncate 3
    |> Seq.iteri
        (fun i x ->
            ConsoleHelper.printRegressionIterationMetrics (i + 1) x.TrainerName x.ValidationMetrics x.RuntimeInSeconds
        )

    experimentResult

// Evaluate the model and print metrics
ConsoleHelper.consoleWriteHeader "===== Evaluating model's accuracy with test data ====="
let predictions = experimentResult.BestRun.Model.Transform(testDataView)
let metrics = mlContext.Regression.Evaluate(predictions, labelColumnName = labelColumnName, scoreColumnName = "Score")
ConsoleHelper.printRegressionMetrics experimentResult.BestRun.TrainerName metrics

// Save / persist the best model to a.ZIP file
ConsoleHelper.consoleWriteHeader "=============== Saving the model ==============="
mlContext.Model.Save(experimentResult.BestRun.Model, trainDataView.Schema, modelPath)
printfn "The model is saved to %s" modelPath

// Make a single test prediction loading the model from .ZIP file
ConsoleHelper.consoleWriteHeader "=============== Testing prediction engine ==============="

// Sample: 
// vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
// VTS,1,1,1140,3.75,CRD,15.5

let taxiTripSample = 
    {
        VendorId = "VTS"
        RateCode = 1.f
        PassengerCount = 1.f
        TripTime = 1140.f
        TripDistance = 3.75f
        PaymentType = "CRD"
        FareAmount = 0.f // To predict. Actual/Observed = 15.5
    }

let trainedModel, modelInputSchema = mlContext.Model.Load(modelPath)

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel)

// Score
let predictedResult = predEngine.Predict(taxiTripSample)

printfn "**********************************************************************"
printfn "Predicted fare: %0.4f, actual fare: 15.5" predictedResult.FareAmount
printfn "**********************************************************************"

// Paint regression distribution chart for a number of elements read from a Test DataSet file
plotRegressionChart mlContext 100

// Re-fit best pipeline on train and test data, to produce 
// a model that is trained on as much data as is available.
// This is the final model that can be deployed to production.
ConsoleHelper.consoleWriteHeader "=============== Re-fitting best pipeline ==============="
let refitModel = 
    let textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions)
    MultiFileSource(trainDataPath, testDataPath)
    |> textLoader.Load
    |> experimentResult.BestRun.Estimator.Fit

// Save the re-fit model to a.ZIP file
ConsoleHelper.consoleWriteHeader "=============== Saving the model ==============="
mlContext.Model.Save(refitModel, trainDataView.Schema, modelPath)
printfn "The model is saved to %s" modelPath

printfn "Press any key to exit.."
Console.ReadLine() |> ignore

