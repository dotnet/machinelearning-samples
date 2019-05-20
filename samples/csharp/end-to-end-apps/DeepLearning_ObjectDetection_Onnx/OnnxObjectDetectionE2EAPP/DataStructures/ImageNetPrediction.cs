using Microsoft.ML.Data;

namespace OnnxObjectDetectionE2EAPP
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
