Friend Class TestTaxiTrips
    Friend Shared ReadOnly Trip1 As New TaxiTrip With {
        .VendorId = "VTS",
        .RateCode = "1",
        .PassengerCount = 1,
        .TripDistance = 10.33F,
        .PaymentType = "CSH",
        .FareAmount = 0
    }
End Class
