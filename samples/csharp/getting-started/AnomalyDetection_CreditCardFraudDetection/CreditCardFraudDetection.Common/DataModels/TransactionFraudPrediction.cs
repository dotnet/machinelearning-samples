using System;

namespace CreditCardFraudDetection.Common.DataModels
{
    public class TransactionFraudPrediction : IModelEntity
    {
        public float Label;
        public float Score;
        public float Probability;
        public bool PredictedLabel;

        public void PrintToConsole()
        {
            // Console.WriteLine($"Predicted Label: {Score > 0.2}");
            Console.WriteLine($"Predicted Label: {PredictedLabel}");
            Console.WriteLine($"Probability: {Probability}  (Score: {Score})");
        }
    }
}
