using Microsoft.ML.Data;

namespace Scalable.Model.DataModels
{
    public class SamplePrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool IsToxic { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }
    }
}



