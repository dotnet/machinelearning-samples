using Microsoft.ML.Data;

namespace ObjectDetection
{
    #region ImageNetPrediction
    public class ImageNetPrediction
    {
        [ColumnName(OnnxModelScorer.TinyYoloModelSettings.ModelOutput)]
        public float[] PredictedLabels;
    }
    #endregion
}
