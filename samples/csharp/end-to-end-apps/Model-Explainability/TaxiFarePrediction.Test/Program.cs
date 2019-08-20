using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaxiFareRegression
{
    internal static class Program
    {
        private static string BaseRelativePath = @"../../../../TaxiFarePrediction";
        private static string BaseDataPath = Path.Combine(Path.GetFullPath(BaseRelativePath), "inputs");
        private static string TestDataPath = Path.Combine(BaseDataPath, "taxi-fare-test.csv");
        private static string ModelPath = Path.Combine(BaseRelativePath, "outputs", "TaxiFareModel.zip");

        static void Main(string[] args)
        {
            var modelPredictor = new Predictor(ModelPath, TestDataPath);
            List<DataStructures.TaxiFarePrediction> predictions = modelPredictor.RunMultiplePredictions(numberOfPredictions: 5);
            Console.WriteLine(JsonConvert.SerializeObject(predictions, Formatting.Indented));
            
        }

        
    }
}
