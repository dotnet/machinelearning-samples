namespace TaxiFarePrediction.DataStructures

module DataStructures =
    open Microsoft.ML.Data

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
