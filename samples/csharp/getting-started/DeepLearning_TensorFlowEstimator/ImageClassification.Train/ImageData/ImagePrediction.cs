using System;

namespace ImageClassification.DataModels
{
    public class ImagePrediction
    {
        public float[] Score;

        public string PredictedLabelValue;
    }

    public class ImageWithLabelPrediction : ImagePrediction
    {
        public ImageWithLabelPrediction(ImagePrediction pred, string label)
        {
            Label = label;
            Score = pred.Score;
            PredictedLabelValue = pred.PredictedLabelValue;
        }

        public string Label;
    }

}
