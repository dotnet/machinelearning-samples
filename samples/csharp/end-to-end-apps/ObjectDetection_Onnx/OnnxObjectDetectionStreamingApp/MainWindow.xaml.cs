using OpenCvSharp;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OnnxObjectDetectionStreamingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        Mat frame;
        VideoCapture capture;
        bool isCameraRunning = false;

        private Thread camera;

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

                        // This works (change the path to your machine) - so, we are getting frames from the web cam
                        // var testImage = BitmapConverter.ToBitmap(frame);
                        // testImage.Save(@"C:\Users\nicolela\Documents\TestImages\TestImage.png");
                        // testImage.Save(@"C:\Users\colbyw\Documents\TestImages\TestImage.png");
                    });
                }
            }
        }
    }
}
