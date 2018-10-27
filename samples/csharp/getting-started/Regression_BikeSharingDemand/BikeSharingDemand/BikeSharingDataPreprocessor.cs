using BikeSharingDemand.Helpers;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace BikeSharingDemand
{
    public class BikeSharingDataPreprocessor
    {
        public IEstimator<ITransformer> DataPreprocessPipeline { get; private set; }

        private static string[] _featureColumns = new[] {
            "Season", "Year", "Month",
            "Hour", "Holiday", "Weekday",
            "Weather", "Temperature", "NormalizedTemperature",
            "Humidity", "Windspeed" };

        public BikeSharingDataPreprocessor(MLContext mlContext)
        {
            // Configure data transformations in the Preprocess pipeline
            DataPreprocessPipeline =
                // Copy the Count column to the Label column
                new CopyColumnsEstimator(mlContext, "Count", "Label")
                    // Concatenate all the numeric columns into a single features column
                    .Append(new ConcatEstimator(mlContext, "Features", _featureColumns));
        }
    }
}
