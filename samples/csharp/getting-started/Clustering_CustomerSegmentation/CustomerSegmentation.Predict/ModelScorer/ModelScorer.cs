using CustomerSegmentation.RetailData;
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
using static CustomerSegmentation.Model.ConsoleHelpers;

namespace CustomerSegmentation.Model
{
    public class ModelScorer
    {
        private readonly string pivotDataLocation;
        private readonly string modelLocation;
        private readonly string plotLocation;
        private readonly string csvlocation;
        private readonly LocalEnvironment mlContext;

        public ModelScorer(string pivotDataLocation, string modelLocation, string plotLocation, string csvlocation)
        {
            this.pivotDataLocation = pivotDataLocation;
            this.modelLocation = modelLocation;
            this.plotLocation = plotLocation;
            this.csvlocation = csvlocation;
            mlContext = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic results
        }

        public void CreateCustomerClusters()
        {
            ITransformer model;
            using (var file = File.OpenRead(modelLocation))
            {
                model = TransformerChain
                    .LoadFrom(mlContext, file);
            }
            
            var reader = new TextLoader(mlContext,
                new TextLoader.Arguments
                {
                    Column = new[] {
                        new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
                        new TextLoader.Column("LastName", DataKind.Text, 32)
                    },
                    HasHeader = true,
                    Separator = ","
                });

            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            var data = reader.Read(new MultiFileSource(pivotDataLocation));

            var predictions = model.Transform(data)
                            .AsEnumerable<ClusteringPrediction>(mlContext, false)
                            .ToArray();

            //Generate data files with customer data grouped by clusters
            SaveCustomerSegmentationCSV(predictions, csvlocation);

            //Plot/paint the clusters in a chart and open it with the by-default image-tool in Windows
            SaveCustomerSegmentationPlotChart(predictions, plotLocation);
            OpenChartInDefaultWindow(plotLocation);

        }

        private static void SaveCustomerSegmentationCSV(IEnumerable<ClusteringPrediction> predictions, string csvlocation)
        {
            ConsoleWriteHeader("CSV Customer Segmentation");
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
            ConsoleWriteHeader("Plot Customer Segmentation");

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
