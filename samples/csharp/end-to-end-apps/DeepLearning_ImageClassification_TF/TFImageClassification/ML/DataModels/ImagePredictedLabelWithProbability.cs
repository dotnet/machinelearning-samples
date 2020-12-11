
namespace TFClassification.ML.DataModels
{
    public class ImagePredictedLabelWithProbability
    {
        public string ImageId;

        public string PredictedLabel { get; set; }
        public float Probability { get; set; }

        public long PredictionExecutionTime;
    }
}
