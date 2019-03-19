using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRecommendationConsoleApp.DataStructures
{
    public class MovieRating
    {
        [LoadColumn(0)]
        public float userId;

        [LoadColumn(1)]
        public float movieId;

        [LoadColumn(2)]
        public float Label;
    }
}
