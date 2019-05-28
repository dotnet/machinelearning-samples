using System;
using System.Collections.Generic;
using Microsoft.ML.Data;
//using static Microsoft.ML.Runtime.Data.RoleMappedSchema;

namespace CreditCardFraudDetection.Common.DataModels
{
    public class TransactionFraudPrediction : IModelEntity
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
