using System.Linq;
using System.Text;

namespace OnnxObjectDetectionWeb.Infrastructure
{
    public static class ImageValidationExtensions
    {
        public static bool IsValidImage(this byte[] image)
        {
            var imageFormat = GetImageFormat(image);
            return imageFormat == ImageFormat.jpeg ||
                   imageFormat == ImageFormat.png;
        }

        public enum ImageFormat
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            unknown
        }

        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");        // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");       // GIF
            var png = new byte[] { 137, 80, 78, 71 };       // PNG
            var tiff = new byte[] { 73, 73, 42 };           // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };          // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 };   // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 };  // jpeg canon
            var jpg1 = new byte[] { 255, 216, 255, 219 };
            var jpg2 = new byte[] { 255, 216, 255, 226 };

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)) || jpg1.SequenceEqual(bytes.Take(jpg1.Length)) || jpg2.SequenceEqual(bytes.Take(jpg2.Length)))
                return ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;

            return ImageFormat.unknown;
        }
    }
}
