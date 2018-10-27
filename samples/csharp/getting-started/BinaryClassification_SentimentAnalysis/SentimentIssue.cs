using Microsoft.ML.Runtime.Api;

namespace BinaryClassification_SentimentAnalysis
{
    public class SentimentIssue
    {
        public bool Label { get; set; }
        public string Text { get; set; }
    }
}
