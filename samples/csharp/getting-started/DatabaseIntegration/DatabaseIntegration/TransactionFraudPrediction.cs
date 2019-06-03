using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseIntegration
{
    public class TransactionFraudPrediction 
    {
        public bool Label;
        public bool PredictedLabel;
        public float Score;
        public float Probability;

        public void PrintToConsole()
        {
            Console.WriteLine($"Predicted Label: {PredictedLabel}");
            Console.WriteLine($"Probability: {Probability}  ({Score})");
        }
    }
}
