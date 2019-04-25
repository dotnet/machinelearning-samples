using Microsoft.ML.Data;

namespace AdvancedTaxiFarePrediction.DataStructures
{
    public class TaxiTrip
    {
        [ColumnName("vendor_id")]
        public string VendorId;

        [ColumnName("rate_code")]
        public float RateCode;

        [ColumnName("passenger_count")]
        public float PassengerCount;

        [ColumnName("trip_time_in_secs")]
        public float TripTime;

        [ColumnName("trip_distance")]
        public float TripDistance;

        [ColumnName("payment_type")]
        public string PaymentType;

        [ColumnName("fare_amount")]
        public float FareAmount;
    }
}