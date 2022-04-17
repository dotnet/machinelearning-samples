using Microsoft.ML.Data;

namespace WeatherRecognition
{
    public class WeatherRecognitionPrediction
    {
        [ColumnName("model_output")]
        public float[] PredictedLabels { get; set; }
    }
}
