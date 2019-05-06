using Microsoft.ML.Data;

namespace Scalable.WebAPI.ML.DataModels
{
    public class SampleObservation
    {
        [ColumnName("Label")]
        public bool IsToxic { get; set; }


        [ColumnName("Text")]
        public string SentimentText { get; set; }

    }
}
