using Microsoft.ML.Data;

namespace Regression_AutoML.DataStructures
{
    public class TaxiTripFarePrediction
    {
        [ColumnName("Score")]
        public float FareAmount;
    }
}