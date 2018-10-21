using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;

namespace GitHubLabeler
{
    internal class Predictor
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string DataPath => Path.Combine(AppPath, "datasets", "corefx-issues-train.tsv");
        private static string ModelPath => Path.Combine(AppPath, "GitHubLabelerModel.zip");

        public static void Train()
        {
            using (var env = new LocalEnvironment())
            {
                var reader = new TextLoader(env,
                    new TextLoader.Arguments()
                    {
                        Separator = "tab",
                        HasHeader = true,
                        Column = new[]
                        {
                            new TextLoader.Column("ID", DataKind.Text, 0),
                            new TextLoader.Column("Area", DataKind.Text, 1),
                            new TextLoader.Column("Title", DataKind.Text, 2),
                            new TextLoader.Column("Description", DataKind.Text, 3),
                        }
                    });

                var pipeline = new TermEstimator(env, "Area", "Label")
                    .Append(new TextTransform(env, "Title", "Title"))
                    .Append(new TextTransform(env, "Description", "Description"))
                    .Append(new ConcatEstimator(env, "Features", "Title", "Description"))
                    .Append(new SdcaMultiClassTrainer(env, new SdcaMultiClassTrainer.Arguments()))
                    .Append(new KeyToValueEstimator(env, "PredictedLabel"));

                Console.WriteLine("=============== Training model ===============");

                var model = pipeline.Fit(reader.Read(new MultiFileSource(DataPath)));

                using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                    model.SaveTo(env, fs);

                Console.WriteLine("=============== End training ===============");
                Console.WriteLine("The model is saved to {0}", ModelPath);
            }

        }

        public static string Predict(GitHubIssue issue)
        {
            using (var env = new LocalEnvironment())
            {
                ITransformer loadedModel;
                using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    loadedModel = TransformerChain.LoadFrom(env, stream);
                }

                // Create prediction engine and make prediction.
                var engine = loadedModel.MakePredictionFunction<GitHubIssue, GitHubIssuePrediction>(env);

                var prediction = engine.Predict(issue);

                return prediction.Area;
            }
        }

    }
}
