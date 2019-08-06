using System;

namespace CreditCardFraudDetection.Common.DataModels
{
    public class TransactionFraudPrediction : IModelEntity
    {
        public float Label;

        /// <summary>
        /// The non-negative, unbounded score that was calculated by the anomaly detection model.
        /// Fraudulent transactions (Anomalies) will have higher scores than normal transactions
        /// </summary>
        public float Score;

        /// <summary>
        /// The predicted label, based on the score. A value of true indicates an anomaly.
        /// </summary>
        public bool PredictedLabel;

        public void PrintToConsole()
        {
            // There is currently an issue where PredictedLabel is always set to true
            // Due to this issue, we'll manually choose the treshold that will indicate an anomaly
            // Issue: https://github.com/dotnet/machinelearning/issues/3990
            //Console.WriteLine($"Predicted Label: {Score > 0.2f}  (Score: {Score})");

            Console.WriteLine($"Predicted Label: {PredictedLabel}  (Score: {Score})");
        }
    }
}
