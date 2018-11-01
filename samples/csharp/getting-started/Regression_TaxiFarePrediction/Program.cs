using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PLplot;
using System.Diagnostics;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML;
using Microsoft.ML.Core.Data;

namespace Regression_TaxiFarePrediction
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "taxi-fare-train.csv");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "taxi-fare-test.csv");
        private static string ModelPath => Path.Combine(AppPath, "TaxiFareModel.zip");

        static void Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {
            //Create ML Context with seed for repeteable/deterministic results
            LocalEnvironment mlcontext = new LocalEnvironment(seed: 0);

            // STEP 1: Create and train a model
            var model = BuildAndTrain(mlcontext);

            // STEP2: Evaluate accuracy of the model
            Evaluate(mlcontext, TestDataPath, model);

            // STEP 3: Make a test prediction
            TestSinglePrediction(mlcontext, model);

            //STEP 4: Paint regression distribution chart for a number of elements read from a Test DataSet file
            PlotRegressionChart(model, TestDataPath, 100, args);

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static TextLoader CreateTaxiFareDataFileLoader(LocalEnvironment mlcontext)
        {
            return new TextLoader(mlcontext,
                                                      new TextLoader.Arguments()
                                                      {
                                                          Separator = ",",
                                                          HasHeader = true,
                                                          Column = new[]
                                                          {
                                                    new TextLoader.Column("VendorId", DataKind.Text, 0),
                                                    new TextLoader.Column("RateCode", DataKind.Text, 1),
                                                    new TextLoader.Column("PassengerCount", DataKind.R4, 2),
                                                    new TextLoader.Column("TripTime", DataKind.R4, 3),
                                                    new TextLoader.Column("TripDistance", DataKind.R4, 4),
                                                    new TextLoader.Column("PaymentType", DataKind.Text, 5),
                                                    new TextLoader.Column("FareAmount", DataKind.R4, 6)
                                                          }
                                                      });
        }

        private static ITransformer BuildAndTrain(LocalEnvironment mlcontext)
        {
            // Create the TextLoader by defining the data columns and where to find (column position) them in the text file.
            TextLoader textLoader = CreateTaxiFareDataFileLoader(mlcontext);

            // Now read the file (remember though, readers are lazy, so the actual reading will happen when 'fitting').
            IDataView dataView = textLoader.Read(new MultiFileSource(TrainDataPath));

            //Copy the Count column to the Label column 

            // In our case, we will one-hot encode as categorical values the VendorId, RateCode and PaymentType
            // Then concatenate that with the numeric columns.
            var pipeline = new CopyColumnsEstimator(mlcontext, "FareAmount", "Label")
                                    .Append(new CategoricalEstimator(mlcontext, "VendorId"))
                                    .Append(new CategoricalEstimator(mlcontext, "RateCode"))
                                    .Append(new CategoricalEstimator(mlcontext, "PaymentType"))
                                    .Append(new Normalizer(mlcontext, "PassengerCount", Normalizer.NormalizerMode.MeanVariance))
                                    .Append(new Normalizer(mlcontext, "TripTime", Normalizer.NormalizerMode.MeanVariance))
                                    .Append(new Normalizer(mlcontext, "TripDistance", Normalizer.NormalizerMode.MeanVariance))
                                    .Append(new ConcatEstimator(mlcontext, "Features", "VendorId", "RateCode", "PassengerCount", "TripTime", "TripDistance", "PaymentType"));

            // We apply our selected Trainer (SDCA Regression algorithm)
            var pipelineWithTrainer = pipeline.Append(new SdcaRegressionTrainer(mlcontext, new SdcaRegressionTrainer.Arguments(),
                                                                                "Features", "Label"));

            // The pipeline is trained on the dataset that has been loaded and transformed.
            Console.WriteLine("=============== Training model ===============");
            var model = pipelineWithTrainer.Fit(dataView);

            return model;
        }

        private static RegressionEvaluator.Result Evaluate(LocalEnvironment mlcontext,
                                                           string testDataLocation,
                                                           ITransformer model
                                                          )
        {
            //Create TextLoader with schema related to columns in the TESTING/EVALUATION data file
            TextLoader textLoader = CreateTaxiFareDataFileLoader(mlcontext);

            //Load evaluation/test data
            IDataView testDataView = textLoader.Read(new MultiFileSource(testDataLocation));

            Console.WriteLine("=============== Evaluating Model's accuracy with Test data===============");
            var predictions = model.Transform(testDataView);

            var regressionCtx = new RegressionContext(mlcontext);
            var metrics = regressionCtx.Evaluate(predictions, "Label", "Score");
            var algorithmName = "SdcaRegressionTrainer";
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for {algorithmName}          ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       R2 Score: {metrics.RSquared:0.##}");
            Console.WriteLine($"*       RMS loss: {metrics.Rms:#.##}");
            Console.WriteLine($"*       Absolute loss: {metrics.L1:#.##}");
            Console.WriteLine($"*       Squared loss: {metrics.L2:#.##}");
            Console.WriteLine($"*************************************************");

            return metrics;
        }

        private static void TestSinglePrediction(LocalEnvironment mlcontext, ITransformer model)
        {
            //Prediction test
            // Create prediction engine and make prediction.
            var engine = model.MakePredictionFunction<TaxiTrip, TaxiTripFarePrediction>(mlcontext);

            //Sample: 
            //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
            //VTS,1,1,1140,3.75,CRD,15.5

            var taxiTripSample = new TaxiTrip()
            {
                VendorId = "VTS",
                RateCode = "1",
                PassengerCount = 1,
                TripTime = 1140,
                TripDistance = 3.75f,
                PaymentType = "CRD",
                FareAmount = 0 // To predict. Actual/Observed = 15.5
            };

            var prediction = engine.Predict(taxiTripSample);
                Console.WriteLine($"**********************************************************************");
                Console.WriteLine($"Predicted fare: {prediction.FareAmount:0.####}, actual fare: 29.5");
                Console.WriteLine($"**********************************************************************");
        }

        private static void PlotRegressionChart(ITransformer model,
                                                string testDataSetPath,
                                                int numberOfRecordsToRead,
                                                string[] args)
        {
            //Create the Prediction Function
            var mlcontext = new LocalEnvironment();
            // Create prediction engine 
            var engine = model.MakePredictionFunction<TaxiTrip, TaxiTripFarePrediction>(mlcontext);
            //var prediction = engine.Predict(taxiTripSample);


            string chartFileName = "";

            using (var pl = new PLStream())
            {
                // use SVG backend and write to SineWaves.svg in current directory
                if (args.Length == 1 && args[0] == "svg")
                {
                    pl.sdev("svg");
                    chartFileName = "TaxiRegressionDistribution.svg";
                    pl.sfnam(chartFileName);
                }
                else
                {
                    pl.sdev("pngcairo");
                    chartFileName = "TaxiRegressionDistribution.png";
                    pl.sfnam(chartFileName);
                }

                // use white background with black foreground
                pl.spal0("cmap0_alternate.pal");

                // Initialize plplot
                pl.init();

                // set axis limits
                const int xMinLimit = 0;
                const int xMaxLimit = 40; //Rides larger than $40 are not shown in the chart
                const int yMinLimit = 0;
                const int yMaxLimit = 40;  //Rides larger than $40 are not shown in the chart
                pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);

                // Set scaling for mail title text 125% size of default
                pl.schr(0, 1.25);

                // The main title
                pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction");

                // plot using different colors
                // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
                pl.col0(1);

                int totalNumber = numberOfRecordsToRead;
                var testData = new TaxiTripCsvReader().GetDataFromCsv(testDataSetPath, totalNumber).ToList();

                //This code is the symbol to paint
                char code = (char)9;

                // plot using other color
                //pl.col0(9); //Light Green
                //pl.col0(4); //Red
                pl.col0(2); //Blue

                double yTotal = 0;
                double xTotal = 0;
                double xyMultiTotal = 0;
                double xSquareTotal = 0;

                for (int i = 0; i < testData.Count; i++)
                {
                    var x = new double[1];
                    var y = new double[1];

                    //Make Prediction
                    var FarePrediction = engine.Predict(testData[i]);
  
                    x[0] = testData[i].FareAmount;
                    y[0] = FarePrediction.FareAmount;

                    //Paint a dot
                    pl.poin(x, y, code);

                    xTotal += x[0];
                    yTotal += y[0];

                    double multi = x[0] * y[0];
                    xyMultiTotal += multi;

                    double xSquare = x[0] * x[0];
                    xSquareTotal += xSquare;

                    double ySquare = y[0] * y[0];

                    Console.WriteLine($"-------------------------------------------------");
                    Console.WriteLine($"Predicted : {FarePrediction.FareAmount}");
                    Console.WriteLine($"Actual:    {testData[i].FareAmount}");
                    Console.WriteLine($"-------------------------------------------------");
                }

                // Regression Line calculation explanation:
                // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

                double minY = yTotal / totalNumber;
                double minX = xTotal / totalNumber;
                double minXY = xyMultiTotal / totalNumber;
                double minXsquare = xSquareTotal / totalNumber;

                double m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare);

                double b = minY - (m * minX);

                //Generic function for Y for the regression line
                // y = (m * x) + b;

                double x1 = 1;
                //Function for Y1 in the line
                double y1 = (m * x1) + b;

                double x2 = 39;
                //Function for Y2 in the line
                double y2 = (m * x2) + b;

                var xArray = new double[2];
                var yArray = new double[2];
                xArray[0] = x1;
                yArray[0] = y1;
                xArray[1] = x2;
                yArray[1] = y2;

                pl.col0(4);
                pl.line(xArray, yArray);

                // end page (writes output to disk)
                pl.eop();

                // output version of PLplot
                pl.gver(out var verText);
                Console.WriteLine("PLplot version " + verText);

            } // the pl object is disposed here

            // Open Chart File In Microsoft Photos App (Or default app, like browser for .svg)

            Console.WriteLine("Showing chart...");
            var p = new Process();
            string chartFileNamePath = @".\" + chartFileName;
            p.StartInfo = new ProcessStartInfo(chartFileNamePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }

    }

    public class TaxiTripCsvReader
    {
        public IEnumerable<TaxiTrip> GetDataFromCsv(string dataLocation, int numMaxRecords)
        {
            IEnumerable<TaxiTrip> records =
                File.ReadAllLines(dataLocation)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new TaxiTrip()
                {
                    VendorId = x[0],
                    RateCode = x[1],
                    PassengerCount = float.Parse(x[2]),
                    TripTime = float.Parse(x[3]),
                    TripDistance = float.Parse(x[4]),
                    PaymentType = x[5],
                    FareAmount = float.Parse(x[6])
                })
                .Take<TaxiTrip>(numMaxRecords);

            return records;
        }
    }

}
