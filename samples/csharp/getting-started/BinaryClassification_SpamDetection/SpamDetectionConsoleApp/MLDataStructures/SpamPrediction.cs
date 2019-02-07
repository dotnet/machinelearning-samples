using Microsoft.ML.Data;

namespace SpamDetectionConsoleApp.MLDataStructures
{
    class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool isSpam { get; set; }

        public float Score { get; set; }
        public float Probability { get; set; }
    }
}
