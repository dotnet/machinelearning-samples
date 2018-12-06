using Microsoft.ML.Runtime.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BikeSharingDemand.DataStructures
{
    public class DemandObservation
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

    public static class DemandObservationSample
    {
        public static DemandObservation SingleDemandSampleData =>
                                        // Single data
                                        // instant,dteday,season,yr,mnth,hr,holiday,weekday,workingday,weathersit,temp,atemp,hum,windspeed,casual,registered,cnt
                                        // 13950,2012-08-09,3,1,8,10,0,4,1,1,0.8,0.7576,0.55,0.2239,72,133,205
                                        new DemandObservation()
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
