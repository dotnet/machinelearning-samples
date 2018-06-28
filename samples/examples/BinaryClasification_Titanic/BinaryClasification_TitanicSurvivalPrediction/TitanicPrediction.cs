using Microsoft.ML.Runtime.Api;

namespace BinaryClasification_TitanicSurvivalPrediction
{
    public class TitanicPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Survived;
    }
}