open System
open System.IO
open System.Diagnostics
open PLplot
open TaxiFarePrediction.DataStructures.DataStructures
open Microsoft.ML
open Microsoft.ML.Runtime.Data
open Microsoft.ML.Transforms
open Microsoft.ML.Transforms.Normalizers

let appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs().[0])

let baseDatasetsLocation = @"../../../../Data"
let trainDataPath = sprintf @"%s/taxi-fare-train.csv" baseDatasetsLocation
let testDataPath = sprintf @"%s/taxi-fare-test.csv" baseDatasetsLocation

let baseModelsPath = @"../../../../MLModels"
let modelPath = sprintf @"%s/TaxiFareModel.zip" baseModelsPath

let dataLoader (mlContext : MLContext) =
    mlContext.Data.TextReader(
        TextLoader.Arguments(
            Separator = ",",
            HasHeader = true,
            Column = 
                [|
                    TextLoader.Column("VendorId", Nullable DataKind.Text, 0)
                    TextLoader.Column("RateCode", Nullable DataKind.Text, 1)
                    TextLoader.Column("PassengerCount", Nullable DataKind.R4, 2)
                    TextLoader.Column("TripTime", Nullable DataKind.R4, 3)
                    TextLoader.Column("TripDistance", Nullable DataKind.R4, 4)
                    TextLoader.Column("PaymentType", Nullable DataKind.Text, 5)
                    TextLoader.Column("FareAmount", Nullable DataKind.R4, 6)
                |]
        )
    )

let read (dataPath : string) (dataLoader : TextLoader) =
    dataLoader.Read dataPath

let buildTrainEvaluateAndSaveModel (mlContext : MLContext) =
    
    // STEP 1: Common data loading configuration
    let trainingDataView = 
        dataLoader mlContext
        |> read trainDataPath

    let testDataView = 
        dataLoader mlContext
        |> read testDataPath

    // STEP 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline =
        CopyColumnsEstimator(mlContext, "FareAmount", "Label")
        |> Common.ModelBuilder.append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId", "VendorIdEncoded"))
        |> Common.ModelBuilder.append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode", "RateCodeEncoded"))
        |> Common.ModelBuilder.append(mlContext.Transforms.Categorical.OneHotEncoding("PaymentType", "PaymentTypeEncoded"))
        |> Common.ModelBuilder.append(mlContext.Transforms.Normalize(inputName = "PassengerCount", mode = NormalizingEstimator.NormalizerMode.MeanVariance))
        |> Common.ModelBuilder.append(mlContext.Transforms.Normalize(inputName = "TripTime", mode = NormalizingEstimator.NormalizerMode.MeanVariance))
        |> Common.ModelBuilder.append(mlContext.Transforms.Normalize(inputName = "TripDistance", mode = NormalizingEstimator.NormalizerMode.MeanVariance))
        |> Common.ModelBuilder.append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PaymentTypeEncoded", "PassengerCount", "TripTime", "TripDistance"))
        |> Common.ConsoleHelper.downcastPipeline

    // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<TaxiTrip> mlContext trainingDataView dataProcessPipeline 5 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 5 |> ignore

    // STEP 3: Set the training algorithm, then create and config the modelBuilder
    let trainer = mlContext.Regression.Trainers.StochasticDualCoordinateAscent(label = "Label", features = "Features")
    let modelBuilder = 
        Common.ModelBuilder.create mlContext dataProcessPipeline
        |> Common.ModelBuilder.addTrainer trainer

    // STEP 4: Train the model fitting to the DataSet
    printfn "=============== Training the model ==============="
    let trainedModel = 
        modelBuilder
        |> Common.ModelBuilder.train trainingDataView

    // STEP 5: Evaluate the model and show accuracy stats
    printfn "===== Evaluating Model's accuracy with Test data ====="
    let metrics = 
        (trainedModel, modelBuilder)
        |> Common.ModelBuilder.evaluateRegressionModel testDataView "Label" "Score"

    Common.ConsoleHelper.printRegressionMetrics (trainer.ToString()) metrics

    // STEP 6: Save/persist the trained model to a .ZIP file
    printfn "=============== Saving the model to a file ==============="
    (trainedModel, modelBuilder)
    |> Common.ModelBuilder.saveModelAsFile modelPath


let testSinglePrediction (mlContext : MLContext) =
    //Sample: 
    //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
    //VTS,1,1,1140,3.75,CRD,15.5
    let taxiTripSample = 
        {
            VendorId = "VTS"
            RateCode = "1"
            PassengerCount = 1.0f
            TripTime = 1140.0f
            TripDistance = 3.75f
            PaymentType = "CRD"
            FareAmount = 0.0f // To predict. Actual/Observed = 15.5
        };

    let resultprediction = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.loadModelFromZipFile modelPath
        |> Common.ModelScorer.predictSingle taxiTripSample

    printfn "=============== Single Prediction  ==============="
    printfn "Predicted fare: %.4f, actual fare: 15.5" resultprediction.FareAmount
    printfn "=================================================="


