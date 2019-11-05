using Microsoft.ML;
using Microsoft.ML.Data;
using WebRanking.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebRanking.Common
{
    public class ConsoleHelper
    {
        // To evaluate the accuracy of the model's predicted rankings, prints out the Discounted Cumulative Gain and Normalized Discounted Cumulative Gain for search queries.
        public static void EvaluateMetrics(MLContext mlContext, IDataView predictions, RankingEvaluatorOptions options)
        {
            // Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
            RankingMetrics metrics = mlContext.Ranking.Evaluate(predictions, options);

            Console.WriteLine($"DCG: {string.Join(", ", metrics.DiscountedCumulativeGains.Select((d, i) => $"@{i + 1}:{d:F4}").ToArray())}");

            Console.WriteLine($"NDCG: {string.Join(", ", metrics.NormalizedDiscountedCumulativeGains.Select((d, i) => $"@{i + 1}:{d:F4}").ToArray())}\n");
        }

        // Prints out the the individual scores used to determine the relative ranking.
        public static void PrintScores(IEnumerable<SearchResultPrediction> predictions)
        {
            foreach (var prediction in predictions)
            {
                Console.WriteLine($"GroupId: {prediction.GroupId}, Score: {prediction.Score}");
            }
        }
    }
}
