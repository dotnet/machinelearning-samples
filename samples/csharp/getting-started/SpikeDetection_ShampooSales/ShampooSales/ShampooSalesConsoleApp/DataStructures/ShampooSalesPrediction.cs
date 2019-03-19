﻿using Microsoft.ML.Data;

namespace ShampooSales.DataStructures
{
    public class ShampooSalesPrediction
    {
        //vector to hold alert,score,p-value values
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}
