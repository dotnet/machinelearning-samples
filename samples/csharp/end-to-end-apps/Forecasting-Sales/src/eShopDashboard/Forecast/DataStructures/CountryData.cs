namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the input to the trained model.
    /// </summary>
    public class CountryData
    {
        public CountryData(string country, int year, int month, float max, float min, float std, int count, float sales, float med, float prev)
        {
            this.country = country;

            this.year = year;
            this.month = month;
            this.max = max;
            this.min = min;
            this.std = std;
            this.count = count;
            this.sales = sales;
            this.med = med;
            this.prev = prev;
        }

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
}
