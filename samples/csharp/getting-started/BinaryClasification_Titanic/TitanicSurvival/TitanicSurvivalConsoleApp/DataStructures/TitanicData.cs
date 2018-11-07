using Microsoft.ML.Runtime.Api;

namespace TitanicSurvivalConsoleApp.DataStructures
{
    public class TitanicData
    {
        [Column("0")]
        public float PassengerId;

        [Column(ordinal: "1")]
        public float Label;   //If "Survived" or not

        [Column("2")]
        public float Pclass;

        [Column("3")]
        public string Name;

        [Column("4")]
        public string Sex;

        [Column("5")]
        public float Age;

        [Column("6")]
        public float SibSp;

        [Column("7")]
        public float Parch;

        [Column("8")]
        public string Ticket;

        [Column("9")]
        public float Fare;

        [Column("10")]
        public string Cabin;

        [Column("11")]
        public string Embarked;
    }
}