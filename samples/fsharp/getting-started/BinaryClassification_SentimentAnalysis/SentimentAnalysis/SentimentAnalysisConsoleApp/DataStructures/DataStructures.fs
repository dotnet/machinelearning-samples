namespace SentimentAnalysis.DataStructures

module Model =
    open Microsoft.ML.Runtime.Api

    /// Type representing the text to run sentiment analysis on.
    [<CLIMutable>] 
    type SentimentIssue = 
        { 
            Text : string 
        }

    /// Result of sentiment prediction.
    [<CLIMutable>]
    type  SentimentPrediction = 
        { 
            // Predicted sentiment: 0 - negative, 1 - positive
            [<ColumnName("PredictedLabel")>]
            Prediction : bool; 
            Probability : float32; 
            Score : float32 
        }
