using Microsoft.ML.Data;

namespace ClusteringNewsArticles.Perdict.DataStructures
{
    public class ClusteringPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId;

        [ColumnName("Score")]
        public float[] Distance;

        [ColumnName("Features")]
        public float[] Location;

        [ColumnName("news_articles")]
        public string NewsArticles;

        [ColumnName("category")]
        public string Category;
    }
}
