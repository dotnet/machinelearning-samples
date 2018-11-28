using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.ML.Legacy;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML;
using Microsoft.Extensions.Configuration;

namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the input to the trained model.
    /// </summary>
    public class CountryData
    {
        // next,country,year,month,max,min,std,count,sales,med,prev
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
