using Microsoft.ML.Data;

namespace OnnxObjectDetection
{
    public class ImageObjectPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
