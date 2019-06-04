using Microsoft.ML.Data;

namespace LargeDatasets.DataStructures
{
    public class UrlData
    {
        [LoadColumn(0)]
        public string LabelColumn;
        
        [LoadColumn(1, 3231961)]
        [VectorType(3231961)]
        public float[] FeatureVector;
    }
}
