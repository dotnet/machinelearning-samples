using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace TensorFlowImageClassification.ML.DataModels
{
    public class ImageInputData
    {
        [ImageType(227, 227)]
        public Bitmap Image { get; set; }
    }
}
