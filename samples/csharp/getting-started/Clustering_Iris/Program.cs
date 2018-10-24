using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using System.Collections.Generic;

namespace Clustering_Iris
{
    public static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string DataPath => Path.Combine(AppPath, "datasets", "iris-full.txt");

        private static void Main(string[] args)
        {
            //1. Create ML.NET context/environment
            using (var env = new LocalEnvironment())
            {
                //2. Create DataReader with data schema mapped to file's columns
                var reader = new TextLoader(env,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = "tab",
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

                //Load training data
                IDataView trainingDataView = reader.Read(new MultiFileSource(DataPath));


                //3.Create a flexible pipeline (composed by a chain of estimators + trainer) for creating/traing the model.
                var pipeline =
                    new ConcatEstimator(env, "Features", new[] { "SepalLength", "SepalWidth", "PetalLength", "PetalWidth" })
                           .Append(new KMeansPlusPlusTrainer(env, "Features", 3,
                           advancedSettings: s => { s.InitAlgorithm = KMeansPlusPlusTrainer.InitAlgorithm.KMeansPlusPlus; }));

                var model = pipeline.Fit(trainingDataView);


                // Create the prediction function 
                var predictionFunct = model.MakePredictionFunction<IrisData, ClusterPrediction>(env);


                // 4.Make a prediction
                Console.WriteLine();
                var prediction1 = predictionFunct.Predict(TestIrisData.Setosa1);
                var prediction2 = predictionFunct.Predict(TestIrisData.Setosa2);
                Console.WriteLine($"Clusters assigned for setosa flowers:");
                Console.WriteLine($"                                        {prediction1.SelectedClusterId}");
                Console.WriteLine($"                                        {prediction2.SelectedClusterId}");

                var prediction3 = predictionFunct.Predict(TestIrisData.Virginica1);
                var prediction4 = predictionFunct.Predict(TestIrisData.Virginica2);
                Console.WriteLine($"Clusters assigned for virginica flowers:");
                Console.WriteLine($"                                        {prediction3.SelectedClusterId}");
                Console.WriteLine($"                                        {prediction4.SelectedClusterId}");

                var prediction5 = predictionFunct.Predict(TestIrisData.Versicolor1);
                var prediction6 = predictionFunct.Predict(TestIrisData.Versicolor2);
                Console.WriteLine($"Clusters assigned for versicolor flowers:");
                Console.WriteLine($"                                        {prediction5.SelectedClusterId}");
                Console.WriteLine($"                                        {prediction6.SelectedClusterId}");
            }
        }
    }
}