using System.Collections.Generic;

namespace BinaryClassification_SentimentAnalysis
{
    internal class TestSentimentData
    {
        internal static readonly IEnumerable<SentimentIssue> Sentiments = new[]
        {
            new SentimentIssue
            {
                text = "Contoso's 11 is a wonderful experience",
                label = 0
            },
            new SentimentIssue
            {
                text = "The acting in this movie is very bad",
                label = 0
            },
            new SentimentIssue
            {
                text = "Joe versus the Volcano Coffee Company is a great film.",
                label = 0
            }
        };
    }
}