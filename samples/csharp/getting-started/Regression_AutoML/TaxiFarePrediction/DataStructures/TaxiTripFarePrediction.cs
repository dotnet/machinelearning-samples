using Microsoft.ML.Data;

namespace TaxiFarePrediction.DataStructures
{
    public class TaxiTripFarePrediction
    {
        [ColumnName("Score")]
        public float FareAmount;
    }
}