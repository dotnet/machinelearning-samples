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
    public class ModelEvaluator
    {
        private readonly string pivotDataLocation;
        private readonly string modelLocation;
        private readonly string plotLocation;
        private readonly LocalEnvironment env;

        public ModelEvaluator(string pivotDataLocation, string modelLocation, string plotLocation)
        {
            this.pivotDataLocation = pivotDataLocation;
            this.modelLocation = modelLocation;
            this.plotLocation = plotLocation;
            env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
        }

        public void Evaluate()
        {
            ITransformer model;
            using (var file = File.OpenRead(modelLocation))
            {
                model = TransformerChain
                    .LoadFrom(env, file);
            }
            
            var reader = new TextLoader(env,
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
                            .AsEnumerable<ClusteringPrediction>(env, false)
                            .ToArray();

            SaveCustomerSegmentationPlot(predictions, plotLocation);

            OpenChartInDefaultWindow(plotLocation);


        }

        private static void SaveCustomerSegmentationPlot(IEnumerable<ClusteringPrediction> predictions, string plotLocation)
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
