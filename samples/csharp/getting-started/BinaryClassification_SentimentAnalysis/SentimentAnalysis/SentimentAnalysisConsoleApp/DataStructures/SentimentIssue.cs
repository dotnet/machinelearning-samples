
using Microsoft.ML.Data;

namespace SentimentAnalysisConsoleApp.DataStructures
{
    public class SentimentIssue
    {
        [LoadColumn(0)]
        public bool Label { get; set; }
        [LoadColumn(2)]
        public string Text { get; set; }
    }
}
