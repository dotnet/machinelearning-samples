using Microsoft.ML.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BikeSharingDemand.DataStructures
{
    public class DemandObservation
    {
        // Note that we're loading only some columns (certain indexes) starting on column number 2
        // Also, the label column is number 16. 
        // Columns 14, 15 are not being loaded from the file.
        [LoadColumn(2)]
        public float Season { get; set; }
        [LoadColumn(3)]
        public float Year { get; set; }
        [LoadColumn(4)]
        public float Month { get; set; }
        [LoadColumn(5)]
        public float Hour { get; set; }
        [LoadColumn(6)]
        public float Holiday { get; set; }
        [LoadColumn(7)]
        public float Weekday { get; set; }
        [LoadColumn(8)]
        public float WorkingDay { get; set; }
        [LoadColumn(9)]
        public float Weather { get; set; }
        [LoadColumn(10)]
        public float Temperature { get; set; }
        [LoadColumn(11)]
        public float NormalizedTemperature { get; set; }
        [LoadColumn(12)]
        public float Humidity { get; set; }
        [LoadColumn(13)]
        public float Windspeed { get; set; }
        [LoadColumn(16)]
        [ColumnName("Label")]
        public float Count { get; set; }   // This is the observed count, to be used a "label" to predict
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
