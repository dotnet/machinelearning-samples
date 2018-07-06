using Microsoft.ML.Runtime.Api;

namespace BikeSharingDemand.BikeSharingDemandData
{
    public class BikeSharingDemandPrediction
    {
        [ColumnName("Score")]
        public float PredictedCount;
    }
}
