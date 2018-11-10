using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using System;
using static Microsoft.ML.Transforms.Normalizers.NormalizingEstimator;

namespace Regression_TaxiFarePrediction
{
    public class TaxiFareDataProcessPipelineFactory
    {
        public static IEstimator<ITransformer> CreateDataProcessPipeline(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline
            // In our case, we will one-hot encode as categorical values the VendorId, RateCode and PaymentType
            // Then concatenate that with the numeric columns.

            return mlContext.Transforms.CopyColumns("FareAmount", "Label")
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId", "VendorIdEncoded"))
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode", "RateCodeEncoded"))
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding("PaymentType", "PaymentTypeEncoded"))
                        .Append(mlContext.Transforms.Normalize(inputName: "PassengerCount", mode: NormalizerMode.MeanVariance))
                        .Append(mlContext.Transforms.Normalize(inputName: "TripTime", mode: NormalizerMode.MeanVariance))
                        .Append(mlContext.Transforms.Normalize(inputName: "TripDistance", mode: NormalizerMode.MeanVariance))
                        .Append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PaymentTypeEncoded", "PassengerCount", "TripTime", "TripDistance"));                       
        }
    }
}


