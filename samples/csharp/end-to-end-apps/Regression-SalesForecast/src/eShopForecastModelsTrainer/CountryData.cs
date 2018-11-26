using Microsoft.ML.Runtime.Api;

namespace eShopForecastModelsTrainer
{
    public class CountryData
    {
        // next,country,year,month,max,min,std,count,sales,med,prev
        public float next;

        public string country;

        public float year;
        public float month;
        public float max;
        public float min;
        public float std;
        public float count;
        public float sales;
        public float med;
        public float prev;
    }

    public class CountrySalesPrediction
    {
        public float Score;
    }
}
