Imports System.Collections.Generic

Friend Class TestTitanicData
    Friend Shared ReadOnly Passenger As TitanicData = New TitanicData() With {
        .Pclass = 2,
        .Name = "Shelley, Mrs. William (Imanita Parrish Hall)",
        .Sex = "female",
        .Age = 25,
        .SibSp = 0,
        .Parch = 1,
        .Ticket = "230433",
        .Fare = "26",
        .Cabin = "",
        .Embarked = "S"
    }
End Class
