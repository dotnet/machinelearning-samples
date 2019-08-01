using Microsoft.ML.Data;

namespace LargeDatasets.DataStructures
{
    public class UrlPrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction;
        
        public float Score;
    }
}
