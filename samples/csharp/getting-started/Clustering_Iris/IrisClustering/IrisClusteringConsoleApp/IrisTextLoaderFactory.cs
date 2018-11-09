using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clustering_Iris
{
    public static class IrisTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                                            {
                                                                Separator = "\t",
                                                                HasHeader = true,
                                                                Column = new[]
                                                                            {
                                                                                new TextLoader.Column("Label", DataKind.R4, 0),
                                                                                new TextLoader.Column("SepalLength", DataKind.R4, 1),
                                                                                new TextLoader.Column("SepalWidth", DataKind.R4, 2),
                                                                                new TextLoader.Column("PetalLength", DataKind.R4, 3),
                                                                                new TextLoader.Column("PetalWidth", DataKind.R4, 4),
                                                                            }
                                                            });
            return textLoader;
        }
    }
}

