using Microsoft.ML.Runtime.Api;

namespace Clustering_Iris
{
    public class ClusterPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId;

        [ColumnName("Score")]
        public float[] Distance;
    }
}