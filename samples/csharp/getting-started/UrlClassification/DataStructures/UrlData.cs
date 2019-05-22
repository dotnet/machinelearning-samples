using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrlClassification.DataStructures
{
    public class UrlData
    {
        [LoadColumn(0)]
        public string LabelColumn;

        //[LoadColumn(1), ColumnName("Features")]
        [LoadColumn(1, 3231961)]
        [VectorType(3231961)]
        public float[] FeatureVector;
    }

    public class TransformedData : UrlData
    {
        public bool LabelKey { get; set; }
    }
}
