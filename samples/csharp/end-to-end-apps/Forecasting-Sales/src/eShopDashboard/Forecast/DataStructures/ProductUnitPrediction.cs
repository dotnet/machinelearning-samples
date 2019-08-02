namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the output of the scored model, the prediction.
    /// </summary>
    public class ProductUnitRegressionPrediction
    {
        // Below columns are produced by the model's predictor.
        public float Score;
    }

    public class ProductUnitTimeSeriesPrediction
    {
        public float[] ForecastedProductUnits { get; set; }

        public float[] ConfidenceLowerBound { get; set; }

        public float[] ConfidenceUpperBound { get; set; }
    }
}
