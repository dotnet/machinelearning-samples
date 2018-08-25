Imports Microsoft.ML.Runtime.Api

Public Class TitanicData
    <Column("0")>
    Public PassengerId As Single
    <Column("1", "Label")>
    Public Survived As Single
    <Column("2")>
    Public Pclass As Single
    <Column("3")>
    Public Name As String
    <Column("4")>
    Public Sex As String
    <Column("5")>
    Public Age As Single
    <Column("6")>
    Public SibSp As Single
    <Column("7")>
    Public Parch As Single
    <Column("8")>
    Public Ticket As String
    <Column("9")>
    Public Fare As String
    <Column("10")>
    Public Cabin As String
    <Column("11")>
    Public Embarked As String
End Class
