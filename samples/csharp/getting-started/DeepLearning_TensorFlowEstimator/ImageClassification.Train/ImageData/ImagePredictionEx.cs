using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassification.DataModels
{
    public class ImagePredictionEx
    {
        public string ImagePath;
        public string Label;
        public string PredictedLabelValue;
        public float[] Score;
       
        //[ColumnName("InceptionV3/Predictions/Reshape")]
        //public float[] ImageFeatures;  //In Inception v1: "softmax2_pre_activation"
    }
}
