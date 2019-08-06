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
        private uint cameraWidth;
        private uint cameraHeight;
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
            cameraWidth = (uint)CameraPreview.ActualWidth;
            cameraHeight = (uint)CameraPreview.ActualHeight;
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            GetCameraSize();
        }

        private void CameraFrameArrived(object sender, Microsoft.Toolkit.Uwp.Helpers.FrameEventArgs e)
        {
            if (e?.VideoFrame?.SoftwareBitmap == null)
            {
                return;
            }

            SoftwareBitmap bitmap = SoftwareBitmap.Convert(e.VideoFrame.SoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            VideoFrame inputFrame = VideoFrame.CreateWithSoftwareBitmap(bitmap);

            //TODO: Use onnx model to do object recognition on the frame

            frameCount++;
            Debug.WriteLine($"Frame received: {frameCount}");
        }
    }
}
