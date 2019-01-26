using ImageClassification.ModelScorer;
using Microsoft.ML.Data;

namespace ImageClassification.ImageDataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName(TFModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;
    }
}
