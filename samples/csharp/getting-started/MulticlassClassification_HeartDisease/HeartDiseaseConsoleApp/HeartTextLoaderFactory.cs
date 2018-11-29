using Microsoft.ML;
using Microsoft.ML.Runtime.Data;

namespace MulticlassClassification_HeartDisease
{
    public static class HeartTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.TextReader(new Microsoft.ML.Runtime.Data.TextLoader.Arguments()
            {
                HasHeader = false,
                Separator = ",",
                Column = new[]
                {
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Age", DataKind.R4, 0),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Sex", DataKind.R4, 1),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Cp", DataKind.R4, 2),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("TrestBps", DataKind.R4, 3),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Chol", DataKind.R4, 4),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Fbs", DataKind.R4, 5),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("RestEcg", DataKind.R4, 6),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Thalac", DataKind.R4, 7),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Exang", DataKind.R4, 8),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("OldPeak", DataKind.R4, 9),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Slope", DataKind.R4, 10),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Ca", DataKind.R4, 11),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Thal", DataKind.R4, 12),
                    new Microsoft.ML.Runtime.Data.TextLoader.Column("Label", DataKind.R4, 13)
                }

            });

            return textLoader;
        }
    }
}
