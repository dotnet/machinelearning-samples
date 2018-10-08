using Microsoft.ML.Runtime.Api;

namespace BinaryClassification_SentimentAnalysis
{
    public class SentimentPrediction
    {
        [ColumnName("prediction.predictedLabel")] 
        public bool PredictionLabel { get; set; }
    }
}