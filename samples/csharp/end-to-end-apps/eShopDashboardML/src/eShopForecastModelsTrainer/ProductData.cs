using Microsoft.ML.Runtime.Api;

namespace eShopForecastModelsTrainer
{
    public class ProductData
    {
        // next,productId,year,month,units,avg,count,max,min,prev
        [Column(ordinal: "0", name: "Label")]
        public float next;

        [Column(ordinal: "1")]
        public string productId;

        [Column(ordinal: "2")]
        public float year;
        [Column(ordinal: "3")]
        public float month;
        [Column(ordinal: "4")]
        public float units;
        [Column(ordinal: "5")]
        public float avg;
        [Column(ordinal: "6")]
        public float count;
        [Column(ordinal: "7")]
        public float max;
        [Column(ordinal: "8")]
        public float min;
        [Column(ordinal: "9")]
        public float prev;
    }

    public class ProductUnitPrediction
    {
        public float Score;
    }
}
