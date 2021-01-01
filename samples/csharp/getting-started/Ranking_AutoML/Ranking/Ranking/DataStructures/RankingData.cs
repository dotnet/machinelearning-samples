using Microsoft.ML.Data;

namespace Ranking.DataStructures
{
    public class RankingData
    {
        [LoadColumn(0)]
        public float Label { get; set; }

        [LoadColumn(1)]
        public int GroupId { get; set; }

        [LoadColumn(2, 133)]
        [VectorType(133)]
        public float[] Features { get; set; }
    }
}
