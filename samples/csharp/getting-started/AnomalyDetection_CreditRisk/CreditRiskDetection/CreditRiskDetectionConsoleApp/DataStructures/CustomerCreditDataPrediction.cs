using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnomalyDetection.DataStructures
{
    public class CustomerCreditDataPrediction
    {

        public bool PredictedLabel { get; set; }
        public float Score { get; set; }
    }
}
