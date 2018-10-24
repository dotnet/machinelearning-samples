using Microsoft.ML.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BikeSharingDemand.Model
{
    public class BikeSharingTextLoaderFactory
    {
        public TextLoader CreateTextLoader(LocalEnvironment mlcontext)
        {
              // Create the TextLoader by defining the data columns and where to find (column position) them in the text file.
              return new TextLoader(mlcontext,
                                    new TextLoader.Arguments()
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
                                    });
            //If Using FeatureVector to load all the 11 columns to compose a single feature column.
            //
            //return new TextLoader(mlcontext,
            //                        new TextLoader.Arguments()
            //                        {
            //                            Separator = ",",
            //                            HasHeader = true,
            //                            Column = new[]
            //                            {
            //                               // We read the first 10 values as a single float vector.
            //                               new TextLoader.Column("FeatureVector", DataKind.R4, new[] {new TextLoader.Range(2, 12)}),
            //                               // Separately, read the target variable.
            //                               new TextLoader.Column("Count", DataKind.R4, 16)
            //                            }
            //                        });
        }

    }
}
