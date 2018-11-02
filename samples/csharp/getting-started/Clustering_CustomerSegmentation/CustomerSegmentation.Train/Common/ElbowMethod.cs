using CustomerSegmentation.DataStructures;
using CustomerSegmentation.Train.DataStructures;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers.KMeans;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common
{
    public static class ElbowMethod
    {
        public static void CalculateK(MLContext mlContext, IEstimator<ITransformer> DataProcessPipeline, IDataView pivotDataView, string plotLocation, int maxK = 20)
        {
            Common.ConsoleHelper.ConsoleWriteHeader("Calculate best K value");
            var kValues = new Dictionary<int, double>();
            for (int k = 2; k <= maxK; k++)
            {
                var trainer = new KMeansPlusPlusTrainer(mlContext, "Features", clustersCount: k);
                var modelBuilder = new Common.ModelBuilder<PivotObservation, ClusteringPrediction>(mlContext, DataProcessPipeline, trainer);
                var trainedModel = modelBuilder.Train(pivotDataView);
                Console.WriteLine($"Building model for k={k}");
                var metrics = modelBuilder.EvaluateClusteringModel(pivotDataView);
                var loss = metrics.AvgMinScore;
                kValues.Add(k, loss);
            }
            PlotKValues(kValues, plotLocation);
        }

        public static void PlotKValues(Dictionary<int, double> kValues, string plotLocation)
        {
            Console.Out.WriteLine("Plot Customer Segmentation");
            var plot = new PlotModel { Title = "elbow method", IsLegendVisible = true };
            var lineSeries = new LineSeries() { Title = $"kValues ({kValues.Keys.Max()})" };
            foreach (var item in kValues)
                lineSeries.Points.Add(new DataPoint(item.Key, item.Value));
            plot.Series.Add(lineSeries);
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = -0.1, Title = "k" });
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = -0.1, Title = "loss" });
            var exporter = new SvgExporter { Width = 600, Height = 400 };
            using (var fs = new FileStream(plotLocation, System.IO.FileMode.Create))
            {
                exporter.Export(plot, fs);
            }
        }
    }
}
