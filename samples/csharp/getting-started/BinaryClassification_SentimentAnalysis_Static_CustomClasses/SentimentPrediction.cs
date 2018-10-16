using Microsoft.ML.Runtime.Api;

namespace BinaryClassification_SentimentAnalysis
{
    public class SentimentPrediction
    {
        [ColumnName("prediction.predictedLabel")] 
        public bool PredictionLabel { get; set; }

        [ColumnName("prediction.probability")]
        public float Probability { get; set; }

        [ColumnName("prediction.score")]
        public float Score { get; set; }
    }
}