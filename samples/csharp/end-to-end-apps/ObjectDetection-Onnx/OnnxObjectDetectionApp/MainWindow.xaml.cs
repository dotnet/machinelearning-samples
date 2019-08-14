using Microsoft.ML;
using OnnxObjectDetection;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace OnnxObjectDetectionApp
{
    public partial class MainWindow : System.Windows.Window
    {
        private VideoCapture capture;
        private CancellationTokenSource cameraCaptureCancellationTokenSource;

        private readonly YoloOutputParser yoloParser = new YoloOutputParser();
        private PredictionEngine<ImageInputData, ImageObjectPrediction> predictionEngine;

        public MainWindow()
        {
            InitializeComponent();
            LoadModel();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            StartCameraCapture();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            StopCameraCapture();
        }

        private void LoadModel()
        {
            var onnxModel = "TinyYolo2_model.onnx";
            var modelDirectory = Path.Combine(Environment.CurrentDirectory, @"ML\OnnxModel");
            var onnxPath = Path.Combine(modelDirectory, onnxModel);

            var onnxModelConfigurator = new OnnxModelConfigurator(onnxPath);
            predictionEngine = onnxModelConfigurator.GetMlNetPredictionEngine();
        }

        private void StartCameraCapture()
        {
            cameraCaptureCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => CaptureCamera(cameraCaptureCancellationTokenSource.Token), cameraCaptureCancellationTokenSource.Token) ;
        }

        private void StopCameraCapture()
        {
            cameraCaptureCancellationTokenSource?.Cancel();
        }

        private async Task CaptureCamera(CancellationToken token)
        {
            if (capture == null)
                capture = new VideoCapture(CaptureDevice.DShow);

            capture.Open(0);

            if (capture.IsOpened())
            {
                while (!token.IsCancellationRequested)
                {
                    using MemoryStream memoryStream = capture.RetrieveMat().Flip(FlipMode.Y).ToMemoryStream();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var imageSource = new BitmapImage();

                        imageSource.BeginInit();
                        imageSource.CacheOption = BitmapCacheOption.OnLoad;
                        imageSource.StreamSource = memoryStream;
                        imageSource.EndInit();

                        WebCamImage.Source = imageSource;
                    });

                    var bitmapImage = new Bitmap(memoryStream);

                    await ParseWebCamFrame(bitmapImage);
                }

                capture.Release();
            }
        }

        async Task ParseWebCamFrame(Bitmap bitmap)
        {
            if (predictionEngine == null)
                return;

            var frame = new ImageInputData { Image = bitmap };
            var filteredBoxes = DetectObjectsUsingModel(frame);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DrawOverlays(filteredBoxes, (int)WebCamImage.ActualHeight, (int)WebCamImage.ActualWidth);
            });
        }

        public IList<YoloBoundingBox> DetectObjectsUsingModel(ImageInputData imageInputData)
        {
            var labels = predictionEngine.Predict(imageInputData).PredictedLabels;
            var boundingBoxes = yoloParser.ParseOutputs(labels);
            var filteredBoxes = yoloParser.FilterBoundingBoxes(boundingBoxes, 5, 0.5f);

            return filteredBoxes;
        }
        
        private void DrawOverlays(IList<YoloBoundingBox> filteredBoxes, int originalHeight, int originalWidth)
        {
            WebCamCanvas.Children.Clear();

            foreach (var box in filteredBoxes)
            {
                // process output boxes
                var x = (uint)Math.Max(box.Dimensions.X, 0);
                var y = (uint)Math.Max(box.Dimensions.Y, 0);
                var width = (uint)Math.Min(originalWidth - x, box.Dimensions.Width);
                var height = (uint)Math.Min(originalHeight - y, box.Dimensions.Height);

                // fit to current image size
                x = (uint)originalWidth * x / OnnxModelConfigurator.ImageSettings.imageWidth;
                y = (uint)originalHeight * y / OnnxModelConfigurator.ImageSettings.imageHeight;
                width = (uint)originalWidth * width / OnnxModelConfigurator.ImageSettings.imageWidth;
                height = (uint)originalHeight * height / OnnxModelConfigurator.ImageSettings.imageHeight;

                var boxColor = box.BoxColor.ToMediaColor();

                var description = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";

                var objBox = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(boxColor),
                    StrokeThickness = 2.0,
                    Margin = new Thickness(x, y, 0, 0)
                };

                var objDescription = new TextBlock
                {
                    Margin = new Thickness(x + 4, y + 4, 0, 0),
                    Text = description,
                    FontWeight = FontWeights.Bold,
                    Width = 126,
                    Height = 21,
                    TextAlignment = TextAlignment.Center
                };

                var objDescriptionBackground = new Rectangle
                {
                    Width = 134,
                    Height = 29,
                    Fill = new SolidColorBrush(boxColor),
                    Margin = new Thickness(x, y, 0, 0)
                };

                WebCamCanvas.Children.Add(objDescriptionBackground);
                WebCamCanvas.Children.Add(objDescription);
                WebCamCanvas.Children.Add(objBox);
            }
        }
    }

    internal static class ColorExtensions
    {
        internal static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
    }
}
