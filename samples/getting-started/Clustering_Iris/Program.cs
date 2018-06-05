using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;

namespace Clustering_Iris
{
    public static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string DataPath => Path.Combine(AppPath, "datasets", "iris-full.txt");
        private static string ModelPath => Path.Combine(AppPath, "IrisClustersModel.zip");

        private static async Task Main(string[] args)
        {
            // STEP 1: Create a model
            var model = await TrainAsync();
        
            // STEP 2: Make a prediction
            Console.WriteLine();
            var prediction1 = model.Predict(TestIrisData.Setosa1);
            var prediction2 = model.Predict(TestIrisData.Setosa2);          
            Console.WriteLine($"Clusters assigned for setosa flowers:");
            Console.WriteLine($"                                        {prediction1.SelectedClusterId}");
            Console.WriteLine($"                                        {prediction2.SelectedClusterId}");

            var prediction3 = model.Predict(TestIrisData.Virginica1);
            var prediction4 = model.Predict(TestIrisData.Virginica2);
            Console.WriteLine($"Clusters assigned for virginica flowers:");
            Console.WriteLine($"                                        {prediction3.SelectedClusterId}");
            Console.WriteLine($"                                        {prediction4.SelectedClusterId}");
            
            var prediction5 = model.Predict(TestIrisData.Versicolor1);
            var prediction6 = model.Predict(TestIrisData.Versicolor2);
            Console.WriteLine($"Clusters assigned for versicolor flowers:");
            Console.WriteLine($"                                        {prediction5.SelectedClusterId}");
            Console.WriteLine($"                                        {prediction6.SelectedClusterId}");
            Console.ReadLine();
        }

        internal static async Task<PredictionModel<IrisData, ClusterPrediction>> TrainAsync()
        {
            // LearningPipeline holds all steps of the learning process: data, transforms, learners.
            var pipeline = new LearningPipeline
            {
                // The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
                // all the column names and their types.
                new TextLoader(DataPath).CreateFrom<IrisData>(useHeader: true),
                // ColumnConcatenator concatenates all columns into Features column
                new ColumnConcatenator("Features",
                    "SepalLength",
                    "SepalWidth",
                    "PetalLength",
                    "PetalWidth"),
                // KMeansPlusPlusClusterer is an algorithm that will be used to build clusters. We set the number of clusters to 3.
                new KMeansPlusPlusClusterer() { K = 3 }
            };

            Console.WriteLine("=============== Training model ===============");
            var model = pipeline.Train<IrisData, ClusterPrediction>();
            Console.WriteLine("=============== End training ===============");
            
            // Saving the model as a .zip file.
            await model.WriteAsync(ModelPath);
            Console.WriteLine("The model is saved to {0}", ModelPath);
           
            return model;
        }
    }
}