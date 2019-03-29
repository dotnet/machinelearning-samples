
using Microsoft.ML.Data;

namespace TryOnnx
{
    public class ImageNetPrediction
    {
        [ColumnName(OnnxModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;
    }
}
