using Microsoft.ML.Runtime.Api;

namespace BinaryClasification_TitanicSurvivalPrediction
{
    public class TitanicPrediction
    {
        [Column(ordinal: "0", name: "PredictedLabel")]
        public bool Survived;

        [Column(ordinal: "1", name: "Probability")]
        public float Probability;
    }
}