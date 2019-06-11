#region ImagePredictionUsings
using Microsoft.ML.Data;
#endregion

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
