
namespace ImageClassification.WebApp.ML.DataModels
{
    public class ImagePredictedLabelWithProbability
    {
        public string ImageId;

        public string PredictedLabel;
        public float Probability { get; set; }

        public long PredictionExecutionTime;
    }
}
