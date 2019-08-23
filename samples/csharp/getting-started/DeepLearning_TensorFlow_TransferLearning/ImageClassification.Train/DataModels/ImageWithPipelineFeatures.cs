using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassification.DataModels
{
    public class ImageWithPipelineFeatures
    {
        public string ImagePath;
        public string Label;
        public Int64 PredictedLabel;
        //public string PredictedLabelValue;
        public float[] Score;
    }
}
