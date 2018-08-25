Imports Microsoft.ML.Runtime.Api

Public Class SentimentData
    <Column("0")>
    Public SentimentText As String
    <Column("1", "Label")>
    Public Sentiment As Single
End Class
