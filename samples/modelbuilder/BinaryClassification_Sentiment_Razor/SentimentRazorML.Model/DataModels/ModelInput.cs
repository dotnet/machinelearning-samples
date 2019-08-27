//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace SentimentRazorML.Model.DataModels
{
    public class ModelInput
    {
        [ColumnName("Comment"), LoadColumn(0)]
        public string Comment { get; set; }


        [ColumnName("Sentiment"), LoadColumn(1)]
        public bool Sentiment { get; set; }


    }
}
