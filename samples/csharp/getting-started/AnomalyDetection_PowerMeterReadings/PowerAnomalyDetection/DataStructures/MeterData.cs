using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace PowerAnomalyDetection.DataStructures
{
    class MeterData
    {
        [LoadColumn(0)]
        public string name { get; set; }
        [LoadColumn(1)]
        public DateTime time { get; set; }
        [LoadColumn(2)]
        public float ConsumptionDiffNormalized { get; set; }
    }
}
