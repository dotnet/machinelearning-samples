using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace StyleTransfer.Web.ML.DataModels
{
    public class TensorInput
    {
        [VectorType(ImageConstants.ImageHeight, ImageConstants.ImageWidth, 3)]
        public float[] Placeholder;
    }
}
