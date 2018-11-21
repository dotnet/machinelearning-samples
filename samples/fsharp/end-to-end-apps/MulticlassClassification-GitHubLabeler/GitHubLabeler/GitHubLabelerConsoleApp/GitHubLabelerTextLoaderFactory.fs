module GitHubLabelerTextLoaderFactory

open System
open Microsoft.ML
open Microsoft.ML.Runtime.Data


let createTextLoader (mlContext : MLContext) =
    mlContext.Data.TextReader(
        TextLoader.Arguments(
            Separator = "tab",
            HasHeader = true,
            Column = 
                [|

                    TextLoader.Column("ID", Nullable DataKind.Text, 0)
                    TextLoader.Column("Area", Nullable DataKind.Text, 1)
                    TextLoader.Column("Title", Nullable DataKind.Text, 2)
                    TextLoader.Column("Description", Nullable DataKind.Text, 3)
                |]
        )
    )

let read datasetPath (textLoader : TextLoader) =
    textLoader.Read([| datasetPath |])