using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiFareRegression.DataStructures
{
    public class TaxiTripFarePrediction
    {
        [ColumnName("Score")]
        public float FareAmount;
    }

    public class TaxiTripFarePredictionWithContribution : TaxiTripFarePrediction
    {
        public float[] FeatureContributions { get; set; }

        public List<FeatureContribution> GetFeatureContributions(DataViewSchema dataview)
        {
            //base.PrintToConsole();
            VBuffer<ReadOnlyMemory<char>> slots = default;
            dataview.GetColumnOrNull("Features").Value.GetSlotNames(ref slots);
            var featureNames = slots.DenseValues().ToArray();
            var featureList = new List<FeatureContribution>();
            for (int i = 0; i < featureNames.Count(); i++)
            {
                string featureName = featureNames[i].ToString();
                if (featureName == "PassengerCount" || featureName == "TripTime"|| featureName == "TripDistance")
                featureList.Add(new FeatureContribution(featureName, FeatureContributions[i]));
            }

            return featureList;

        }
    }

    public class TaxiFarePrediction : TaxiTripFarePredictionWithContribution
    {
        public List<FeatureContribution> Features { get; set; }

        public TaxiFarePrediction(float PredictedFareAmount, List<FeatureContribution> Features)
        {
            this.FareAmount = PredictedFareAmount;
            this.Features = Features;
        }
    }

    public class FeatureContribution
    {
        public string Name;
        public float Value;

        public FeatureContribution(string Name, float Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}