let plotRegressionChart (mlContext : MLContext) testDataSetPath numberOfRecordsToRead args =
    let getDataFromCsv dataLocation numMaxRecords =
        File.ReadAllLines(dataLocation)
        |> Array.skip 1
        |> Array.map(fun x -> x.Split(','))
        |> Array.map(fun x ->
                {
                    VendorId = x.[0]
                    RateCode = x.[1]
                    PassengerCount = Single.Parse(x.[2])
                    TripTime = Single.Parse(x.[3])
                    TripDistance = Single.Parse(x.[4])
                    PaymentType = x.[5]
                    FareAmount = Single.Parse(x.[6])
                }
            )
        |> Array.take numMaxRecords
    
    let modelScorer = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.loadModelFromZipFile modelPath

    use pl = new PLStream()
    // use SVG backend and write to SineWaves.svg in current directory
    let chartFileName =
        match args with
        | [| "svg" |] ->
            pl.sdev "svg"
            "TaxiRegressionDistribution.svg"
        | _ ->
            pl.sdev "pngcairo"
            "TaxiRegressionDistribution.png"
    pl.sfnam chartFileName

    // use white background with black foreground
    pl.spal0 "cmap0_alternate.pal"

    // Initialize plplot
    pl.init ()

    // set axis limits
    let xMinLimit = 0.
    let xMaxLimit = 35. //Rides larger than 35 are not shown in the chart
    let yMinLimit = 0.
    let yMaxLimit = 35. //Rides larger than 35 are not shown in the chart
    pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes)

    // Set scaling for main title text 125% size of default
    pl.schr(0., 1.25)

    // The main title
    pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction")

    // plot using different colors
    // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
    pl.col0 1

    let totalNumber = numberOfRecordsToRead
    let testData = getDataFromCsv testDataSetPath totalNumber

    //This code is the symbol to paint
    let code = (char)9

    // plot using other color
    //pl.col0 9   //Light Green
    //pl.col0 4   //Red
    pl.col0 2     //Blue

    let mutable yTotal = 0.
    let mutable xTotal = 0.
    let mutable xyMultiTotal = 0.
    let mutable xSquareTotal = 0.

    for i in 0 .. testData.Length - 1 do

        //Make Prediction
        let farePrediction = modelScorer |> Common.ModelScorer.predictSingle testData.[i]
  
        let x = [| float testData.[i].FareAmount |]
        let y = [| float farePrediction.FareAmount |]

        //Paint a dot
        pl.poin(x, y, code)

        xTotal <- xTotal + float x.[0]
        yTotal <- yTotal + float y.[0]

        let multi = x.[0] * y.[0]
        xyMultiTotal <-  xyMultiTotal + multi

        let xSquare = x.[0] * x.[0]
        xSquareTotal <- xSquareTotal + xSquare

        let ySquare = y.[0] * y.[0]

        printfn "-------------------------------------------------"
        printfn "Predicted : %.5f" farePrediction.FareAmount
        printfn "Actual:     %.2f" testData.[i].FareAmount
        printfn "-------------------------------------------------"

    // Regression Line calculation explanation:
    // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

    let minY = yTotal / float totalNumber
    let minX = xTotal / float totalNumber
    let minXY = xyMultiTotal / float totalNumber
    let minXsquare = xSquareTotal / float totalNumber

    let m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare)

    let b = minY - (m * minX)

    //Generic function for Y for the regression line
    // y = (m * x) + b

    let x1 = 1.
    //Function for Y1 in the line
    let y1 = (m * x1) + b

    let x2 = 39.
    //Function for Y2 in the line
    let y2 = (m * x2) + b

    let xArray = [| x1; x2 |]
    let yArray = [| y1; y2 |]

    pl.col0 4
    pl.line(xArray, yArray)

    // end page (writes output to disk)
    pl.eop()

    // output version of PLplot
    let verText = pl.gver()
    printfn "PLplot version %s" verText
 


    // Open Chart File In Microsoft Photos App (Or default app, like browser for .svg)
    printfn "Showing chart..."
    let chartFileNamePath = @".\" + chartFileName
    let p = new Process(StartInfo=ProcessStartInfo(chartFileNamePath, UseShellExecute = true))
    p.Start() |> ignore


[<EntryPoint>]
let main argv =

    //Create ML Context with seed for repeteable/deterministic results
    let mlContext = new MLContext(seed = Nullable 0)

    // Create, Train, Evaluate and Save a model
    buildTrainEvaluateAndSaveModel mlContext

    // Make a single test prediction loding the model from .ZIP file
    testSinglePrediction mlContext

    // Paint regression distribution chart for a number of elements read from a Test DataSet file
    plotRegressionChart mlContext testDataPath 100 argv

    Common.ConsoleHelper.consolePressAnyKey()


    0 // return an integer exit code
