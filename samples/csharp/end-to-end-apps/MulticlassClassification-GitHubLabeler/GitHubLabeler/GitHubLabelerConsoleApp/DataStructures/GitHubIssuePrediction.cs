
#pragma warning disable 649 // We don't care about unused fields here, because they are mapped with the input file.

using Microsoft.ML.Data;

namespace GitHubLabeler.DataStructures
{
    internal class GitHubIssuePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Area;

        public float[] Score;
    }
}
