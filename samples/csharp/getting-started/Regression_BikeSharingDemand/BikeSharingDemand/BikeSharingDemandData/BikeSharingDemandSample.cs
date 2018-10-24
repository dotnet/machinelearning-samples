using System;
using System.Collections.Generic;
using Microsoft.ML.Runtime.Api;

#pragma warning disable 649 // We don't care about unsused fields here, because they are mapped with the input file.

namespace BikeSharingDemand.BikeSharingDemandData
{
    public class BikeSharingDemandSample
    {
        [Column(ordinal: "2")] public float Season;
        [Column(ordinal: "3")] public float Year;
        [Column(ordinal: "4")] public float Month;
        [Column(ordinal: "5")] public float Hour;
        [Column(ordinal: "6")] public float Holiday;
        [Column(ordinal: "7")] public float Weekday;
        [Column(ordinal: "8")] public float WorkingDay;
        [Column(ordinal: "9")] public float Weather;
        [Column(ordinal: "10")] public float Temperature;
        [Column(ordinal: "11")] public float NormalizedTemperature;
        [Column(ordinal: "12")] public float Humidity;
        [Column(ordinal: "13")] public float Windspeed;
        [Column(ordinal: "16")] public float Count;   // This is the observed count, to be used a "label" to predict
    }

    
}
