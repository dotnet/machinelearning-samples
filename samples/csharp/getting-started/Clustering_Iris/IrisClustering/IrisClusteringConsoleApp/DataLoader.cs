using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clustering_Iris
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
                                                    Separator = "\t",
                                                    HasHeader = true,
                                                    Column = new[]
                                                    {
                                                                    new TextLoader.Column("Label", DataKind.R4, 0),
                                                                    new TextLoader.Column("SepalLength", DataKind.R4, 1),
                                                                    new TextLoader.Column("SepalWidth", DataKind.R4, 2),
                                                                    new TextLoader.Column("PetalLength", DataKind.R4, 3),
                                                                    new TextLoader.Column("PetalWidth", DataKind.R4, 4),
                                                    }
                                                 });
        }

        public IDataView GetDataView(string filePath)
        {
            return _loader.Read(filePath);
        }
    }
}

