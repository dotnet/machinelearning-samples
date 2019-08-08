using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace OnnxObjectDetection
{
    public class ImageInputData
    {
        [ImageType(416, 416)]
        public Bitmap Image { get; set; }
    }
}
