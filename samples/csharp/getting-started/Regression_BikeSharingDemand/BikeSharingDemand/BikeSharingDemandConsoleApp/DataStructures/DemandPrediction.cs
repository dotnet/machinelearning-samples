using Microsoft.ML.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BikeSharingDemand.DataStructures
{
    public class DemandPrediction
    {
        [ColumnName("Score")]
        public float PredictedCount;
    }
}
