using Microsoft.ML;
using OnnxObjectDetection;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private OnnxOutputParser outputParser;
        private PredictionEngine<ImageInputData, TinyYoloPrediction> tinyYoloPredictionEngine;
        private PredictionEngine<ImageInputData, CustomVisionPrediction> customVisionPredictionEngine;

        private static readonly string modelsDirectory = Path.Combine(Environment.CurrentDirectory, @"ML\OnnxModels");

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
            // Check for an Onnx model exported from Custom Vision
            var customVisionExport = Directory.GetFiles(modelsDirectory, "*.zip").FirstOrDefault();

            // If there is one, use it.
            if (customVisionExport != null)
            {
                var customVisionModel = new CustomVisionModel(customVisionExport);
                var modelConfigurator = new OnnxModelConfigurator(customVisionModel);

                outputParser = new OnnxOutputParser(customVisionModel);
                customVisionPredictionEngine = modelConfigurator.GetMlNetPredictionEngine<CustomVisionPrediction>();
            }
            else // Otherwise default to Tiny Yolo Onnx model
            {
                var tinyYoloModel = new TinyYoloModel(Path.Combine(modelsDirectory, "TinyYolo2_model.onnx"));
                var modelConfigurator = new OnnxModelConfigurator(tinyYoloModel);

                outputParser = new OnnxOutputParser(tinyYoloModel);
                tinyYoloPredictionEngine = modelConfigurator.GetMlNetPredictionEngine<TinyYoloPrediction>();
            }
        }

        private void StartCameraCapture()
        {
            cameraCaptureCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => CaptureCamera(cameraCaptureCancellationTokenSource.Token), cameraCaptureCancellationTokenSource.Token) ;
        }

        private void StopCameraCapture() => cameraCaptureCancellationTokenSource?.Cancel();

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

                    await ParseWebCamFrame(bitmapImage, token);
                }

                capture.Release();
            }
        }

        async Task ParseWebCamFrame(Bitmap bitmap, CancellationToken token)
        {
            if (customVisionPredictionEngine == null && tinyYoloPredictionEngine == null)
                return;

            var frame = new ImageInputData { Image = bitmap };
            var filteredBoxes = DetectObjectsUsingModel(frame);

            if (!token.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DrawOverlays(filteredBoxes, WebCamImage.ActualHeight, WebCamImage.ActualWidth);
                });
            }
        }

        public List<BoundingBox> DetectObjectsUsingModel(ImageInputData imageInputData)
        {
            var labels = customVisionPredictionEngine?.Predict(imageInputData).PredictedLabels ?? tinyYoloPredictionEngine?.Predict(imageInputData).PredictedLabels;
            var boundingBoxes = outputParser.ParseOutputs(labels);
            var filteredBoxes = outputParser.FilterBoundingBoxes(boundingBoxes, 5, 0.5f);
            return filteredBoxes;
        }
        
        private void DrawOverlays(List<BoundingBox> filteredBoxes, double originalHeight, double originalWidth)
        {
            WebCamCanvas.Children.Clear();

            foreach (var box in filteredBoxes)
            {
                // process output boxes
                double x = Math.Max(box.Dimensions.X, 0);
                double y = Math.Max(box.Dimensions.Y, 0);
                double width = Math.Min(originalWidth - x, box.Dimensions.Width);
                double height = Math.Min(originalHeight - y, box.Dimensions.Height);

                // fit to current image size
                x = originalWidth * x / ImageSettings.imageWidth;
                y = originalHeight * y / ImageSettings.imageHeight;
                width = originalWidth * width / ImageSettings.imageWidth;
                height = originalHeight * height / ImageSettings.imageHeight;

                var boxColor = box.BoxColor.ToMediaColor();

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
                    Text = box.Description,
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
