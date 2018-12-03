using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Common;
using CustomerSegmentation.DataStructures;
using Microsoft.ML;

namespace CustomerSegmentation.Model
{
    public class ClusteringModelScorer
    {
        private readonly string _pivotDataLocation;

        private readonly string _plotLocation;
        private readonly string _csvlocation;
        private readonly MLContext _mlContext;
        private ITransformer _trainedModel;

        public ClusteringModelScorer(MLContext mlContext, string pivotDataLocation, string plotLocation, string csvlocation)
        {
            _pivotDataLocation = pivotDataLocation;
            _plotLocation = plotLocation;
            _csvlocation = csvlocation;
            _mlContext = mlContext;
        }

        public ITransformer LoadModelFromZipFile(string modelPath)
        {
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _trainedModel = _mlContext.Model.Load(stream);
            }

            return _trainedModel;
        }

        public void CreateCustomerClusters()
        {            
            var reader = new TextLoader(_mlContext,
                new TextLoader.Arguments
                {
                    Column = new[] {
                        new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
                        new TextLoader.Column("LastName", DataKind.Text, 32)
                    },
                    HasHeader = true,
                    Separator = ","
                });

            var data = reader.Read(_pivotDataLocation);

            //Apply data transformation to create predictions/clustering
            var predictions = _trainedModel.Transform(data)
                            .AsEnumerable<ClusteringPrediction>(_mlContext, false)
                            .ToArray();

            //Generate data files with customer data grouped by clusters
            SaveCustomerSegmentationCSV(predictions, _csvlocation);

            //Plot/paint the clusters in a chart and open it with the by-default image-tool in Windows
            SaveCustomerSegmentationPlotChart(predictions, _plotLocation);
            OpenChartInDefaultWindow(_plotLocation);
        }

        private static void SaveCustomerSegmentationCSV(IEnumerable<ClusteringPrediction> predictions, string csvlocation)
        {
            ConsoleHelper.ConsoleWriteHeader("CSV Customer Segmentation");
            using (var w = new System.IO.StreamWriter(csvlocation))
            {
                w.WriteLine($"LastName,SelectedClusterId");
                w.Flush();
                predictions.ToList().ForEach(prediction => {
                    w.WriteLine($"{prediction.LastName},{prediction.SelectedClusterId}");
                    w.Flush();
                });
            }

            Console.WriteLine($"CSV location: {csvlocation}");
        }

        private static void SaveCustomerSegmentationPlotChart(IEnumerable<ClusteringPrediction> predictions, string plotLocation)
        {
            Common.ConsoleHelper.ConsoleWriteHeader("Plot Customer Segmentation");

            var plot = new PlotModel { Title = "Customer Segmentation", IsLegendVisible = true };

            var clusters = predictions.Select(p => p.SelectedClusterId).Distinct().OrderBy(x => x);

            foreach (var cluster in clusters)
            {
                var scatter = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerStrokeThickness = 2, Title = $"Cluster: {cluster}", RenderInLegend=true };
                var series = predictions
                    .Where(p => p.SelectedClusterId == cluster)
                    .Select(p => new ScatterPoint(p.Location[0], p.Location[1])).ToArray();
                scatter.Points.AddRange(series);
                plot.Series.Add(scatter);
            }

            plot.DefaultColors = OxyPalettes.HueDistinct(plot.Series.Count).Colors;

            var exporter = new SvgExporter { Width = 600, Height = 400 };
            using (var fs = new System.IO.FileStream(plotLocation, System.IO.FileMode.Create))
            {
                exporter.Export(plot, fs);
            }

            Console.WriteLine($"Plot location: {plotLocation}");
        }

        private static void OpenChartInDefaultWindow(string plotLocation)
        {
            Console.WriteLine("Showing chart...");
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(plotLocation)
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
