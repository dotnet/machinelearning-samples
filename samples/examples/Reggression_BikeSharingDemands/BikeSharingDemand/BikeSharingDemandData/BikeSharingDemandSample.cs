using Microsoft.ML.Runtime.Api;

namespace BikeSharingDemand.BikeSharingDemandData
{
    public class BikeSharingDemandSample
    {
        [Column("2")] public float Season;
        [Column("3")] public float Year;
        [Column("4")] public float Month;
        [Column("5")] public float Hour;
        [Column("6")] public bool Holiday;
        [Column("7")] public float Weekday;
        [Column("8")] public float Weather;
        [Column("9")] public float Temperature;
        [Column("10")] public float NormalizedTemperature;
        [Column("11")] public float Humidity;
        [Column("12")] public float Windspeed;
        [Column("15")] public float Count;
    }
}
