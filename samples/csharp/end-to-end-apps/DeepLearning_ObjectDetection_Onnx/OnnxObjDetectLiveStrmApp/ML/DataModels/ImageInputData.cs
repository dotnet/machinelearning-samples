using Microsoft.ML.Transforms.Image;
using System.Drawing;
using Windows.Graphics.Imaging;
using Windows.Media;

namespace OnnxObjectDetectionLiveStreamApp
{
    public class ImageInputData
    {
        [ImageType(416, 416)]
        public Bitmap Image { get; set; }
    }
}
