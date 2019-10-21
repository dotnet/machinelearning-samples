using Microsoft.ML.Data;

namespace BlazorSentimentAnalysis.Server.ML.DataModels
{
    public class SampleObservation
    {
        [ColumnName("col0"), LoadColumn(0)]
        public string Col0 { get; set; }

        [ColumnName("Label"), LoadColumn(1)]
        public bool Label { get; set; }
    }
}
