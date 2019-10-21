using Microsoft.ML.Data;

namespace eShopForecast
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
            return $"ProductData [ productId: {productId}, year: {year}, month: {month:00}, next: {next:0000}, units: {units:0000}, avg: {avg:000}, count: {count:00}, max: {max:000}, min: {min}, prev: {prev:0000} ]";
        }
    }
}
