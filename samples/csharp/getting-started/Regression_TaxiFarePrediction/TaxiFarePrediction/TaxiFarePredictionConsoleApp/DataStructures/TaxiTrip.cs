using Microsoft.ML.Runtime.Api;

namespace Regression_TaxiFarePrediction.DataStructures
{
    public class TaxiTrip
    {
        public string VendorId;

        public string RateCode;
        
        public float PassengerCount;
        
        public float TripTime;

        public float TripDistance;
        
        public string PaymentType;

        public float FareAmount;
    }
}