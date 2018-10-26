using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using Microsoft.ML.Runtime.Learners;
using System;
using System.IO;

namespace Clustering_Iris
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string DataPath => Path.Combine(AppPath, "datasets", "iris-full.txt");
        private static string ModelPath => Path.Combine(AppPath, "IrisModel.zip");


        private static void Main(string[] args)
        {
            // Create ML.NET context/environment
            using (var env = new LocalEnvironment())
            {
                // Create DataReader with data schema mapped to file's columns
                var reader = new TextLoader(env,
                                new TextLoader.Arguments()
                                {
                                    Separator = "\t",
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

                // Transform your data and add a learner
                // Add a learning algorithm to the pipeline. e.g.(What are characteristics of iris is this?)
                // Convert the Label back into original text (after converting to number in step 3)
                var pipeline = new ConcatEstimator(env, "Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth")
                       .Append(new KMeansPlusPlusTrainer(env, "Features",clustersCount:3));

                // Create and train the model            
                Console.WriteLine("=============== Create and Train the Model ===============");

                var model = pipeline.Fit(trainingDataView);

                Console.WriteLine("=============== End of training ===============");
                Console.WriteLine();

                // Test with one sample text 
                var sampleIrisData = new IrisData()
                {
                    SepalLength = 3.3f,
                    SepalWidth = 1.6f,
                    PetalLength = 0.2f,
                    PetalWidth = 5.1f,
                };

                var prediction = model.MakePredictionFunction<IrisData, IrisPrediction>(env).Predict(
                    sampleIrisData);

                Console.WriteLine($"Clusters assigned for setosa flowers:"+prediction.SelectedClusterId);
                // Save model to .ZIP file
                SaveModelAsFile(env, model);

                // Predict again but now testing the model loading from the .ZIP file
                PredictWithModelLoadedFromFile(sampleIrisData);

                Console.WriteLine("=============== End of process, hit any key to finish ===============");
                Console.ReadKey();
            }


        }

        private static void SaveModelAsFile(LocalEnvironment env, TransformerChain<ClusteringPredictionTransformer<KMeansPredictor>> model)
        {
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                model.SaveTo(env, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private static void PredictWithModelLoadedFromFile(IrisData sampleData)
        {
            // Test with Loaded Model from .zip file

            using (var env = new LocalEnvironment())
            {
                ITransformer loadedModel;
                using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    loadedModel = TransformerChain.LoadFrom(env, stream);
                }

                // Create prediction engine and make prediction.
                var prediction = loadedModel.MakePredictionFunction<IrisData, IrisPrediction>(env).Predict(
                    new IrisData()
                    {
                        SepalLength = 3.3f,
                        SepalWidth = 1.6f,
                        PetalLength = 0.2f,
                        PetalWidth = 5.1f,
                    });

                Console.WriteLine();
                Console.WriteLine($"Clusters assigned for setosa flowers:" + prediction.SelectedClusterId);
            }
        }

    }



    // Define your data structures
    public class IrisData
    {
        [Column("0")]
        public float Label;

        [Column("1")]
        public float SepalLength;

        [Column("2")]
        public float SepalWidth;

        [Column("3")]
        public float PetalLength;

        [Column("4")]
        public float PetalWidth;

    }

    // IrisPrediction is the result returned from prediction operations
    public class IrisPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId;

        [ColumnName("Score")]
        public float[] Distance;
    }
}
