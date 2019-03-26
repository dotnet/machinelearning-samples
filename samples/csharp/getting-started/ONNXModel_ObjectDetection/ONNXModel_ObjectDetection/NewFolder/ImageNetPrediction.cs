using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDetection.NewFolder
{
    public class ImageNetPrediction
    {
        [VectorType(1000)]
        public float[] softmaxout_1 { get; set; }
    }
}
