open Microsoft.ML.Data

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

let labelColumnName = "FareAmount"

[<CLIMutable>]
type TaxiTrip = 
    {
        [<LoadColumn(0)>]
        VendorId : string
        [<LoadColumn(1)>]
        RateCode : float32
        [<LoadColumn(2)>]
        PassengerCount : float32
        [<LoadColumn(3)>]
        TripTime : float32
        [<LoadColumn(4)>]
        TripDistance : float32
        [<LoadColumn(5)>]
        PaymentType : string
        [<LoadColumn(6)>]
        FareAmount : float32
    }

[<CLIMutable>]
type TaxiTripFarePrediction = 
    {
        [<ColumnName("Score")>]
        FareAmount : float32
    }


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

        // use white background with black foreground
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
        //pl.col0(4) // Red
        pl.col0(2) // Blue

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


let experimentTimeInSeconds = 60u

let mlContext = MLContext()


// Create, train, evaluate and save a model

// STEP 1: Common data loading configuration
let trainingDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(trainDataPath, hasHeader = true, separatorChar = ',')
let testDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(testDataPath, hasHeader = true, separatorChar = ',')

// STEP 2: Display first few rows of the training data
ConsoleHelper.showDataViewInConsole mlContext trainingDataView 4

// STEP 3: Initialize our user-defined progress handler that AutoML will 
// invoke after each model it produces and evaluates.
let progressHandler = ConsoleHelper.regressionExperimentProgressHandler()

// STEP 4: Run AutoML regression experiment
ConsoleHelper.consoleWriteHeader "=============== Training the model ==============="
printfn "Running AutoML regression experiment for %d seconds..." experimentTimeInSeconds
let experimentResult = mlContext.Auto() .CreateRegressionExperiment(experimentTimeInSeconds).Execute(trainingDataView, labelColumnName, progressHandler= progressHandler)

// Print top models found by AutoML
printfn ""
printfn "Top models ranked by R-Squared --"
experimentResult.RunDetails
|> Seq.filter (fun r -> not (isNull r.ValidationMetrics) && not (Double.IsNaN r.ValidationMetrics.RSquared))
|> Seq.sortBy (fun x -> x.ValidationMetrics.RSquared)
|> Seq.truncate 3
|> Seq.iteri
    (fun i x ->
        ConsoleHelper.printRegressionIterationMetrics (i + 1) x.TrainerName x.ValidationMetrics x.RuntimeInSeconds
    )

// STEP 5: Evaluate the model and print metrics
ConsoleHelper.consoleWriteHeader "===== Evaluating model's accuracy with test data ====="
let best = experimentResult.BestRun
let trainedModel = best.Model
let predictions = trainedModel.Transform(testDataView)
let metrics = mlContext.Regression.Evaluate(predictions, labelColumnName = labelColumnName, scoreColumnName = "Score")
// Print metrics from top model
ConsoleHelper.printRegressionMetrics best.TrainerName metrics

// STEP 6: Save/persist the trained model to a .ZIP file
mlContext.Model.Save(trainedModel, trainingDataView.Schema, modelPath)

printfn "The model is saved to %s" modelPath


// Make a single test prediction loading the model from .ZIP file

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

let loadedTrainedModel, modelInputSchema = mlContext.Model.Load(modelPath)

// Create prediction engine related to the loaded trained model
let predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(loadedTrainedModel)

// Score
let predictedResult = predEngine.Predict(taxiTripSample)

printfn "**********************************************************************"
printfn "Predicted fare: %0.4f, actual fare: 15.5" predictedResult.FareAmount
printfn "**********************************************************************"

// Paint regression distribution chart for a number of elements read from a Test DataSet file
plotRegressionChart mlContext 100

printfn "Press any key to exit.."
Console.ReadLine() |> ignore
