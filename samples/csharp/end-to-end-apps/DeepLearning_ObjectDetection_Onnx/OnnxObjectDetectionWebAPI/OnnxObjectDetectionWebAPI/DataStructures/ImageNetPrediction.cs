using Microsoft.ML.Data;

namespace TensorFlowImageClassificationWebAPI
{
    public class ImageNetPrediction
    {
        [ColumnName(OnnxModelScorers.OnnxModelScorer.TinyYoloModelSettings.ModelOutput)]
        public float[] PredictedLabels;
    }
}
