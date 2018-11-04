using Microsoft.ML.Runtime.Api;

namespace SentimentAnalysisConsoleApp.DataStructures
{
    public class SentimentIssue
    {
        public bool Label { get; set; }
        public string Text { get; set; }
    }
}
