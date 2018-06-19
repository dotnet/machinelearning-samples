using System.Collections.Generic;

namespace BinaryClasification_TitanicSurvivalPrediction
{
    internal class TestTitanicData
    {
        internal static readonly TitanicData Passenger = new TitanicData()
            {
                Pclass = 3f,
                Name = "Braund, Mr. Owen Harris",
                Sex = "male",
                Age = 31,
                SibSp = 0,
                Parch = 0,
                Ticket = "335097",
                Fare = "7.75",
                Cabin = "",
                Embarked = "Q"
            };
    }
}