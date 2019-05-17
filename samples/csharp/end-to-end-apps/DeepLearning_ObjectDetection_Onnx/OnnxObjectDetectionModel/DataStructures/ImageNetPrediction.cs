using Microsoft.ML.Data;

namespace OnnxObjectDetectionModel
{
    public class ImageNetPrediction
    {
        [ColumnName(OnnxModelScorer.TinyYoloModelSettings.ModelOutput)]
        public float[] PredictedLabels;
    }
}
