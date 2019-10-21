namespace SentimentAnalysis.DataStructures

module Model =
    open Microsoft.ML.Data

    /// Type representing the text to run sentiment analysis on.
    [<CLIMutable>] 
    type SentimentIssue = 
        { 
            [<LoadColumn(0)>]
            Label : bool

            [<LoadColumn(2)>]
            Text : string 
        }

    /// Result of sentiment prediction.
    [<CLIMutable>]
    type  SentimentPrediction = 
        { 
            // ColumnName attribute is used to change the column name from
            // its default value, which is the name of the field.
            [<ColumnName("PredictedLabel")>]
            Prediction : bool; 

            // No need to specify ColumnName attribute, because the field
            // name "Probability" is the column name we want.
            Probability : float32; 

            Score : float32 
        }
