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
        public static void EvaluateMetrics(MLContext mlContext, IDataView predictions)
        {
            // Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
            RankingMetrics metrics = mlContext.Ranking.Evaluate(predictions);

            Console.WriteLine($"DCG: {string.Join(", ", metrics.DiscountedCumulativeGains.Select((d, i) => $"@{i + 1}:{d:F4}").ToArray())}");

            Console.WriteLine($"NDCG: {string.Join(", ", metrics.NormalizedDiscountedCumulativeGains.Select((d, i) => $"@{i + 1}:{d:F4}").ToArray())}\n");
        }

        // Performs evaluation with the truncation level set up to 10 search results within a query.
        // This is a temporary workaround for this issue: https://github.com/dotnet/machinelearning/issues/2728.
        public static void EvaluateMetrics(MLContext mlContext, IDataView predictions, int truncationLevel)
        {
            if (truncationLevel < 1 || truncationLevel > 10)
            {
                throw new InvalidOperationException("Currently metrics are only supported for 1 to 10 truncation levels.");
            }

            //  Uses reflection to set the truncation level before calling evaluate.
            var mlAssembly = typeof(TextLoader).Assembly;
            var rankEvalType = mlAssembly.DefinedTypes.Where(t => t.Name.Contains("RankingEvaluator")).First();

            var evalArgsType = rankEvalType.GetNestedType("Arguments");
            var evalArgs = Activator.CreateInstance(rankEvalType.GetNestedType("Arguments"));

            var dcgLevel = evalArgsType.GetField("DcgTruncationLevel");
            dcgLevel.SetValue(evalArgs, truncationLevel);

            var ctor = rankEvalType.GetConstructors().First();
            var evaluator = ctor.Invoke(new object[] { mlContext, evalArgs });

            var evaluateMethod = rankEvalType.GetMethod("Evaluate");
            RankingMetrics metrics = (RankingMetrics)evaluateMethod.Invoke(evaluator, new object[] { predictions, "Label", "GroupId", "Score" });

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
