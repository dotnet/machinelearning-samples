using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentimentAnalysisConsoleApp
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        public DataProcessor(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline

            DataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Text", "Features");
                                  //Another way: new TextFeaturizingEstimator(mlContext, "Text", "Features");
                                         //You can add additional transformations here with Appends()
                                         //.Append(new YourSelectedEstimator(mlContext, YOUR_REQUIRED_PARAMETERS))                     
        }
    }
}