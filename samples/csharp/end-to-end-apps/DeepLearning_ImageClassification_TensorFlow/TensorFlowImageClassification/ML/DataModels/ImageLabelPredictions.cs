

using Microsoft.ML.Data;

namespace TensorFlowImageClassification.ML.DataModels
{
    public class ImageLabelPredictions
    {
        //TODO: Change to fixed output column name for TensorFlow model
        [ColumnName("loss")]
        public float[] PredictedLabels;
    }
}
