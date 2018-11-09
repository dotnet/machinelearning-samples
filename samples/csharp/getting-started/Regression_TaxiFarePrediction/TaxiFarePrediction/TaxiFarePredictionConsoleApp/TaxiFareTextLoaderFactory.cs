using Microsoft.ML;
using Microsoft.ML.Runtime.Data;

namespace Regression_TaxiFarePrediction
{
    public static class TaxiFareTextLoaderFactory
    {
        public static TextLoader CreateTextLoader(MLContext mlContext)
        {
            TextLoader textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                            {
                                                Separator = ",",
                                                HasHeader = true,
                                                Column = new[]
                                                            {
                                                            new TextLoader.Column("VendorId", DataKind.Text, 0),
                                                            new TextLoader.Column("RateCode", DataKind.Text, 1),
                                                            new TextLoader.Column("PassengerCount", DataKind.R4, 2),
                                                            new TextLoader.Column("TripTime", DataKind.R4, 3),
                                                            new TextLoader.Column("TripDistance", DataKind.R4, 4),
                                                            new TextLoader.Column("PaymentType", DataKind.Text, 5),
                                                            new TextLoader.Column("FareAmount", DataKind.R4, 6)
                                                            }
                                            });
            return textLoader;
        }
    }
}

