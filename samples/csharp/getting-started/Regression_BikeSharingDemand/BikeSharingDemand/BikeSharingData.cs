using Microsoft.ML.Runtime.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BikeSharingDemand
{
    public class BikeSharingData
    {
        public class Prediction
        {
            [ColumnName("Score")]
            public float PredictedCount;
        }

        public class Demand
        {
            public float Season;
            public float Year;
            public float Month;
            public float Hour;
            public float Holiday;
            public float Weekday;
            public float WorkingDay;
            public float Weather;
            public float Temperature;
            public float NormalizedTemperature;
            public float Humidity;
            public float Windspeed;
            public float Count;   // This is the observed count, to be used a "label" to predict
        }

        public static List<Demand> ReadCsv(string dataLocation)
        {
            // Since bike demand data fits in memory, we can load it all in memory by
            // using ToList() at the end. This makes the processing more efficient.
            // For larger dataset, the data can be read as IEnumerable instead.
            return File.ReadLines(dataLocation)
                .Skip(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(','))
                .Select(x => new Demand()
                {
                    Season = float.Parse(x[2]),
                    Year = float.Parse(x[3]),
                    Month = float.Parse(x[4]),
                    Hour = float.Parse(x[5]),
                    Holiday = float.Parse(x[6]),
                    Weekday = float.Parse(x[7]),
                    WorkingDay = float.Parse(x[8]),
                    Weather = float.Parse(x[9]),
                    Temperature = float.Parse(x[10]),
                    NormalizedTemperature = float.Parse(x[11]),
                    Humidity = float.Parse(x[12]),
                    Windspeed = float.Parse(x[13]),
                    Count = float.Parse(x[16])
                }).ToList();
        }

        public static Demand SingleDemandData =>
            // Single data
            // instant,dteday,season,yr,mnth,hr,holiday,weekday,workingday,weathersit,temp,atemp,hum,windspeed,casual,registered,cnt
            // 13950,2012-08-09,3,1,8,10,0,4,1,1,0.8,0.7576,0.55,0.2239,72,133,205
            new Demand()
            {
                Season = 3,
                Year = 1,
                Month = 8,
                Hour = 10,
                Holiday = 0,
                Weekday = 4,
                WorkingDay = 1,
                Weather = 1,
                Temperature = 0.8f,
                NormalizedTemperature = 0.7576f,
                Humidity = 0.55f,
                Windspeed = 0.2239f
            };
    }
}
