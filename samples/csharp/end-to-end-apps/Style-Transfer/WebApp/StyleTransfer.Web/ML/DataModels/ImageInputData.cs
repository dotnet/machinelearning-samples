using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Transforms.Image;

namespace StyleTransfer.Web.ML.DataModels
{
    public class ImageInputData
    {
        [ImageType(ImageConstants.ImageHeight, ImageConstants.ImageWidth)]
        public Bitmap Image { get; set; }
    }
}
