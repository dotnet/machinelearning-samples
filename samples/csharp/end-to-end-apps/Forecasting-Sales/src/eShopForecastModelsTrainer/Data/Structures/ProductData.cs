
using Microsoft.ML.Data;

namespace eShopForecastModelsTrainer
{
    public class ProductData
    {
        // The index of column in LoadColumn(int index) should be matched with the position of columns in the underlying data file.

        // The next column is used by the Regression algorithm as the Label (e.g. the value that is being predicted by the Regression model).
        [LoadColumn(0)]
        public float next;

        [LoadColumn(1)]
        public float productId;

        [LoadColumn(2)]
        public float year;

        [LoadColumn(3)]
        public float month;

        [LoadColumn(4)]
        public float units;

        [LoadColumn(5)]
        public float avg;

        [LoadColumn(6)]
        public float count;

        [LoadColumn(7)]
        public float max;

        [LoadColumn(8)]
        public float min;

        [LoadColumn(9)]
        public float prev;

        public override string ToString()
        {
            return $"ProductData [productId: {productId}, next: {next}, year: {year}, month: {month}, units: {units}, avg: {avg}, count: {count}, max: {max}, min: {min}, prev: {prev}]";
        }
    }

    public class ProductUnitRegressionPrediction
    {
        public float Score;
    }

    public class ProductUnitTimeSeriesPrediction
    {
        public float[] ForecastedProductUnits { get; set; }

        public float[] ConfidenceLowerBound { get; set; }

        public float[] ConfidenceUpperBound { get; set; }
    }
}
