using Microsoft.ML.Data;

namespace SrCnnEntireDetection.DataStructures
{
    public class PhoneCallsPrediction
    {
        //vector to hold anomaly detection results. Including isAnomaly, anomalyScore, magnitude, expectedValue, boundaryUnits, upperBoundary and lowerBoundary.
        [VectorType(7)]
        public double[] Prediction { get; set; }
    }
}
