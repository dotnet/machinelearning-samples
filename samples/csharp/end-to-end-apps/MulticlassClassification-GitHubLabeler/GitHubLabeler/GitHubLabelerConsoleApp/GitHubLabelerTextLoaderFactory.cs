using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubLabeler
{
    public static class GitHubLabelerTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                                    {
                                                        Separator = "tab",
                                                        HasHeader = true,
                                                        Column = new[]
                                                                    {
                                                                        new TextLoader.Column("ID", DataKind.Text, 0),
                                                                        new TextLoader.Column("Area", DataKind.Text, 1),
                                                                        new TextLoader.Column("Title", DataKind.Text, 2),
                                                                        new TextLoader.Column("Description", DataKind.Text, 3),
                                                                    }
                                                    });

            return textLoader;
        }
    }

}
