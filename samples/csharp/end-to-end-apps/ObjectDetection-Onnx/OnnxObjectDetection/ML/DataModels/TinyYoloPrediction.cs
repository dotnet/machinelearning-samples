using Microsoft.ML.Data;

namespace OnnxObjectDetection
{
    public class TinyYoloPrediction : IOnnxObjectPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels { get; set; }
    }
}
