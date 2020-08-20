
namespace ImageClassification.WebApp.ML.DataModels
{
    public class ImagePredictedLabelWithProbability
    {
        public string ImageId { get; set; }

        public string PredictedLabel { get; set; }
        public float Probability { get; set; }

        public long PredictionExecutionTime { get; set; }
    }
}
