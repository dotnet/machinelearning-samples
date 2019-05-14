using Microsoft.ML.Data;

namespace OnnxObjectDetectionE2EAPP
{
    public class ImageNetPrediction
    {
        [ColumnName(OnnxModelScorers.OnnxModelScorer.TinyYoloModelSettings.ModelOutput)]
        public float[] PredictedLabels;
    }
}
