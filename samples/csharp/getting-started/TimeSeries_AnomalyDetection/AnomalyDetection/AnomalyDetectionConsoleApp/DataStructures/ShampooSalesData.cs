using Microsoft.ML.Data;

namespace SpikeDetection.DataStructures
{
    public class ShampooSalesData
    {
        [LoadColumn(0)]
        public string Month;

        [LoadColumn(1)]
        public float numSales;
    }
}
