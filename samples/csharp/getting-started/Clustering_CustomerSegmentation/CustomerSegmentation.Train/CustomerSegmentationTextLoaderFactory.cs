using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerSegmentation
{
    public static class CustomerSegmentationTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                    {
                                        Separator = ",",
                                        HasHeader = true,
                                        Column = new[]
                                                    {
                                                    new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
                                                    new TextLoader.Column("LastName", DataKind.Text, 32)
                                                    }
                                    }
                                    );

            return textLoader;
        }
    }
}
