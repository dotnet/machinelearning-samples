using Microsoft.ML.Data;

namespace ShampooSalesSpikeDetection
{
    class ShampooSalesPrediction
    {
        // Vector to hold Alert, Score, and P-Value values
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}
