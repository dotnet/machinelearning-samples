using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClusteringNewsArticles.Perdict.DataStructures;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using OxyPlot;
using OxyPlot.Series;

namespace ClusteringNewsArticles.Perdict
{
    public class ClusteringModelScorer
    {
        private readonly string _newsDataLocation;
        private readonly string _plotLocation;
        private readonly string _csvLocation;
        private readonly MLContext _mlContext;
        private ITransformer _trainedModel;

        public ClusteringModelScorer(MLContext mlContext, string newsDataLocation, string plotLocation, string csvLocation)
        {
            _newsDataLocation = newsDataLocation;
            _plotLocation = plotLocation;
            _csvLocation = csvLocation;
            _mlContext = mlContext;
        }

        public ITransformer LoadModel(string modelPath)
        {
            _trainedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);
            
            return _trainedModel;
        }

        public void CreateNewsArticlesCluster()
        {
            var data = _mlContext.Data.LoadFromTextFile(path: _newsDataLocation, 
                new[]
                {
                    new TextLoader.Column("news_articles", DataKind.String, 0),
                    new TextLoader.Column("category", DataKind.String, 1)
                }, ',', true);
            var tranfomedDataView = _trainedModel.Transform(data);
            var predictions = _mlContext.Data.CreateEnumerable<ClusteringPrediction>(tranfomedDataView, false).ToArray();

            SaveNewsArticlesClusterCsv(predictions, _csvLocation);
            SaveNewsArticlesClusterPlotChart(predictions, _plotLocation);
            OpenChartInDefaultWindow(_plotLocation);
        }

        private static void SaveNewsArticlesClusterCsv(IEnumerable<ClusteringPrediction> predictions, string csvLocation)
        {
            ConsoleHelper.ConsoleWriteHeader("CSV News Articles Cluster");

            using (var w = new StreamWriter(csvLocation))
            {
                w.WriteLine("news_articles,SelectedClusterId");
                w.Flush();
                predictions.ToList().ForEach(delegate (ClusteringPrediction prediction)
                {
                    w.WriteLine($"{prediction.NewsArticles},{prediction.SelectedClusterId},{prediction.Category}");
                    w.Flush();
                });
            }

            Console.WriteLine("CSV location: " + csvLocation);
        }

        private static void SaveNewsArticlesClusterPlotChart(IEnumerable<ClusteringPrediction> predictions, string plotLocation)
        {
            ConsoleHelper.ConsoleWriteHeader("Plot News Articles Clusters");
            var plot = new PlotModel
            {
                Title = "News Articles Clusters",
                IsLegendVisible = true
            };
            var clusters = predictions.Select(p => p.SelectedClusterId).Distinct().OrderBy(x => x);

            foreach (var cluster in clusters)
            {
                var scatter = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerStrokeThickness = 2, Title = $"Cluster: {cluster}", RenderInLegend = true };
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
            
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(plotLocation)
                {
                    UseShellExecute = true
                }
            };

            p.Start();
        }
    }
}
