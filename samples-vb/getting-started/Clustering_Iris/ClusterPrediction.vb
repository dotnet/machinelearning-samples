Imports Microsoft.ML.Runtime.Api

Public Class ClusterPrediction
    <ColumnName("PredictedLabel")>
    Public SelectedClusterId As UInteger
    <ColumnName("Score")>
    Public Distance As Single()
End Class
