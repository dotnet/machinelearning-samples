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
        private readonly string pivotLocation;
        private readonly string modelLocation;
        private readonly string plotLocation;
        private readonly LocalEnvironment env;

        public ModelBuilder(string pivotLocation, string modelLocation, string plotLocation)
        {
            this.pivotLocation = pivotLocation;
            this.modelLocation = modelLocation;
            this.plotLocation = plotLocation;
            env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
        }

        private class PivotPipelineData {
            // Features,LastName,PCAFeatures,PredictedLabel,Score,preds.score,preds.predictedLabel
            public float[] Features;
            public string LastName;
            public float[] PCAFeatures;
            public float[] Score;
        }

        public void BuildAndTrain(int kClusters = 3)
        {
            ConsoleWriteHeader("Build and Train using Static API");
            Console.Out.WriteLine($"Input file: {pivotLocation}");

            ConsoleWriteHeader("Reading file ...");
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


            var estrimator = new PcaEstimator(env, "Features", "PCAFeatures", rank: 2, advancedSettings: (p) => p.Seed = 42)
            .Append(new CategoricalEstimator(env, new[] { new CategoricalEstimator.ColumnInfo("LastName", "LastNameKey", CategoricalTransform.OutputKind.Ind) }))
            .Append(new KMeansPlusPlusTrainer(env, "Features", clustersCount: kClusters));


            ConsoleWriteHeader("Training model for customer clustering");

            var dataSource = reader.Read(new MultiFileSource(pivotLocation));
            var model = estrimator.Fit(dataSource);
            var data = model.Transform(dataSource);

            // inspect data
            var columnNames = data.Schema.GetColumnNames().ToArray();
            var trainDataAsEnumerable = data.AsEnumerable<PivotPipelineData>(env, false).Take(10).ToArray();

            ConsoleWriteHeader("Evaluate model");

            var clustering = new ClusteringContext(env);
            var metrics = clustering.Evaluate(data, score: "Score", features: "Features");
            Console.WriteLine($"AvgMinScore is: {metrics.AvgMinScore}");
            Console.WriteLine($"Dbi is: {metrics.Dbi}");

            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(modelLocation);
            using (var f = new FileStream(modelLocation, FileMode.Create))
                model.SaveTo(env, f);
            Console.WriteLine($"Model saved: {modelLocation}");

        }
    }
}
