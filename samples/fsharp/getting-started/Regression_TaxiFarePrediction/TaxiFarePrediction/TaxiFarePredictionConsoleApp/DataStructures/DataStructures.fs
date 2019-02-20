namespace TaxiFarePrediction.DataStructures

module DataStructures =
    open Microsoft.ML.Data

    [<CLIMutable>]
    type TaxiTrip = {
        [<LoadColumn(0)>]
        VendorId : string
        [<LoadColumn(1)>]
        RateCode : string
        [<LoadColumn(2)>]
        PassengerCount : float32
        [<LoadColumn(3)>]
        TripTime : float32
        [<LoadColumn(4)>]
        TripDistance : float32
        [<LoadColumn(5)>]
        PaymentType : string
        [<LoadColumn(6)>]
        FareAmount : float32
    }

    [<CLIMutable>]
    type TaxiTripFarePrediction = {
        [<ColumnName("Score")>]
        FareAmount : float32
    }
