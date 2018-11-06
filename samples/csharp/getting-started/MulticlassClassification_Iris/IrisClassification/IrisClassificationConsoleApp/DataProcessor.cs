using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using System;
using static Microsoft.ML.Transforms.Normalizers.NormalizingEstimator;

namespace MulticlassClassification_Iris
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        public DataProcessor(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline

            DataProcessPipeline = mlContext.Transforms.Concatenate("Features", new[] { "SepalLength",
                                                                                       "SepalWidth",
                                                                                       "PetalLength",
                                                                                       "PetalWidth" });
        }
    }
}


