using System;
using static CustomerSegmentation.Model.ConsoleHelpers;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using System.IO;

namespace CustomerSegmentation.Model
{
    public class ModelBuilder
    {
        private readonly LocalEnvironment env;

        public ModelBuilder(LocalEnvironment mlContext)
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

        public TransformerChain<ClusteringPredictionTransformer<KMeansPredictor>> BuildAndTrain(string pivotLocation, int kClusters = 3, int rank = 2, int seed = 42)
        {
            ConsoleWriteHeader("Build and Train using Static API");
            Console.Out.WriteLine($"Input file: {pivotLocation}");

            var pipeline = new PcaEstimator(env, "Features", "PCAFeatures", rank: rank, advancedSettings: (p) => p.Seed = seed)
                .Append(new CategoricalEstimator(env, new[] { new CategoricalEstimator.ColumnInfo("LastName", "LastNameKey", CategoricalTransform.OutputKind.Ind) }))
                .Append(new KMeansPlusPlusTrainer(env, "Features", clustersCount: kClusters));

            ConsoleWriteHeader("Training model for customer clustering");
            TextLoader reader = CreateTextLoader();
            var dataSource = reader.Read(new MultiFileSource(pivotLocation));
            var model = pipeline.Fit(dataSource);
            Console.WriteLine($"Train Done.");
            return model;
        }

        public void Evaluate(string pivotLocation, TransformerChain<ClusteringPredictionTransformer<KMeansPredictor>> model)
        {
            ConsoleWriteHeader("Evaluate model");
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
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(modelLocation);
            using (var f = new FileStream(modelLocation, FileMode.Create))
                model.SaveTo(env, f);
            Console.WriteLine($"Model saved: {modelLocation}");
        }

        //public void BuildAndTrain(int kClusters = 3)
        //{
        //    ConsoleWriteHeader("Build and Train using Static API");
        //    Console.Out.WriteLine($"Input file: {pivotLocation}");

        //    ConsoleWriteHeader("Reading file ...");
        //    var reader = new TextLoader(env,
        //        new TextLoader.Arguments
        //        {
        //            Column = new[] {
        //                new TextLoader.Column("Features", DataKind.R4, new[] {new TextLoader.Range(0, 31) }),
        //                new TextLoader.Column("LastName", DataKind.Text, 32)
        //            },
        //            HasHeader = true,
        //            Separator = ","
        //        });


        //    var estrimator = new PcaEstimator(env, "Features", "PCAFeatures", rank: 2, advancedSettings: (p) => p.Seed = 42)
        //    .Append(new CategoricalEstimator(env, new[] { new CategoricalEstimator.ColumnInfo("LastName", "LastNameKey", CategoricalTransform.OutputKind.Ind) }))
        //    .Append(new KMeansPlusPlusTrainer(env, "Features", clustersCount: kClusters));


        //    ConsoleWriteHeader("Training model for customer clustering");

        //    var dataSource = reader.Read(new MultiFileSource(pivotLocation));
        //    var model = estrimator.Fit(dataSource);
        //    var data = model.Transform(dataSource);

        //    // inspect data
        //    var columnNames = data.Schema.GetColumnNames().ToArray();
        //    var trainDataAsEnumerable = data.AsEnumerable<PivotPipelineData>(env, false).Take(10).ToArray();

        //    ConsoleWriteHeader("Evaluate model");

        //    var clustering = new ClusteringContext(env);
        //    var metrics = clustering.Evaluate(data, score: "Score", features: "Features");
        //    Console.WriteLine($"AvgMinScore is: {metrics.AvgMinScore}");
        //    Console.WriteLine($"Dbi is: {metrics.Dbi}");

        //    ConsoleWriteHeader("Save model to local file");
        //    ModelHelpers.DeleteAssets(modelLocation);
        //    using (var f = new FileStream(modelLocation, FileMode.Create))
        //        model.SaveTo(env, f);
        //    Console.WriteLine($"Model saved: {modelLocation}");

        //}
    }
}
