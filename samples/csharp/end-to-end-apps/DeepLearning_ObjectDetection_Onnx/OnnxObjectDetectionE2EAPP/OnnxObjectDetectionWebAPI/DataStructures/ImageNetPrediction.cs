using Microsoft.ML.Data;

namespace OnnxObjectDetectionWebAPI
{
    public class ImageNetPrediction
    {
        [ColumnName(OnnxModelScorers.OnnxModelScorer.TinyYoloModelSettings.ModelOutput)]
        public float[] PredictedLabels;
    }
}
