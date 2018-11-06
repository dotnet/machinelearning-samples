using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clustering_Iris.DataStructures
{
    // IrisPrediction is the result returned from prediction operations
    public class IrisPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId;

        [ColumnName("Score")]
        public float[] Distance;
    }
}
