using Microsoft.ML;
using Microsoft.ML.Data;

namespace MulticlassClassification_HeartDisease
{
    public static class HeartTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.CreateTextReader(
                columns:new[]
                {
                    new TextLoader.Column("Age", DataKind.R4, 0),
                    new TextLoader.Column("Sex", DataKind.R4, 1),
                    new TextLoader.Column("Cp", DataKind.R4, 2),
                    new TextLoader.Column("TrestBps", DataKind.R4, 3),
                    new TextLoader.Column("Chol", DataKind.R4, 4),
                    new TextLoader.Column("Fbs", DataKind.R4, 5),
                    new TextLoader.Column("RestEcg", DataKind.R4, 6),
                    new TextLoader.Column("Thalac", DataKind.R4, 7),
                    new TextLoader.Column("Exang", DataKind.R4, 8),
                    new TextLoader.Column("OldPeak", DataKind.R4, 9),
                    new TextLoader.Column("Slope", DataKind.R4, 10),
                    new TextLoader.Column("Ca", DataKind.R4, 11),
                    new TextLoader.Column("Thal", DataKind.R4, 12),
                    new TextLoader.Column("Label", DataKind.R4, 13)
                },
                hasHeader:false,
                separatorChar: ',');

            return textLoader;
        }
    }
}
