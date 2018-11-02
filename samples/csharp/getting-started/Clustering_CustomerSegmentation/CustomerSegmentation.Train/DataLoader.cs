using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerSegmentation
{
    class DataLoader
    {
        MLContext _mlContext;
        private TextLoader _loader;

        public DataLoader(MLContext mlContext)
        {
            _mlContext = mlContext;

            // Create the TextLoader by defining the data columns and where to find (column position) them in the text file.
            _loader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                                {
                                                    Separator = ",",
                                                    HasHeader = true,
                                                    Column = new[]
                                                                {
                                                                  new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
                                                                  new TextLoader.Column("LastName", DataKind.Text, 32)
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
