
using Microsoft.ML.Data;

namespace CustomerSegmentation.DataStructures
{
    public class ClusteringPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId;
        [ColumnName("Score")]
        public float[] Distance;
        [ColumnName("PCAFeatures")]
        public float[] Location;
        [ColumnName("LastName")]
        public string LastName;
    }
}
