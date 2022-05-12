using Microsoft.ML.Data;

namespace StopSignDetection_ONNX
{
    public class StopSignPrediction
    {
        [ColumnName("detected_classes")]
        public long[] PredictedLabels { get; set; }

        [ColumnName("detected_boxes")]
        public float[] BoundingBoxes { get; set; }

        [ColumnName("detected_scores")]
        public float[] Scores { get; set; }
    }
}
