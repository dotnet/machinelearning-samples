using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace PowerAnomalyDetection.DataStructures
{
    class SpikePrediction
    {
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}
