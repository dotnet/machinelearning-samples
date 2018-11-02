using System;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using System.IO;
using Microsoft.ML.Trainers.KMeans;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.PCA;

namespace CustomerSegmentation.Model
{
    public class ModelBuilder
    {
        private readonly MLContext env;

        public ModelBuilder(MLContext mlContext)
        {
            env = mlContext;
        }

        private TextLoader CreateTextLoader()
        {
            // Create the TextLoader by defining the data columns and where to find (column position) them in the text file.
            var reader = new TextLoader(env,
                new TextLoader.Arguments
                {
                    Column = new[] {
                        new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
                        new TextLoader.Column("LastName", DataKind.Text, 32)
                    },
                    HasHeader = true,
                    Separator = ","
                });
            return reader;
        }

        public TransformerChain<ClusteringPredictionTransformer<KMeansPredictor>> BuildAndTrain(string pivotLocation, int kClusters = 3, int rank = 2)
        {
            Console.Out.WriteLine($"Input file: {pivotLocation}");

            var pipeline = new PrincipalComponentAnalysisEstimator(env, "Features", "PCAFeatures", rank: rank)
                .Append(new OneHotEncodingEstimator(env, new[] { new OneHotEncodingEstimator.ColumnInfo("LastName", "LastNameKey", CategoricalTransform.OutputKind.Ind) }))
                .Append(new KMeansPlusPlusTrainer(env, "Features", clustersCount: kClusters));

            TextLoader reader = CreateTextLoader();
            var dataSource = reader.Read(new MultiFileSource(pivotLocation));
            var model = pipeline.Fit(dataSource);
            Console.WriteLine($"Train Done.");
            return model;
        }

        public void Evaluate(string pivotLocation, TransformerChain<ClusteringPredictionTransformer<KMeansPredictor>> model)
        {
            Common.ConsoleHelper.ConsoleWriteHeader("Evaluate model");
            TextLoader reader = CreateTextLoader();
            var dataSource = reader.Read(new MultiFileSource(pivotLocation));

            var data = model.Transform(dataSource);
            var clustering = new ClusteringContext(env);
            var metrics = clustering.Evaluate(data, score: "Score", features: "Features");
            Console.WriteLine($"AvgMinScore is: {metrics.AvgMinScore}");
            Console.WriteLine($"Dbi is: {metrics.Dbi}");


        }

        public void SaveModel(string modelLocation, TransformerChain<ClusteringPredictionTransformer<KMeansPredictor>> model)
        {
            Common.ConsoleHelper.ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(modelLocation);
            using (var f = new FileStream(modelLocation, FileMode.Create))
                model.SaveTo(env, f);
            Console.WriteLine($"Model saved: {modelLocation}");
        }
    }
}
