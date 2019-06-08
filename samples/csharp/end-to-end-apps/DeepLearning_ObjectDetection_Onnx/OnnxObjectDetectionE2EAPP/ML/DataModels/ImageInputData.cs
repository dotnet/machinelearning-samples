using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace OnnxObjectDetectionE2EAPP
{
    public class ImageInputData
    {
        [ImageType(416, 416)]
        public Bitmap Image { get; set; }
    }
}
