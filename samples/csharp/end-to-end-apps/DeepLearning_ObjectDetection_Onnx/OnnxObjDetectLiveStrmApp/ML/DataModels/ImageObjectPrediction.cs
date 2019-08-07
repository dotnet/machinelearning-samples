using Microsoft.ML.Data;

namespace OnnxObjectDetectionLiveStreamApp
{
    public class ImageObjectPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
