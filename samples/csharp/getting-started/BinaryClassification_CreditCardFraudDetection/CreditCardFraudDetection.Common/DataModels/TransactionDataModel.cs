using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using static Microsoft.ML.Runtime.Data.RoleMappedSchema;

namespace CreditCardFraudDetection.Common.DataModels
{

    public interface IModelEntity {
        void PrintToConsole();
    }

    public class TransactionObservation : IModelEntity
    {
        public bool Label;
        public float V1;
        public float V2;
        public float V3;
        public float V4;
        public float V5;
        public float V6;
        public float V7;
        public float V8;
        public float V9;
        public float V10;
        public float V11;
        public float V12;
        public float V13;
        public float V14;
        public float V15;
        public float V16;
        public float V17;
        public float V18;
        public float V19;
        public float V20;
        public float V21;
        public float V22;
        public float V23;
        public float V24;
        public float V25;
        public float V26;
        public float V27;
        public float V28;
        public float Amount;

        public void PrintToConsole() {
            Console.WriteLine($"Label: {Label}");
            Console.WriteLine($"Features: [V1] {V1} [V2] {V2} [V3] {V3} ... [V28] {V28} Amount: {Amount}");
        }

        public static List<KeyValuePair<ColumnRole, string>>  Roles() {
            return new List<KeyValuePair<ColumnRole, string>>() {
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Label, "Label"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V1"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V2"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V3"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V4"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V5"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V6"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V7"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V8"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V9"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V10"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V11"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V12"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V13"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V14"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V15"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V16"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V17"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V18"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V19"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V20"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V21"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V22"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V23"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V24"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V25"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V26"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V27"),
                    new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V28"),
                    new KeyValuePair<ColumnRole, string>(new ColumnRole("Amount"), ""),

                };
        }
    }

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
