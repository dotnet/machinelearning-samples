using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrlClassification.DataStructures
{
    public class UrlPrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction;
        
        public float Score;
    }
}
