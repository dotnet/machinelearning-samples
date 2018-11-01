using ImageClassification.ModelScorer;
using Microsoft.ML.Runtime.Api;

namespace ImageClassification.ImageData
{
    public class ImageNetPrediction
    {
        [ColumnName(TFModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;
    }
}
