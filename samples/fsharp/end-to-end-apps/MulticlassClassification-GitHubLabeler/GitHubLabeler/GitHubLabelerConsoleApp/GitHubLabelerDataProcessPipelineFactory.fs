module GitHubLabelerDataProcessPipelineFactory

open Microsoft.ML

let createDataProcessPipeline (mlContext : MLContext) =

    mlContext.Transforms.Categorical.MapValueToKey("Area", "Label")
    |> Common.ModelBuilder.append (mlContext.Transforms.Text.FeaturizeText("Title", "TitleFeaturized"))
    |> Common.ModelBuilder.append (mlContext.Transforms.Text.FeaturizeText("Description", "DescriptionFeaturized"))
    |> Common.ModelBuilder.append (mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"))
    |> Common.ModelBuilder.downcastPipeline