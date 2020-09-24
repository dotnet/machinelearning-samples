using System;
using Common;
using OxyPlot;
using System.IO;
using System.Linq;
using Microsoft.ML;
using OxyPlot.Series;
using Microsoft.ML.Data;
using System.Diagnostics;
using System.Collections.Generic;
using ClusteringNewsArticles.Predict.DataStructures;

namespace ClusteringNewsArticles.Predict
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
            _trainedModel = _mlContext.Model.Load(modelPath, out _);

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
            var transformedDataView = _trainedModel.Transform(data);
            var predictions = _mlContext.Data.CreateEnumerable<ClusteringPrediction>(transformedDataView, false).ToArray();

            SaveNewsArticlesClusterCsv(predictions, _csvLocation);
            SaveNewsArticlesClusterPlotChart(predictions, _plotLocation);
            OpenChartInDefaultWindow(_plotLocation);
        }

        private static void SaveNewsArticlesClusterCsv(IEnumerable<ClusteringPrediction> predictions, string csvLocation)
        {
            ConsoleHelper.ConsoleWriteHeader("CSV News Articles Cluster");

            using var w = new StreamWriter(csvLocation);

            w.WriteLine("news_articles,SelectedClusterId");
            w.Flush();

            predictions.ToList().ForEach(delegate (ClusteringPrediction prediction)
            {
                if (w != null)
                {
                    w.WriteLine($"{prediction.NewsArticles},{prediction.SelectedClusterId},{prediction.Category}");
                    w.Flush();
                }
            });

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
            var clusteringPredictions = predictions as ClusteringPrediction[] ?? predictions.ToArray();
            var clusters = clusteringPredictions.Select(p => p.SelectedClusterId).Distinct().OrderBy(x => x);

            foreach (var cluster in clusters)
            {
                var scatter = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerStrokeThickness = 2, Title = $"Cluster: {cluster}", RenderInLegend = true };
                var series = clusteringPredictions
                    .Where(p => p.SelectedClusterId == cluster)
                    .Select(p => new ScatterPoint(p.Location[0], p.Location[1])).ToArray();
                scatter.Points.AddRange(series);
                plot.Series.Add(scatter);
            }

            plot.DefaultColors = OxyPalettes.HueDistinct(plot.Series.Count).Colors;

            var exporter = new SvgExporter { Width = 600, Height = 400 };
            using (var fs = new FileStream(plotLocation, FileMode.Create))
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
