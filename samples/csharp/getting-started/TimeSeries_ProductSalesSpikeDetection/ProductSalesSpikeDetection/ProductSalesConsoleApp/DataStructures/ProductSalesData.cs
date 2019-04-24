using Microsoft.ML.Data;

namespace ProductSalesSpikeDetection.DataStructures
{
    public class ProductSalesData
    {
        [LoadColumn(0)]
        public string Month;

        [LoadColumn(1)]
        public float numSales;
    }
}
