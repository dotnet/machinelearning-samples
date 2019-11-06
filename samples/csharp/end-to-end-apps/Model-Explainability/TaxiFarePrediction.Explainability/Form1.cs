using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.IO;


namespace TaxiFareRegression.Explainability
{
    public partial class Form1 : Form
    {
        int _predictionIndex = 0;
        
        static List<DataStructures.TaxiFarePrediction> predictions = GetTaxiFare.Predictions();
        int _resultCount= predictions.Count()-1;

        public Form1()
        {
            this.InitializeComponent();
            PaintChart();
        }

        void PaintChart()
        {
            PlotModel chart = Chart.GetPlotModel(predictions[_predictionIndex]);
            lblTripID.Text = (_predictionIndex + 1).ToString();
            string predictedAmount = String.Format("{0:C}", Convert.ToDecimal(predictions[_predictionIndex].FareAmount));
            lblFare.Text = predictedAmount;
            this.plot1.Model = chart;
        }

        internal static class Chart
        {
            public static PlotModel GetPlotModel(DataStructures.TaxiFarePrediction prediction)
            {
                var model = new PlotModel { Title = "Taxi Fare Prediction Impact per Feature" };

                var barSeries = new BarSeries
                {
                    ItemsSource = new List<BarItem>(new[]
                        {
                        new BarItem{ Value = (prediction.Features[0].Value)},
                        new BarItem{ Value = (prediction.Features[1].Value)},
                        new BarItem{ Value = (prediction.Features[2].Value)}
                    }),
                    LabelPlacement = LabelPlacement.Inside,
                    LabelFormatString = "{0:.00}"
                };

                model.Series.Add(barSeries);

                model.Axes.Add(new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    Key = "FeatureAxis",
                    ItemsSource = new[]
                    {
                        prediction.Features[0].Name,
                        prediction.Features[1].Name,
                        prediction.Features[2].Name
                }
                });

                return model;
            }

            
        }
        internal static class GetTaxiFare
        {
            private static string BaseRelativePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TaxiFarePrediction", "TaxiFarePredictionConsoleApp");
            private static string BaseDataPath = Path.Combine(Path.GetFullPath(BaseRelativePath), "inputs");
            private static string TestDataPath = Path.Combine(BaseDataPath, "taxi-fare-test.csv");
            private static string ModelPath = Path.Combine(BaseRelativePath, "outputs", "TaxiFareModel.zip");

            public static List<DataStructures.TaxiFarePrediction> Predictions()
            {
                var modelPredictor = new Predictor(ModelPath, TestDataPath);
                List<DataStructures.TaxiFarePrediction> predictions = modelPredictor.RunMultiplePredictions(numberOfPredictions: 20);
                return predictions;
                //Console.WriteLine(JsonConvert.SerializeObject(predictions, Formatting.Indented));

            }


        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Plot1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (_predictionIndex < _resultCount)
            {
                _predictionIndex++;
                PaintChart();
            }
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void Label1_Click_1(object sender, EventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (_predictionIndex > 0)
            {
                _predictionIndex--;
                PaintChart();
            }
        }
    }
}
