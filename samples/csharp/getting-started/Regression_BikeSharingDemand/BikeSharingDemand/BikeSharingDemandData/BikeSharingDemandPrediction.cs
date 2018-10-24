using Microsoft.ML.Runtime.Api;

#pragma warning disable 649 // We don't care about unsused fields here, because they are mapped with the input file.


namespace BikeSharingDemand.BikeSharingDemandData
{
    public class BikeSharingDemandPrediction
    {
        [ColumnName("Score")]
        public float PredictedCount;
    }
}

