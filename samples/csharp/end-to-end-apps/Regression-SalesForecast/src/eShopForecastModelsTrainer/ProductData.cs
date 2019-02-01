
using Microsoft.ML.Data;

namespace eShopForecastModelsTrainer
{
    public class ProductData
    {
        // next,productId,year,month,units,avg,count,max,min,prev
        [LoadColumn(0)]
        public float next;

        [LoadColumn(1)]
        public string productId;

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
    }

    public class ProductUnitPrediction
    {
        public float Score;
    }
}
