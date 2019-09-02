using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassification.DataModels
{
    public class ImagePredictionEx
    {
        public string ImagePath;
        public string Label;
        public UInt32 PredictedLabel;
        public float[] Score;
    }
}
