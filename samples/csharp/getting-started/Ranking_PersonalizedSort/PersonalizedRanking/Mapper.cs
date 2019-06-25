using Microsoft.ML;
using PersonalizedRanking.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalizedRanking
{
    public class Mapper
    {
        // Custom mapper used to label a hotel search result with the ideal rank.  
        // This is based on guidelines provided by Expedia: https://www.kaggle.com/c/expedia-personalized-sort/overview/evaluation.
        public static Action<HotelData, HotelRelevance> GetLabelMapper(MLContext mlContext, IDataView data)
        {
            Action<HotelData, HotelRelevance> mapper = (input, output) =>
            {
                if (input.Srch_Result_Booked == 1)
                {
                    output.Label = 2;
                }
                else if (input.Srch_Result_Clicked == 1)
                {
                    output.Label = 1;
                }
                else
                {
                    output.Label = 0;
                }
            };

            return mapper;
        }
    }
}
