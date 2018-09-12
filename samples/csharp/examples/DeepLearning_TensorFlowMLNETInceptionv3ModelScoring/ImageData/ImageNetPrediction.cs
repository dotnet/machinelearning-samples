using Microsoft.ML.Runtime.Api;

namespace TensorFlowMLNETInceptionv3ModelScoring.ImageData
{
    public class ImageNetPrediction
    {
        [ColumnName("Score")]
        public float[] PredictedLabels;
    }
}
