namespace eShopForecast
{
    /// <summary>
    /// This is the output of the scored regression model, the prediction.
    /// </summary>
    public class ProductUnitRegressionPrediction
    {
        // Below columns are produced by the model's predictor.
        public float Score;
    }

    /// <summary>
    /// This is the output of the scored time series model, the prediction.
    /// </summary>
    public class ProductUnitTimeSeriesPrediction
    {
        public float[] ForecastedProductUnits { get; set; }

        public float[] ConfidenceLowerBound { get; set; }

        public float[] ConfidenceUpperBound { get; set; }
    }
}
