using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PLplot;
using System.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Auto;
using Microsoft.ML.Data;
using Regression_AutoML.DataStructures;
using Common;
using System.Threading;

namespace Regression_TaxiFarePrediction
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsRelativePath = @"Data";
        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-train.csv";
        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TrainDataSmallRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-train-small.csv";
        private static string TrainDataSmallPath = GetAbsolutePath(TrainDataSmallRelativePath);
        private static string TestDataRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-test.csv";
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/TaxiFareModel.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        private static string LabelColumnName = "fare_amount";
        private static uint ExperimentTime = 5;

        static void Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {
            MLContext mlContext = new MLContext();

            // Create, train, evaluate and save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loading the model from .ZIP file
            TestSinglePrediction(mlContext);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file
            PlotRegressionChart(mlContext, TestDataPath, 100, args);

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Infer columns in the dataset
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(TrainDataPath, LabelColumnName, groupColumns: false);
            ConsoleHelper.Print(columnInference);
            
            // STEP 2: Load data
            TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
            // Send a subsample of the full training data to AutoML for faster experimentation time
            IDataView smallTrainingDataView = textLoader.Load(TrainDataSmallPath);
            IDataView testDataView = textLoader.Load(TestDataPath);

            // STEP 3: Display first few rows of the training data
            ConsoleHelper.ShowDataViewInConsole(mlContext, smallTrainingDataView);

            // STEP 4: Build a pre-featurizer for use in the AutoML experiment.
            // (Internally, AutoML uses one or more train/validation data splits to 
            // evaluate the models it produces. The pre-featurizer is fit only on the 
            // training data split to produce a trained transform. Then, the trained transform 
            // is applied to both the train and validation data splits.)
            IEstimator<ITransformer> preFeaturizer = mlContext.Transforms.Conversion.MapValue("is_cash",
                new[] { new KeyValuePair<string, bool>("CSH", true) }, "payment_type");

            // STEP 5: Initialize custom column information for use in AutoML experiment
            ColumnInformation columnInformation = new ColumnInformation() { LabelColumnName = LabelColumnName };
            // Indicate that the VendorId column should be treated as categorical data.
            // (Categorical data columns should generally be columns that contain a small number of unique values.)
            columnInformation.CategoricalColumnNames.Add("vendor_id");
            columnInformation.IgnoredColumnNames.Add("payment_type");

            // STEP 6: Initialize AutoML experiment settings.
            var experimentSettings = new RegressionExperimentSettings();
            experimentSettings.MaxExperimentTimeInSeconds = ExperimentTime;
            // Set the metric that AutoML will try to optimize over the course of the experiment.
            experimentSettings.OptimizingMetric = RegressionMetric.MeanSquaredError;
            // Only use the LightGBM regression trainer during the experiment
            experimentSettings.Trainers.Clear();
            experimentSettings.Trainers.Add(RegressionTrainer.LightGbm);

            // STEP 7: Set up a cancellation token to stop the experiment.
            CancellationTokenSource cts = new CancellationTokenSource();
            experimentSettings.CancellationToken = cts.Token;
            // Cancel the experiment after the specified # of seconds
            cts.CancelAfter(TimeSpan.FromSeconds(ExperimentTime));

            // STEP 8: Initialize our user-defined progress handler that will AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RegressionExperimentProgressHandler();

            // STEP 9: Run AutoML regression experiment
            Console.WriteLine("=============== Training the model ===============");
            Console.WriteLine($"Running AutoML regression experiment for {ExperimentTime} seconds...");
            ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
                .CreateRegressionExperiment(experimentSettings)
                .Execute(smallTrainingDataView, columnInformation, preFeaturizer, progressHandler);
            Console.WriteLine($"AutoML experiment completed.{Environment.NewLine}");

            // STEP 10: Refit best model on entire training data.
            RunDetail<RegressionMetrics> best = experimentResult.BestRun;
            IDataView trainingDataView = textLoader.Load(TrainDataPath);
            var refitBestModel = best.Estimator.Fit(trainingDataView);

            // STEP 11: Evaluate the model and show metrics.
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            IDataView predictions = refitBestModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
            // Print metrics from top model
            ConsoleHelper.PrintRegressionMetrics(best.TrainerName.ToString(), metrics);

            // STEP 12: Save/persist the refit best model to a .ZIP file
            mlContext.Model.Save(refitBestModel, trainingDataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return refitBestModel;
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            //Sample: 
            //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
            //VTS,1,1,1140,3.75,CRD,15.5

            var taxiTripSample = new TaxiTrip()
            {
                VendorId = "VTS",
                RateCode = 1,
                PassengerCount = 1,
                TripTime = 1140,
                TripDistance = 3.75f,
                PaymentType = "CRD",
                FareAmount = 0 // To predict. Actual/Observed = 15.5
            };

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

            // Score
            var resultprediction = predEngine.Predict(taxiTripSample);

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {resultprediction.FareAmount:0.####}, actual fare: 15.5");
            Console.WriteLine($"**********************************************************************");
        }

        private static void PlotRegressionChart(MLContext mlContext,                                               
                                                string testDataSetPath,
                                                int numberOfRecordsToRead,
                                                string[] args)
        {
            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
            }

            // Create prediction engine related to the loaded trained model
            var predFunction = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

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
                const int xMaxLimit = 35; //Rides larger than $35 are not shown in the chart
                const int yMinLimit = 0;
                const int yMaxLimit = 35;  //Rides larger than $35 are not shown in the chart
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
                    var FarePrediction = predFunction.Predict(testData[i]);

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

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
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
                    RateCode = float.Parse(x[1]),
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
