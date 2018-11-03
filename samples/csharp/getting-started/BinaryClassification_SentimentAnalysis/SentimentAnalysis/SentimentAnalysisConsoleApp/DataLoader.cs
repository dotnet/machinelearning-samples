using Microsoft.ML;
using Microsoft.ML.Runtime.Data;


namespace SentimentAnalysisConsoleApp
{
    class DataLoader
    {
        MLContext _mlContext;
        private TextLoader _loader;

        public DataLoader(MLContext mlContext)
        {
            _mlContext = mlContext;

            _loader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                                {
                                                    Separator = "tab",
                                                    HasHeader = true,
                                                    Column = new[]
                                                        {
                                                            new TextLoader.Column("Label", DataKind.Bool, 0),
                                                            new TextLoader.Column("Text", DataKind.Text, 1)
                                                        }
                                                });
        }

        public IDataView GetDataView(string filePath)
        {
            return _loader.Read(filePath);
        }
    }
}
