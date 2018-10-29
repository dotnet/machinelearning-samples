using CreditCardFraudDetection.Common;
using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;
using System.Linq;
using System.IO;
using Microsoft.ML.Runtime.Data.IO;

namespace CreditCardFraudDetection.Trainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var assetsPath = ConsoleHelpers.GetAssetsPath(@"..\..\..\assets");
            var zipDataSet = Path.Combine(assetsPath, "input", "creditcardfraud-dataset.zip");
            var dataSetFile = Path.Combine(assetsPath, "input", "creditcardfraud-dataset.csv");


            ConsoleHelpers.UnZipDataSet(zipDataSet, dataSetFile);

            TrainModelWithDynamicApi(assetsPath, dataSetFile);
            //TrainModelWithStaticApi(assetsPath, dataSetFile);

            ConsoleHelpers.ConsolePressAnyKey();
        }

        private static void TrainModelWithDynamicApi(string assetsPath, string dataSetFile) {
            var modelBuilder = new ModelBuilder(assetsPath, dataSetFile);
            modelBuilder.Build();
            modelBuilder.TrainFastTreeAndSaveModels();
        }

        private static void TrainModelWithStaticApi(string assetsPath, string dataSetFile) {

            var outputPath = Path.Combine(assetsPath, "output");
            // Create a new environment for ML.NET operations.
            // It can be used for exception tracking and logging, 
            // as well as the source of randomness.
            // Seed set to any number so you have a deterministic environment
            var env = new LocalEnvironment(1);

            var reader = TextLoader.CreateReader(env,
                            ctx => (
                                V1: ctx.LoadFloat(1),
                                V2: ctx.LoadFloat(2),
                                V3: ctx.LoadFloat(3),
                                V4: ctx.LoadFloat(4),
                                V5: ctx.LoadFloat(5),
                                V6: ctx.LoadFloat(6),
                                V7: ctx.LoadFloat(7),
                                V8: ctx.LoadFloat(8),
                                V9: ctx.LoadFloat(9),
                                V10: ctx.LoadFloat(10),
                                V11: ctx.LoadFloat(11),
                                V12: ctx.LoadFloat(12),
                                V13: ctx.LoadFloat(13),
                                V14: ctx.LoadFloat(14),
                                V15: ctx.LoadFloat(15),
                                V16: ctx.LoadFloat(16),
                                V17: ctx.LoadFloat(17),
                                V18: ctx.LoadFloat(18),
                                V19: ctx.LoadFloat(19),
                                V20: ctx.LoadFloat(20),
                                V21: ctx.LoadFloat(21),
                                V22: ctx.LoadFloat(22),
                                V23: ctx.LoadFloat(23),
                                V24: ctx.LoadFloat(24),
                                V25: ctx.LoadFloat(25),
                                V26: ctx.LoadFloat(26),
                                V27: ctx.LoadFloat(27),
                                V28: ctx.LoadFloat(28),
                                Amount: ctx.LoadFloat(29),
                                Label: ctx.LoadBool(30)),
                                separator: ',', hasHeader: true);

            // Now read the file 
            var data = reader.Read(new MultiFileSource(dataSetFile));
            // (remember though, readers are lazy, so the actual 
            //  reading will happen when the data is accessed).

            ConsoleHelpers.ConsoleWriteHeader("Peek/show 4 fraud transactions and 4 NOT fraud transactions, from the Training Dataset");
            ConsoleHelpers.InspectData(env, data.AsDynamic, 4);

            // We know that this is a Binary Classification task,
            // so we create a Binary Classification context:
            // it will give us the algorithms we need,
            // as well as the evaluation procedure.
            var classification = new BinaryClassificationContext(env);

            // Split the data 80:20 into train and test sets, train and evaluate.
            var (trainData, testData) = classification.TrainTestSplit(data, testFraction: 0.2);

            if (!File.Exists(Path.Combine(outputPath, "testData.idv"))) {
                // save test split
                using (var ch = env.Start("SaveData"))
                using (var file = env.CreateOutputFile(Path.Combine(outputPath, "testData.idv")))
                {
                    var saver = new BinarySaver(env, new BinarySaver.Arguments());
                    DataSaverUtils.SaveDataView(ch, saver, testData.AsDynamic, file);
                }
            }

            if (!File.Exists(Path.Combine(outputPath, "trainData.idv")))
            {
                // save train split
                using (var ch = env.Start("SaveData"))
                using (var file = env.CreateOutputFile(Path.Combine(outputPath, "trainData.idv")))
                {
                    var saver = new BinarySaver(env, new BinarySaver.Arguments());
                    DataSaverUtils.SaveDataView(ch, saver, trainData.AsDynamic, file);
                }
            }


            // Start creating our processing pipeline. 
            var estimator = reader.MakeNewEstimator()
                   .Append(row => (
                     row.Label,
                     // Concat all features
                     Features: row.V1.ConcatWith(row.V2, row.V3, row.V4, row.V5, row.V6, row.V7, row.V8, row.V9, row.V10, row.V11,
                                                 row.V12, row.V13, row.V14, row.V15, row.V16, row.V17, row.V18, row.V19, row.V20,
                                                 row.V21, row.V22, row.V23, row.V24, row.V25, row.V26, row.V27, row.V28, row.Amount)))
                   .Append(row => (
                        row.Label,
                        FeaturesNormalizedByMeanVar: row.Features.NormalizeByMeanVar() // normalize values
                      ))
                   .Append(row => (
                           row.Label,
                           Predictions: classification.Trainers.FastTree(row.Label, row.FeaturesNormalizedByMeanVar)));

            // Step three: Train the model.
            var model = estimator.Fit(trainData);

            // Now run the n - fold cross - validation experiment, using the same pipeline.
            int numFolds = 2;
            var cvResults = classification.CrossValidate(trainData, estimator, r => r.Label, numFolds: numFolds);

            // Let's get Cross Validate metrics           
            int count = 1;
            var cvModels = cvResults.ToList();
            cvModels.ForEach(result =>
            {
                ConsoleHelpers.ConsoleWriteHeader($"Train Metrics Cross Validate [{count}/{numFolds}]:");
                result.metrics.ToConsole();
                ConsoleHelpers.ConsoleWriteHeader($"Show 4 [model {count}]");
                ConsoleHelpers.InspectScoredData(env, result.scoredTestData.AsDynamic, 4);
                count++;
            });

            // save model with best accuracy
            var bestmodel = cvModels.OrderByDescending(result => result.metrics.Accuracy).Select(result => result.model).FirstOrDefault();
            bestmodel.AsDynamic.SaveModel(env, Path.Combine(outputPath, "cv-fastTree.zip"));
            System.Console.WriteLine("");
            System.Console.WriteLine("Saved best model.");

        }
    } 
}
