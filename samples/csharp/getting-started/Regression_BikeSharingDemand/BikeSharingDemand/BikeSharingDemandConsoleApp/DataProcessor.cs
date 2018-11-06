using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace BikeSharingDemand
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        private static string[] _featureColumns = new[] {
            "Season", "Year", "Month",
            "Hour", "Holiday", "Weekday",
            "Weather", "Temperature", "NormalizedTemperature",
            "Humidity", "Windspeed" };

        public DataProcessor(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline
            DataProcessPipeline =
                // Copy the Count column to the Label column
                new CopyColumnsEstimator(mlContext, "Count", "Label")
                    // Concatenate all the numeric columns into a single features column
                    .Append(mlContext.Transforms.Concatenate("Features", _featureColumns));
                    //Another way: .Append(new ColumnConcatenatingEstimator(mlContext, "Features", _featureColumns));
        }
    }
}
