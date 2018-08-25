Friend Class TestSentimentData
    Friend Shared ReadOnly Sentiments As IEnumerable(Of SentimentData) = {
        New SentimentData With {
            .SentimentText = "Contoso's 11 is a wonderful experience",
            .Sentiment = 0
        }, New SentimentData With {
            .SentimentText = "The acting in this movie is very bad",
            .Sentiment = 0
        }, New SentimentData With {
            .SentimentText = "Joe versus the Volcano Coffee Company is a great film.",
            .Sentiment = 0
        }
    }
End Class
