using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;

using BikeSharingDemand.DataStructures;

namespace Common
{
    public static class ModelScorer
    {
        public static ITransformer LoadModelFromZipFile(MLContext mlContext, string modelPath)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = TransformerChain.LoadFrom(mlContext, stream);
            }

            return loadedModel;
        }
    }
}
