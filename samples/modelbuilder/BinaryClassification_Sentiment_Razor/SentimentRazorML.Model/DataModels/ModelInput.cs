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
        [ColumnName("Sentiment"), LoadColumn(0)]
        public bool Sentiment { get; set; }


        [ColumnName("SentimentText"), LoadColumn(1)]
        public string SentimentText { get; set; }


    }
}
