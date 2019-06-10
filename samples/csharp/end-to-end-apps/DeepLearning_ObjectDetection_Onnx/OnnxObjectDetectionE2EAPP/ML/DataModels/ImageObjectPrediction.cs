using Microsoft.ML.Data;

namespace OnnxObjectDetectionE2EAPP
{
    public class ImageObjectPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
