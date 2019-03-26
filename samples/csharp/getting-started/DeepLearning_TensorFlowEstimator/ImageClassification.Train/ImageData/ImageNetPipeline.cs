using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassification.ImageData
{
    public class ImageNetPipeline
    {
        public string ImagePath;
        public string Label;
        public string PredictedLabelValue;
        public float[] Score;
        public float[] softmax2_pre_activation;
    }
}
