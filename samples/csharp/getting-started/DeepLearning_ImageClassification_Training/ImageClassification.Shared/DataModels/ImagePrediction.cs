using Microsoft.ML.Data;

namespace ImageClassification.DataModels
{
    public class ImagePrediction
    {
        [ColumnName("Score")]
        public float[] Score;

        [ColumnName("PredictedLabel")]
        public string PredictedLabel;
    }
}
