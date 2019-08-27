
using Microsoft.ML.Data;
using System;

namespace TaxiFareRegression.DataStructures
{
    public interface IModelEntity
    {
        void PrintToConsole();
    }

    public class TaxiTrip: IModelEntity
    {
        [LoadColumn(0)]
        public string VendorId;

        [LoadColumn(1)]
        public string RateCode;

        [LoadColumn(2)]
        public float PassengerCount;

        [LoadColumn(3)]
        public float TripTime;

        [LoadColumn(4)]
        public float TripDistance;

        [LoadColumn(5)]
        public string PaymentType;

        [LoadColumn(6)]
        public float FareAmount;
        public void PrintToConsole()
        {
            Console.WriteLine($"Label: {FareAmount}");
            Console.WriteLine($"Features: [VendorID] {VendorId} [RateCode] {RateCode} [PassengerCount] {PassengerCount} [TripTime] {TripTime} TripDistance: {TripDistance} PaymentType: {PaymentType}");
        }
    }
    

}