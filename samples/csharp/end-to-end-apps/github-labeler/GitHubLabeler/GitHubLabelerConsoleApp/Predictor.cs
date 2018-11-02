using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
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

                var trainData = reader.Read(new MultiFileSource(DataPath));

                var pipeline = new TermEstimator(env, "Area", "Label")
                    .Append(new TextTransform(env, "Title", "Title"))
                    .Append(new TextTransform(env, "Description", "Description"))
                    .Append(new ConcatEstimator(env, "Features", "Title", "Description"))
                    .Append(new SdcaMultiClassTrainer(env, new SdcaMultiClassTrainer.Arguments()))
                    .Append(new KeyToValueEstimator(env, "PredictedLabel"));

                var context = new MulticlassClassificationContext(env);

                var cvResults = context.CrossValidate(trainData, pipeline, labelColumn: "Label", numFolds: 5);

                var microAccuracies = cvResults.Select(r => r.metrics.AccuracyMicro);
                var macroAccuracies = cvResults.Select(r => r.metrics.AccuracyMacro);
                var logLoss = cvResults.Select(r => r.metrics.LogLoss);
                var logLossReduction = cvResults.Select(r => r.metrics.LogLossReduction);
                                                                                      
                Console.WriteLine("=============== Training model ===============");

                var model = pipeline.Fit(trainData);

                using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                    model.SaveTo(env, fs);

                Console.WriteLine("Average MicroAccuracy: " + microAccuracies.Average());
                Console.WriteLine("Average MacroAccuracy: " + macroAccuracies.Average());
                Console.WriteLine("Average LogLoss: " + logLoss.Average());
                Console.WriteLine("Average LogLossReduction: " + logLossReduction.Average());

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
