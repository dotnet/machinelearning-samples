Imports Microsoft.ML.Runtime.Api

Public Class TaxiTrip
    <Column("0")>
    Public VendorId As String
    <Column("1")>
    Public RateCode As String
    <Column("2")>
    Public PassengerCount As Single
    <Column("3")>
    Public TripTime As Single
    <Column("4")>
    Public TripDistance As Single
    <Column("5")>
    Public PaymentType As String
    <Column("6")>
    Public FareAmount As Single
End Class
