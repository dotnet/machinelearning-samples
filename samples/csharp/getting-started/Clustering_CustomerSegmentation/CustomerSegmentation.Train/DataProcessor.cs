using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.PCA;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerSegmentation
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        public DataProcessor(MLContext mlContext, int rank = 2)
        {
            // Configure data transformations in the DataProcess pipeline
            DataProcessPipeline = new PrincipalComponentAnalysisEstimator(mlContext, "Features", "PCAFeatures", rank: rank)
                                        .Append(new OneHotEncodingEstimator(mlContext, new[] { new OneHotEncodingEstimator.ColumnInfo("LastName",
                                                                                                                                      "LastNameKey",
                                                                                                                                      CategoricalTransform.OutputKind.Ind) }));
        }
    }
}
