using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Models;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;

using System.Collections.Generic;
using System.Linq;

using PLplot;
using System.Diagnostics;

namespace Regression_TaxiFarePrediction
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "taxi-fare-train.csv");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "taxi-fare-test.csv");
        private static string ModelPath => Path.Combine(AppPath, "TaxiFareModel.zip");

        private static async Task Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {
            // STEP 1: Create a model
            var model = await TrainAsync();

            // STEP2: Test accuracy
            Evaluate(model);

            // STEP 3: Make a test prediction
            var prediction = model.Predict(TestTaxiTrips.Trip1);
            Console.WriteLine($"Predicted fare: {prediction.FareAmount:0.####}, actual fare: 29.5");

            //STEP 4: Paint regression distribution chart for a number of elements read from a Test DataSet file
            PaintChart(model, TestDataPath, 100, args);

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static async Task<PredictionModel<TaxiTrip, TaxiTripFarePrediction>> TrainAsync()
        {
            // LearningPipeline holds all steps of the learning process: data, transforms, learners.
            var pipeline = new LearningPipeline
            {
                // The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
                // all the column names and their types.
                new TextLoader(TrainDataPath).CreateFrom<TaxiTrip>(separator:','),
                
                // Transforms
                // When ML model starts training, it looks for two columns: Label and Features.
                // Label:   values that should be predicted. If you have a field named Label in your data type,
                //              no extra actions required.
                //          If you don't have it, like in this example, copy the column you want to predict with
                //              ColumnCopier transform:
                new ColumnCopier(("FareAmount", "Label")),
                
                // CategoricalOneHotVectorizer transforms categorical (string) values into 0/1 vectors
                new CategoricalOneHotVectorizer("VendorId",
                    "RateCode",
                    "PaymentType"),
                // Features: all data used for prediction. At the end of all transforms you need to concatenate
                //              all columns except the one you want to predict into Features column with
                //              ColumnConcatenator transform:
                new ColumnConcatenator("Features",
                    "VendorId",
                    "RateCode",
                    "PassengerCount",
                    "TripDistance",
                    "PaymentType"),
                //FastTreeRegressor is an algorithm that will be used to train the model.
                new FastTreeRegressor()
            };

            Console.WriteLine("=============== Training model ===============");
            // The pipeline is trained on the dataset that has been loaded and transformed.
            var model = pipeline.Train<TaxiTrip, TaxiTripFarePrediction>();

            // Saving the model as a .zip file.
            await model.WriteAsync(ModelPath);

            Console.WriteLine("=============== End training ===============");
            Console.WriteLine("The model is saved to {0}", ModelPath);

            return model;
        }

        private static void Evaluate(PredictionModel<TaxiTrip, TaxiTripFarePrediction> model)
        {
            // To evaluate how good the model predicts values, it is run against new set
            // of data (test data) that was not involved in training.
            var testData = new TextLoader(TestDataPath).CreateFrom<TaxiTrip>(separator: ',');

            // RegressionEvaluator calculates the differences (in various metrics) between predicted and actual
            // values in the test dataset.
            var evaluator = new RegressionEvaluator();

            Console.WriteLine("=============== Evaluating model ===============");

            var metrics = evaluator.Evaluate(model, testData);

            Console.WriteLine($"Rms = {metrics.Rms}, ideally should be around 2.8, can be improved with larger dataset");
            Console.WriteLine($"RSquared = {metrics.RSquared}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine("=============== End evaluating ===============");
            Console.WriteLine();
        }

        private static void PaintChart(PredictionModel<TaxiTrip, TaxiTripFarePrediction> model,
                                       string testDataSetPath,
                                       int numberOfRecordsToRead,
                                       string[] args)
        {
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
                    var FarePrediction = model.Predict(testData[i]);

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