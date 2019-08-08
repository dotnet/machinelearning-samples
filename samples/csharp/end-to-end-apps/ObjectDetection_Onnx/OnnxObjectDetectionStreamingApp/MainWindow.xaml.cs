using Microsoft.ML;
using OnnxObjectDetection;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace OnnxObjectDetectionStreamingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private Mat frame;
        private VideoCapture capture;

        private ITransformer model;
        private readonly MLContext mlContext = new MLContext();
        private readonly YoloOutputParser yoloParser = new YoloOutputParser();
        private PredictionEngine<ImageInputData, ImageObjectPrediction> predictionEngine;
       

        private int frameCount = 0;
        private bool isCameraRunning = false;

        private readonly int skipFrames = 0; // if performance is really bad, we may want to send every nth frame to the model

        private Thread camera;

        public MainWindow()
        {
            InitializeComponent();

            LoadModel();
            CaptureCamera();
        }
 
        private void LoadModel()
        {
            var onnxModel = "TinyYolo2_model.onnx";
            var mlNetModelFile = "TinyYoloModel.zip";

            var assetsUri = Path.Combine(Environment.CurrentDirectory, @"Assets");

            var onnxPath = Path.Combine(assetsUri, onnxModel);
            var modelPath = Path.Combine(assetsUri, mlNetModelFile);

            OnnxModelConfigurator onnxModelConfigurator = new OnnxModelConfigurator(onnxPath);
            onnxModelConfigurator.SaveMLNetModel(modelPath);

            model = mlContext.Model.Load(modelPath, out _);

            predictionEngine = mlContext.Model.CreatePredictionEngine<ImageInputData, ImageObjectPrediction>(model);
        }

        private void CaptureCamera()
        {
            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
            isCameraRunning = true;
        }

        private void CaptureCameraCallback()
        {
            frame = new Mat();
            capture = new VideoCapture(0);
            capture.Open(0);

            if (capture.IsOpened())
            {
                while (isCameraRunning)
                {
                    capture.Read(frame);

                    if (CheckSkipFrame())
                    {
                        // TODO: Kill after user closes the app window or it throws an exception here
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            using MemoryStream memoryStream = frame.ToMemoryStream();

                            var imageSource = new BitmapImage();

                            imageSource.BeginInit();
                            imageSource.CacheOption = BitmapCacheOption.OnLoad;
                            imageSource.StreamSource = memoryStream;
                            imageSource.EndInit();

                            WebCamImage.Source = imageSource;

                            var bitmapImage = new Bitmap(memoryStream);

                            ParseWebCamFrame(bitmapImage);
                        });
                    }
                }
            }
        }

        async void ParseWebCamFrame(Bitmap bitmap)
        {
            if (model == null) //TODO: Need to do better than this to make sure that the model has been created first
                return;

            var originalHeight = bitmap.Height;
            var originalWidth = bitmap.Width;

            var frame = new ImageInputData { Image = bitmap };

            var filteredBoxes = DetectObjectsUsingModel(frame);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DrawOverlays(filteredBoxes, originalHeight, originalWidth);
            }, System.Windows.Threading.DispatcherPriority.Render);
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

                var boxColor = ConvertColor(box.BoxColor);

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

        private bool CheckSkipFrame()
        {
            if (skipFrames == 0)
                return true;

            if (++frameCount == skipFrames)
            {
                frameCount = 0;
                return true;
            }
            return false;
        }

        private System.Windows.Media.Color ConvertColor(System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
    }
}
