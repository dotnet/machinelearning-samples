using Microsoft.ML.Runtime.Api;

namespace eShopForecastModelsTrainer
{
    public class ProductData
    {
        // next,productId,year,month,units,avg,count,max,min,prev
        public float next;

        public string productId;

        public float year;
        public float month;
        public float units;
        public float avg;
        public float count;
        public float max;
        public float min;
        public float prev;
    }

    public class ProductUnitPrediction
    {
        public float Score;
    }
}
