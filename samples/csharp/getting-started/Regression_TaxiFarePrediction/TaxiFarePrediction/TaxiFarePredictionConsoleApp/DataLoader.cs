using Microsoft.ML;
using Microsoft.ML.Runtime.Data;


namespace Regression_TaxiFarePrediction
{
    class DataLoader
    {
        MLContext _mlContext;
        private TextLoader _loader;

        public DataLoader(MLContext mlContext)
        {
            _mlContext = mlContext;

            _loader = mlContext.Data.TextReader(new TextLoader.Arguments()
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
        }

        public IDataView GetDataView(string filePath)
        {
            return _loader.Read(filePath);
        }
    }
}

