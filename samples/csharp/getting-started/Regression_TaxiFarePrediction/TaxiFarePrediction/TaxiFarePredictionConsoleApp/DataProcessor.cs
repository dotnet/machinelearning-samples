using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using System;
using static Microsoft.ML.Transforms.Normalizers.NormalizingEstimator;

namespace Regression_TaxiFarePrediction
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        public DataProcessor(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline
            // In our case, we will one-hot encode as categorical values the VendorId, RateCode and PaymentType
            // Then concatenate that with the numeric columns.

            DataProcessPipeline = new CopyColumnsEstimator(mlContext, "FareAmount", "Label")
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId", "VendorIdEncoded"))
                        //.Append(new CategoricalEstimator(mlcontext, "VendorId", "VendorIdEncoded"))
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode", "RateCodeEncoded"))
                        //.Append(new CategoricalEstimator(mlcontext, "RateCode", "RateCodeEncoded"))
                        .Append(mlContext.Transforms.Categorical.OneHotEncoding("PaymentType", "PaymentTypeEncoded"))
                        //.Append(new CategoricalEstimator(mlcontext, "PaymentType", "PaymentTypeEncoded"))
                        .Append(mlContext.Transforms.Normalize(inputName: "PassengerCount", mode:NormalizerMode.MeanVariance))
                        .Append(mlContext.Transforms.Normalize(inputName: "TripTime", mode:NormalizerMode.MeanVariance))
                        .Append(mlContext.Transforms.Normalize(inputName: "TripDistance", mode:NormalizerMode.MeanVariance))
                        .Append(new ColumnConcatenatingEstimator(mlContext, "Features", "VendorIdEncoded", "RateCodeEncoded", "PaymentTypeEncoded", "PassengerCount", "TripTime", "TripDistance"));
         
        }
    }
}


