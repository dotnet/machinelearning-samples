using Microsoft.ML.Runtime.Api;

namespace eShopForecastModelsTrainer
{
    public class CountryData
    {
        // next,country,year,month,max,min,std,count,sales,med,prev
        [Column(ordinal: "0", name: "Label")]
        public float next;

        [Column(ordinal: "1")]
        public string country;

        [Column(ordinal: "2")]
        public float year;
        [Column(ordinal: "3")]
        public float month;
        [Column(ordinal: "4")]
        public float max;
        [Column(ordinal: "5")]
        public float min;
        [Column(ordinal: "6")]
        public float std;
        [Column(ordinal: "7")]
        public float count;
        [Column(ordinal: "8")]
        public float sales;
        [Column(ordinal: "9")]
        public float med;
        [Column(ordinal: "10")]
        public float prev;
    }

    public class CountrySalesPrediction
    {
        public float Score;
    }
}
