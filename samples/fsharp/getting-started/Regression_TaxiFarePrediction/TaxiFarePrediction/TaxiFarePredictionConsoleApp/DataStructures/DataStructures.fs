namespace TaxiFarePrediction.DataStructures

module DataStructures =
    open Microsoft.ML.Runtime.Api

    [<CLIMutable>]
    type TaxiTrip = {
        VendorId : string
        RateCode : string
        PassengerCount : float32
        TripTime : float32
        TripDistance : float32
        PaymentType : string
        FareAmount : float32
    }

    [<CLIMutable>]
    type TaxiTripFarePrediction = {
        [<ColumnName("Score")>]
        FareAmount : float32
    }

    let Trip1 =
        {
            VendorId = "VTS"
            RateCode = "1"
            PassengerCount = 1.0f
            TripTime = 0.0f
            TripDistance = 10.33f
            PaymentType = "CSH"
            FareAmount = 0.0f // predict it. actual = 29.5
        }
