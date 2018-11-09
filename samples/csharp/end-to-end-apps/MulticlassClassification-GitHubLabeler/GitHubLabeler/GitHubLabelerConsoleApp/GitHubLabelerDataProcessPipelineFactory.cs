using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Text;


namespace GitHubLabeler
{
    public class GitHubLabelerDataProcessPipelineFactory
    {
        public static IEstimator<ITransformer> CreateDataProcessPipeline(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline

            return mlContext.Transforms.Categorical.MapValueToKey("Area", "Label")
                                  .Append(mlContext.Transforms.Text.FeaturizeText("Title", "TitleFeaturized"))
                                  .Append(mlContext.Transforms.Text.FeaturizeText("Description", "DescriptionFeaturized"))
                                  .Append(mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"));
        }
    }
}

