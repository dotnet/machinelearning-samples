module MulticlassClassification_Iris

open System
open System.IO
open System.Diagnostics

open Microsoft.ML.Runtime.Learners
open Microsoft.ML.Runtime.Data
open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Runtime.Api
open Microsoft.ML.Legacy
open Microsoft.ML.Core.FSharp

open PLplot


let AppPath = Path.Combine(__SOURCE_DIRECTORY__, "../../../..")
let TrainDataPath= Path.Combine(AppPath, "datasets", "taxi-fare-train.csv")
let TestDataPath= Path.Combine(AppPath, "datasets", "taxi-fare-test.csv")
let ModelPath= Path.Combine(AppPath, "TaxiFareModel.zip")


[<CLIMutable>]
type TaxiTrip = {
    VendorId : string
    RateCode : string
    PassengerCount : float32
    TripTime : float32
    TripDistance : float32
    PaymentType : string
    FareAmount : float32
}

[<CLIMutable>]
type TaxiTripFarePrediction = {
        [<ColumnName("Score")>]
        FareAmount : float32
    }


let createTaxiFareDataFileLoader mlcontext =
    TextLoader(
        mlcontext, 
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

let buildAndTrain mlcontext =

    // Create the TextLoader by defining the data columns and where to find (column position) them in the text file.
    let textLoader = createTaxiFareDataFileLoader mlcontext

    // Now read the file (remember though, readers are lazy, so the actual reading will happen when 'fitting').
    let dataView = MultiFileSource(TrainDataPath) |> textLoader.Read

    //Copy the Count column to the Label column 


    // In our case, we will one-hot encode as categorical values the VendorId, RateCode and PaymentType
    // Then concatenate that with the numeric columns.
    let pipeline = 
        CopyColumnsEstimator(mlcontext, "FareAmount", "Label")
        |> Pipeline.append(new CategoricalEstimator(mlcontext, "VendorId"))
        |> Pipeline.append(new CategoricalEstimator(mlcontext, "RateCode"))
        |> Pipeline.append(new CategoricalEstimator(mlcontext, "PaymentType"))
        |> Pipeline.append(new Normalizer(mlcontext, "PassengerCount", Normalizer.NormalizerMode.MeanVariance))
        |> Pipeline.append(new Normalizer(mlcontext, "TripTime", Normalizer.NormalizerMode.MeanVariance))
        |> Pipeline.append(new Normalizer(mlcontext, "TripDistance", Normalizer.NormalizerMode.MeanVariance))
        |> Pipeline.append(new ConcatEstimator(mlcontext, "Features", "VendorId", "RateCode", "PassengerCount", "TripTime", "TripDistance", "PaymentType"))
    
    // We apply our selected Trainer (SDCA Regression algorithm)
    let pipelineWithTrainer = 
        pipeline
        |> Pipeline.append(new SdcaRegressionTrainer(mlcontext, new SdcaRegressionTrainer.Arguments(), "Features", "Label"))

    // The pipeline is trained on the dataset that has been loaded and transformed.
    printfn "=============== Training model ==============="

    let model = pipelineWithTrainer.Fit dataView

    model

let evaluate mlcontext testDataLocation (model : ITransformer)=
    
    //Create TextLoader with schema related to columns in the TESTING/EVALUATION data file
    let textLoader = createTaxiFareDataFileLoader mlcontext

    //Load evaluation/test data
    let testDataView = MultiFileSource testDataLocation |> textLoader.Read

    printfn "=============== Evaluating Model's accuracy with Test data==============="

    let predictions = model.Transform testDataView 

    let regressionCtx = RegressionContext mlcontext
    let metrics = regressionCtx.Evaluate(predictions, "Label", "Score")
    let algorithmName = "SdcaRegressionTrainer"
    printfn "*************************************************"
    printfn "*       Metrics for %s" algorithmName
    printfn "*------------------------------------------------"
    printfn "*       R2 Score: %.2f" metrics.RSquared
    printfn "*       RMS loss: %.2f" metrics.Rms
    printfn "*       Absolute loss: %.2f" metrics.L1
    printfn "*       Squared loss: %.2f" metrics.L2
    printfn "*************************************************"

    metrics

let testSinglePrediction  mlcontext (model : ITransformer) =
    //Prediction test
    // Create prediction engine and make prediction.
    let engine = model.MakePredictionFunction<TaxiTrip, TaxiTripFarePrediction> mlcontext

    //Sample: 
    //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
    //VTS,1,1,1140,3.75,CRD,15.5
    let taxiTripSample = {
            VendorId = "VTS"
            RateCode = "1"
            PassengerCount = 1.0f
            TripTime = 1140.0f
            TripDistance = 3.75f
            PaymentType = "CRD"
            FareAmount = 0.0f // To predict. Actual/Observed = 15.5
        }

    let prediction = engine.Predict taxiTripSample
    printfn "**********************************************************************"
    printfn "Predicted fare: %.4f, actual fare: 29.5" prediction.FareAmount
    printfn "**********************************************************************"

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

let plotRegressionChart (model : ITransformer) testDataSetPath numberOfRecordsToRead (args : string array )=
    //Create the Prediction Function
    use mlcontext = new LocalEnvironment()

    // Create prediction engine 
    let engine = model.MakePredictionFunction<TaxiTrip, TaxiTripFarePrediction> mlcontext

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
    let xMaxLimit = 40. //Rides larger than $40 are not shown in the chart
    let yMinLimit = 0.
    let yMaxLimit = 40. //Rides larger than $40 are not shown in the chart
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
        let farePrediction = engine.Predict testData.[i]
  
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

    //1. Create ML.NET context/environment
    let mlcontext = new LocalEnvironment(seed = Nullable 0)

    // STEP 1: Create and train a model
    let model = buildAndTrain mlcontext

    // STEP2: Evaluate accuracy of the model
    evaluate mlcontext TestDataPath model |> ignore

    // STEP 3: Make a test prediction
    testSinglePrediction mlcontext model


    //STEP 4: Paint regression distribution chart for a number of elements read from a Test DataSet file
    plotRegressionChart model TestDataPath 100 argv


    printfn "Press any key to exit.."
    Console.ReadLine() |> ignore

    0
