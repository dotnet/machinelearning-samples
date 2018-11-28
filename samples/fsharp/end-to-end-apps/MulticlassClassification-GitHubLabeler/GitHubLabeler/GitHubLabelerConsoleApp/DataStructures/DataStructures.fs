module DataStructures

open Microsoft.ML.Runtime.Api

[<CLIMutable>]
type GitHubIssue =
    {
        ID : string
        Area : string   // This is an issue label, for example "area-System.Threading"
        Title : string
        Description : string
    }

[<CLIMutable>]
type GitHubIssuePrediction =
    {
        [<ColumnName("PredictedLabel")>]
        Area : string
    }