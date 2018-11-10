using Microsoft.ML;
using Microsoft.ML.Runtime.Data;

namespace SentimentAnalysisConsoleApp
{
    public static class SentimentAnalysysTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                            {
                                                Separator = "tab",
                                                HasHeader = true,
                                                Column = new[]
                                                            {
                                                            new TextLoader.Column("Label", DataKind.Bool, 0),
                                                            new TextLoader.Column("Text", DataKind.Text, 1)
                                                            }
                                            });
            return textLoader;
        }
    }
}
