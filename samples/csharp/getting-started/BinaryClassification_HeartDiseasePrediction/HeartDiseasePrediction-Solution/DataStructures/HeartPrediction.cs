using Microsoft.ML.Data;

namespace HeartDiseasePredictionConsoleApp.DataStructures
{
    public class HeartPrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction;

        // No need to specify ColumnName attribute, because the field
        // name "Probability" is the column name we want.
        public float Probability;

        public float Score;
    }
}
