using Microsoft.ML;
using System;

namespace Ranking
{
    class Program
    {
        const string TrainDatasetUrl = "https://aka.ms/mlnet-resources/benchmarks/MSLRWeb10KTrain720kRows.tsv";

        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 0);

            BuildTrainEvaluateAndSaveModel(mlContext);

            TestSinglePrediction(mlContext);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            throw new NotImplementedException();
        }

        private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            throw new NotImplementedException();
        }
    }
}
