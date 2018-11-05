using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubLabeler
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        public DataProcessor(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline

            DataProcessPipeline = new ValueToKeyMappingEstimator(mlContext, "Area", "Label")
                                  .Append(mlContext.Transforms.Text.FeaturizeText("Title", "TitleFeaturized"))
                                  .Append(mlContext.Transforms.Text.FeaturizeText("Description", "DescriptionFeaturized"))
                                  .Append(mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"));
        }
    }
}