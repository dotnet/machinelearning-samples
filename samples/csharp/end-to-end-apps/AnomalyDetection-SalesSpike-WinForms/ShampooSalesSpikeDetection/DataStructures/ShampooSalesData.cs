using Microsoft.ML.Data;

namespace ShampooSalesSpikeDetection
{
    public class ShampooSalesData
    {
        [LoadColumn(0)]
        public string Month;

        [LoadColumn(1)]
        public float numSales;
    }
}
