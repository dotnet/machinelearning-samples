
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System.IO;

namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the input to the trained model.
    /// </summary>
    public class ProductData
    {
        // next,productId,year,month,units,avg,count,max,min,prev
        public ProductData(string productId, int year, int month, float units, float avg, 
            int count, float max, float min, float prev)
        {
            this.productId = productId;
            this.year = year;
            this.month = month;
            this.units = units;
            this.avg = avg;
            this.count = count;
            this.max = max;
            this.min = min;
            this.prev = prev;
        }

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

}
