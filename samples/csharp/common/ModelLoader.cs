using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;

namespace Common
{
    public static class ModelLoader
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
