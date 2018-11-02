using Microsoft.ML.Runtime.Api;
using System;

namespace ImageClassification.ImageData
{
    public class ImageNetPrediction
    {
        public float[] Score;

        public string PredictedLabelValue;
    }

    public class ImageNetWithLabelPrediction : ImageNetPrediction
    {
        public ImageNetWithLabelPrediction(ImageNetPrediction pred, string label)
        {
            Label = label;
            Score = pred.Score;
            PredictedLabelValue = pred.PredictedLabelValue;
        }

        public string Label;
    }

}
