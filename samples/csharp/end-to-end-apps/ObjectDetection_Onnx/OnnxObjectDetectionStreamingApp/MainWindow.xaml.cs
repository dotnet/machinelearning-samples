using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnnxObjectDetectionStreamingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        VideoCapture capture;
        Mat frame;
        private Thread camera;
        bool isCameraRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            CaptureCamera();
        }
 
        private void CaptureCamera()
        {
            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
            isCameraRunning = true;
        }

        private void CaptureCameraCallback()
        {
            // This code is based on the example here: https://ourcodeworld.com/articles/read/761/how-to-take-snapshots-with-the-web-camera-with-c-sharp-using-the-opencvsharp-library-in-winforms

            frame = new Mat();
            capture = new VideoCapture(0);
            capture.Open(0);

            if (capture.IsOpened())
            {
                while (isCameraRunning)
                {
                    capture.Read(frame);

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        using (MemoryStream memoryStream = frame.ToMemoryStream())
                        {
                            var imageSource = new BitmapImage();
                            imageSource.BeginInit();
                            imageSource.StreamSource = memoryStream;
                            imageSource.EndInit();

                            // Attempting to show the image either in a MediaElement or Image control - neither seem to be working currently - need to investigate further
                            WebCamImage.Source = imageSource;
                            WebCamMedia.Source = imageSource.UriSource; //This is always null - not sure how else to get the Uri

                            // This works (change the path to your machine) - so, we are getting frames from the web cam
                            var testImage = BitmapConverter.ToBitmap(frame);
                            testImage.Save(@"C:\Users\nicolela\Documents\TestImages\TestImage.png");
                        }
                    }));
                }
            }
        }
    }
}
