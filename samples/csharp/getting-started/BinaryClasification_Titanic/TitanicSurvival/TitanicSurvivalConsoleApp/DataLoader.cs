using Microsoft.ML;
using Microsoft.ML.Runtime.Data;

using TitanicSurvivalConsoleApp.DataStructures;

namespace TitanicSurvivalConsoleApp
{
    class DataLoader
    {
        MLContext _mlContext;
        private TextLoader _loader;

        public DataLoader(MLContext mlContext)
        {
            _mlContext = mlContext;

            _loader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                                {
                                                    Separator = ",",
                                                    HasHeader = true,
                                                    Column = new[]
                                                        {
                                                            new TextLoader.Column("PassengerId", DataKind.R4, 0),
                                                            new TextLoader.Column("Label", DataKind.R4, 1),
                                                            new TextLoader.Column("Pclass", DataKind.R4, 2),
                                                            new TextLoader.Column("Name", DataKind.Text, 3),
                                                            new TextLoader.Column("Sex", DataKind.Text, 4),
                                                            new TextLoader.Column("Age", DataKind.R4, 5),
                                                            new TextLoader.Column("SibSp", DataKind.R4, 6),
                                                            new TextLoader.Column("Parch", DataKind.R4, 7),
                                                            new TextLoader.Column("Ticket", DataKind.Text, 8),
                                                            new TextLoader.Column("Fare", DataKind.R4, 9),
                                                            new TextLoader.Column("Cabin", DataKind.Text, 10),
                                                            new TextLoader.Column("Embarked", DataKind.Text, 11)
                                                        }
                                                });
        }

        public IDataView GetDataView(string filePath)
        {
            return _loader.Read(filePath);
        }
    }
}

                                                            //new TextLoader.Column(nameof(TitanicData.PassengerId), DataKind.R4, 0),
                                                            //new TextLoader.Column(nameof(TitanicData.Label), DataKind.R4, 1),
                                                            //new TextLoader.Column(nameof(TitanicData.Pclass), DataKind.R4, 2),
                                                            //new TextLoader.Column(nameof(TitanicData.Name), DataKind.Text, 3),
                                                            //new TextLoader.Column(nameof(TitanicData.Sex), DataKind.Text, 4),
                                                            //new TextLoader.Column(nameof(TitanicData.Age), DataKind.R4, 5),
                                                            //new TextLoader.Column(nameof(TitanicData.SibSp), DataKind.R4, 6),
                                                            //new TextLoader.Column(nameof(TitanicData.Parch), DataKind.R4, 7),
                                                            //new TextLoader.Column(nameof(TitanicData.Ticket), DataKind.Text, 8),
                                                            //new TextLoader.Column(nameof(TitanicData.Fare), DataKind.R4, 9),
                                                            //new TextLoader.Column(nameof(TitanicData.Cabin), DataKind.Text, 10),
                                                            //new TextLoader.Column(nameof(TitanicData.Embarked), DataKind.Text, 11)