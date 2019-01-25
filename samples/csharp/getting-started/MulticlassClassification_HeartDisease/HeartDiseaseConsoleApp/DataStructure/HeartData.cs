using Microsoft.ML.Data;

namespace MulticlassClassification_HeartDisease.DataStructure
{
    public class HeartData
    {
        public float Age;
        public float Sex;
        public float Cp;
        public float TrestBps;
        public float Chol;
        public float Fbs;
        public float RestEcg;
        public float Thalac;
        public float Exang;
        public float OldPeak;
        public float Slope;
        public float Ca;
        public float Thal;
    }

    public class HeartDataImport
    {
        [LoadColumn(0)]
        public float Age { get; set; }
        [LoadColumn(1)]
        public float Sex { get; set; }
        [LoadColumn(2)]
        public float Cp { get; set; }
        [LoadColumn(3)]
        public float TrestBps { get; set; }
        [LoadColumn(4)]
        public float Chol { get; set; }
        [LoadColumn(5)]
        public float Fbs { get; set; }
        [LoadColumn(6)]
        public float RestEcg { get; set; }
        [LoadColumn(7)]
        public float Thalac { get; set; }
        [LoadColumn(8)]
        public float Exang { get; set; }
        [LoadColumn(9)]
        public float OldPeak { get; set; }
        [LoadColumn(10)]
        public float Slope { get; set; }
        [LoadColumn(11)]
        public float Ca { get; set; }
        [LoadColumn(12)]
        public float Thal { get; set; }
        [LoadColumn(13)]
        public float Label { get; set; }


        //new TextLoader.Column("Age", DataKind.R4, 0),
        //new TextLoader.Column("Sex", DataKind.R4, 1),
        //new TextLoader.Column("Cp", DataKind.R4, 2),
        //new TextLoader.Column("TrestBps", DataKind.R4, 3),
        //new TextLoader.Column("Chol", DataKind.R4, 4),
        //new TextLoader.Column("Fbs", DataKind.R4, 5),
        //new TextLoader.Column("RestEcg", DataKind.R4, 6),
        //new TextLoader.Column("Thalac", DataKind.R4, 7),
        //new TextLoader.Column("Exang", DataKind.R4, 8),
        //new TextLoader.Column("OldPeak", DataKind.R4, 9),
        //new TextLoader.Column("Slope", DataKind.R4, 10),
        //new TextLoader.Column("Ca", DataKind.R4, 11),
        //new TextLoader.Column("Thal", DataKind.R4, 12),
        //new TextLoader.Column("Label", DataKind.R4, 13)
    }
}
