using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubLabeler.DataStructures
{
    public class FullPrediction
    {
        public string PredictedLabel;
        public float Score;
        public int OriginalSchemaIndex;

        public FullPrediction(string predictedLabel, float score, int originalSchemaIndex)
        {
            PredictedLabel = predictedLabel;
            Score = score;
            OriginalSchemaIndex = originalSchemaIndex;
        }
    }
}
