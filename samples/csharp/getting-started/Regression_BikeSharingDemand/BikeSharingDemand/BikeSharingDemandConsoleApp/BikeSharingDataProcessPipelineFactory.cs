using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace BikeSharingDemand
{
    public class BikeSharingDataProcessPipelineFactory
    {
        public static IEstimator<ITransformer> CreateDataProcessPipeline(MLContext mlContext)
        {
          // Copy the Count column to the Label column.
          return mlContext.Transforms.CopyColumns("Count", "Label")
                    // Concatenate all the numeric columns into a single features column
                    .Append(mlContext.Transforms.Concatenate("Features", "Season", "Year", "Month",
                                                                        "Hour", "Holiday", "Weekday",
                                                                        "Weather", "Temperature", "NormalizedTemperature",
                                                                        "Humidity", "Windspeed"));
        }
    }
}
