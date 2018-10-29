using Microsoft.ML.Runtime.Api;
using System;

namespace ImageClassification.ImageData
{
    public class ImageNetPrediction
    {
        public float[] Score;

        public string PredictedLabel;
    }

    public class ImageNetWithLabelPrediction : ImageNetPrediction
    {
        public ImageNetWithLabelPrediction(ImageNetPrediction pred, string label)
        {
            Label = label;
            Score = pred.Score;
            PredictedLabel = pred.PredictedLabel;
        }

        public string Label;
    }

}
