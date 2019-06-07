using System.Drawing;
using Microsoft.ML.Transforms.Image.

namespace OnnxObjectDetectionE2EAPP
{
    public class ImageInputData
    {

        [ImageType(227, 227)]
        public Bitmap Image { get; set; }
    }
}
