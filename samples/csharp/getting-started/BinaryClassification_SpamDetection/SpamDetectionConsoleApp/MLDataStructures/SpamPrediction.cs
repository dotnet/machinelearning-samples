using Microsoft.ML.Data;

namespace SpamDetectionConsoleApp.MLDataStructures
{
    class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public string isSpam { get; set; }
    }
}
