using ImageClassification.ModelScorer;
using Microsoft.ML.Runtime.Api;

namespace ImageClassification.ImageDataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName(TFModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;
    }
}
