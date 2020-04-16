using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using System.Text.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LandUseUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SatelliteMap_Loaded(object sender, RoutedEventArgs e)
        {
            BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 47.604, Longitude = -122.329 };
            Geopoint cityCenter = new Geopoint(cityPosition);

            await (sender as MapControl).TrySetViewAsync(cityCenter);
        }

        private async void QueryLocation_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get coordinates for address
            var coordinates = await GetCoordinatesAsync(AddressBar.Text);

            // 2. Update map with new coordinates
            await UpdateMapLocationAsync(SatelliteMap, coordinates);

            // 3. Take snapshot of map control
            var satelliteImage = await GetMapAsImageAsync();

            // 4. Call ASP.NET Core Web API to classify image
            PredictionText.Text = "Inspecting Image";
            var prediction = await ClassifyImageAsync(satelliteImage);

            // 5. Display prediction
            PredictionText.Text = $"Prediction: {prediction}";
        }

        private async Task<Coordinates> GetCoordinatesAsync(string address)
        {
            Coordinates result;

            using (HttpClient client = new HttpClient())
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
                if (coordinates == null)
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

        private async Task UpdateMapLocationAsync(MapControl map, Coordinates coordinates)
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
            await renderBitmap.RenderAsync(SatelliteMap);
            IBuffer pixelBuffer = await renderBitmap.GetPixelsAsync();

            var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, renderBitmap.PixelWidth, renderBitmap.PixelHeight, BitmapAlphaMode.Ignore);

            byte[] array;
            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
                array = new byte[stream.Size];
                await stream.ReadAsync(array.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
            }

            return array;
        }

        private async Task<string> ClassifyImageAsync(byte[] imageBytes)
        {
            string prediction;
            string base64image = Convert.ToBase64String(imageBytes);

            // Create request body
            string content = JsonSerializer.Serialize(
                new Dictionary<string, string>
                {
                    { "data", base64image }
                });

            // Send image to ASP.NET Core Web API for classification
            using (var client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (msg, cert, chain, ssl) => true }))
            {
                var res = await client.PostAsync("https://localhost:5001/api/classification", new StringContent(content, Encoding.UTF8, "application/json"));
                prediction = await res.Content.ReadAsStringAsync();
            }

            return prediction;
        }
    }
}
