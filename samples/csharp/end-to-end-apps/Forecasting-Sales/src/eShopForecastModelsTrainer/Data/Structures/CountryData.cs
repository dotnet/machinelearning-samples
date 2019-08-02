
using Microsoft.ML.Data;

namespace eShopForecastModelsTrainer
{
    public class CountryData
    {
        // next,country,year,month,max,min,std,count,sales,med,prev
        [LoadColumn(0)]
        public float next;

        [LoadColumn(1)]
        public string country;

        [LoadColumn(2)]
        public float year;

        [LoadColumn(3)]
        public float month;

        [LoadColumn(4)]
        public float max;

        [LoadColumn(5)]
        public float min;

        [LoadColumn(6)]
        public float std;

        [LoadColumn(7)]
        public float count;

        [LoadColumn(8)]
        public float sales;

        [LoadColumn(9)]
        public float med;

        [LoadColumn(10)]
        public float prev;

        public override string ToString()
        {
            return $"CountryData [next: {next}, country: {country}, year: {year}, month: {month}, max: {max}, min: {min}, std: {std}, count: {count}, sales: {sales}, med: {med}, prev: {prev}]";
        }
    }

    public class CountrySalesPrediction
    {
        public float Score;
    }
}
