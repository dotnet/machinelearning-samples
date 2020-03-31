using LandUseUWPML.Model;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Capture;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LandUseUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PredictionEngine<ModelInput, ModelOutput> _predictionEngine;
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MLContext mlContext = new MLContext();
            mlContext.ComponentCatalog.RegisterAssembly(typeof(NormalizeMapping).Assembly);
            mlContext.ComponentCatalog.RegisterAssembly(typeof(LabelMapping).Assembly);
            var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/MLModel.zip"));
            ITransformer model = mlContext.Model.Load(modelFile.Path, out var inputSchema);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        }

        private async void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 47.604, Longitude = -122.329 };
            Geopoint cityCenter = new Geopoint(cityPosition);

            await (sender as MapControl).TrySetViewAsync(cityCenter);
        }

        private async void QueryLocation_Click(object sender, RoutedEventArgs e)
        {
            // 1. Reverse geocode 
            var coordinates = await GetCoordinatesAsync(AddressBar.Text);

            // 2. Update map with new address location
            await UpdateMapLocation(MapControl, coordinates);

            // 3. Convert map display into an image
            var satelliteImage = await GetMapAsImageAsync();

            // 4. Make a prediction
            PredictionText.Text = "Inspecting Image";
            var prediction = await ClassifyImageAsync(satelliteImage);

            // 5. Display prediction
            PredictionText.Text = $"Prediction: {prediction}";

            // 6. Clean up image
            //await satelliteImage.DeleteAsync();
        }

        private async Task<Coordinates> GetCoordinatesAsync(string address)
        {
            Coordinates result;

            using(HttpClient client = new HttpClient())
            {
                //Generate URL
                string urlEncodedAddress = HttpUtility.UrlEncode(address);
                var uri = new Uri($"https://nominatim.openstreetmap.org/search?q={urlEncodedAddress}&format=json");

                // Build request
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("User-Agent", "LandUseUWP/1.0");

                // Get coordinates
                var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                //Parse results
                var coordinates = JsonSerializer.Deserialize<IEnumerable<Coordinates>>(body).FirstOrDefault();

                //Return results
                if(coordinates == null)
                {
                    result = new Coordinates { Latitude = "47.604", Longitude = "-122.329" };
                    await new MessageDialog("Could not find address provided.", "Address Not Found").ShowAsync();
                } 
                else
                {
                    result = coordinates;
                }
            }

            return result;
        }

        private async Task UpdateMapLocation(MapControl map, Coordinates coordinates)
        {
            BasicGeoposition newPosition = new BasicGeoposition() 
            { 
                Latitude = float.Parse(coordinates.Latitude), 
                Longitude = float.Parse(coordinates.Longitude) 
            };

            await map.TrySetViewAsync(new Geopoint(newPosition));
        }

        private async Task<byte[]> GetMapAsImageAsync()
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            await renderBitmap.RenderAsync(MapControl);
            IBuffer pixelBuffer = await renderBitmap.GetPixelsAsync();

            return pixelBuffer.ToArray();
            //var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, renderBitmap.PixelWidth, renderBitmap.PixelHeight, BitmapAlphaMode.Ignore);

            //StorageFolder installLocation = ApplicationData.Current.TemporaryFolder;
            //StorageFile file = await installLocation.CreateFileAsync("mapimage.jpeg", CreationCollisionOption.ReplaceExisting);

            //using(var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            //{

            //    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

            //    encoder.SetSoftwareBitmap(softwareBitmap);

            //    await encoder.FlushAsync();

            //}

            //return file;
        }

        private async Task<string> ClassifyImageAsync(byte[] imageBytes)
        {
            string prediction;

            using (var client = new HttpClient())
            {
                var content = Convert.ToBase64String(imageBytes);
                var res = await client.PostAsync("http://localhost:54029/api/Predict", new StringContent(content));
                prediction = await res.Content.ReadAsStringAsync();
                //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                //var req = await client.PostAsync("", );
                //prediction = await req.Content.ReadAsStringAsync();
            }

            return prediction;
            //var prediction = await Task.Run(() => _predictionEngine.Predict(new ModelInput { ImageSource = @"C:\Users\luquinta.REDMOND\Datasets\EuroSAT200\AnnualCrop\AnnualCrop_1.jpg" }));
            //return prediction.Prediction;
        }
    }
}
