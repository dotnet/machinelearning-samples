using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OnnxObjectDetectionLiveStreamApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private uint fullImageWidth;
        private uint fullImageHeight;
        private int frameCount;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            GetCameraSize();
            Window.Current.SizeChanged += Current_SizeChanged;

            await CameraPreview.StartAsync();
            CameraPreview.CameraHelper.FrameArrived += CameraFrameArrived;
        }

        private void GetCameraSize()
        {
            fullImageWidth = (uint)CameraPreview.ActualWidth;
            fullImageHeight = (uint)CameraPreview.ActualHeight;
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            GetCameraSize();
        }

        private async void CameraFrameArrived(object sender, Microsoft.Toolkit.Uwp.Helpers.FrameEventArgs e)
        {
            if (e?.VideoFrame?.SoftwareBitmap == null)
            {
                return;
            }

            SoftwareBitmap bitmap = SoftwareBitmap.Convert(e.VideoFrame.SoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            VideoFrame inputFrame = VideoFrame.CreateWithSoftwareBitmap(bitmap);
            
            //TODO: Use onnx model to do object recognition on the frameu

            frameCount++;
            Debug.WriteLine($"Frame received: {frameCount}");

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                DrawOverlays(frameCount);
            });
        }

        private void DrawOverlays(int frameCount)
        {
            CameraCanvas.Children.Clear();
            DrawImageBox(frameCount);
        }

        private void DrawImageBox(int frameCount)
        {
            uint x = 1;
            uint y = 1;
            uint w = 1;
            uint h = 1;

            // TODO: Get the x, y, w, h coordinates from the results of the model to know where the object is detected within the image
            x = fullImageWidth * x / 416;
            y = fullImageHeight * y / 416;
            w = fullImageWidth * w / 416;
            h = fullImageHeight * h / 416;

            var objBox = new Windows.UI.Xaml.Shapes.Rectangle
            {
                Width = w,
                Height = h,
                Fill = new SolidColorBrush(Windows.UI.Colors.Transparent),
                Stroke = new SolidColorBrush(Windows.UI.Colors.Green),
                StrokeThickness = 2.0,
                Margin = new Thickness(x, y, 0, 0)
            };

            var objDescription = new TextBlock
            {
                Margin = new Thickness(x + 4, y + 4, 0, 0),
                Text = $"Test Frame {frameCount}",
                FontWeight = FontWeights.Bold,
                Width = 126,
                Height = 21,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var objDescriptionBackground = new Windows.UI.Xaml.Shapes.Rectangle
            {
                Width = 134,
                Height = 29,
                Fill = new SolidColorBrush(Windows.UI.Colors.Green),
                Margin = new Thickness(x, y, 0, 0)
            };

            CameraCanvas.Children.Add(objDescriptionBackground);
            CameraCanvas.Children.Add(objDescription);
            CameraCanvas.Children.Add(objBox);
        }
    }
}
