using Microsoft.ML.Data;

namespace SpikeDetection.WinFormsTrainer
{
    public class ProductSalesData
    {
        [LoadColumn(0)]
        public string Month;

        [LoadColumn(1)]
        public float numSales;
    }
}
