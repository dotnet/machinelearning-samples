using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace BikeSharingDemand
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
                                                                new TextLoader.Column("Season", DataKind.R4, 2),
                                                                new TextLoader.Column("Year", DataKind.R4, 3),
                                                                new TextLoader.Column("Month", DataKind.R4, 4),
                                                                new TextLoader.Column("Hour", DataKind.R4, 5),
                                                                new TextLoader.Column("Holiday", DataKind.R4, 6),
                                                                new TextLoader.Column("Weekday", DataKind.R4, 7),
                                                                new TextLoader.Column("WorkingDay", DataKind.R4, 8),
                                                                new TextLoader.Column("Weather", DataKind.R4, 9),
                                                                new TextLoader.Column("Temperature", DataKind.R4, 10),
                                                                new TextLoader.Column("NormalizedTemperature", DataKind.R4, 11),
                                                                new TextLoader.Column("Humidity", DataKind.R4, 12),
                                                                new TextLoader.Column("Windspeed", DataKind.R4, 13),
                                                                new TextLoader.Column("Count", DataKind.R4, 16)
                                                                }
                                                }
            );
        }

        public IDataView GetDataView(string filePath)
        {
            return _loader.Read(filePath);
        }
    }
}
