using Microsoft.ML.Data;

namespace CustomVisionObjectDetectionOnnx.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("model_outputs0")]
        public float[] PredictedLabels;
    }
}
