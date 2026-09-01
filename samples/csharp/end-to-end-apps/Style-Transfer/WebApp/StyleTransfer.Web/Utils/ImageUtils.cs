using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace StyleTransfer.Web.Utils
{
    public static class ImageUtils
    {
        public static float[] ExtractPixels(Image<Rgb24> image)
        {
            var pixels = image.GetPixelSpan();
            byte[] rgbaBytes = MemoryMarshal.AsBytes(pixels).ToArray();
            var data = new List<float>(rgbaBytes.Length);
            foreach (var item in rgbaBytes) data.Add(item);
            return data.ToArray();
        }

        public static Image<Rgb24> ResizeImage(string base64Image)
        {
            base64Image = Regex.Replace(base64Image, @"data:image/.+;base64,", "");
            var bytes = Convert.FromBase64String(base64Image);
            var image = SixLabors.ImageSharp.Image.Load<Rgb24>(bytes);
            // Resize
            if (image.Width != ImageConstants.ImageWidth || image.Height != ImageConstants.ImageHeight)
            {
                image.Mutate(x => x.Resize(ImageConstants.ImageWidth, ImageConstants.ImageHeight));
            }

            return image;
        }

        public static string SaveImageForPrediction(string base64Image)
        {
            base64Image = Regex.Replace(base64Image, @"data:image/.+;base64,", "");
            var contentBytes = Convert.FromBase64String(base64Image);
            var filename = $"output-{Guid.NewGuid().ToString()}.jpg";
            var pathInput = $"{ImageConstants.ImagesLocation}/{filename}";
            var ms = new MemoryStream(contentBytes);
            var i = System.Drawing.Image.FromStream(ms);
            i.Save(pathInput, System.Drawing.Imaging.ImageFormat.Jpeg);
            return filename;
        }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}
