using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Conversions;
using Microsoft.ML.Transforms.Text;

namespace GitHubLabeler
{
    internal class Predictor
    {
        public static void Train(string DataSetLocation, string ModelPath)
        {
            var mlContext = new MLContext();

            var reader = new TextLoader(mlContext,
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

            var trainData = reader.Read(new MultiFileSource(DataSetLocation));


            var pipeline = new ValueToKeyMappingEstimator(mlContext, "Area", "Label")
                  .Append(new TextFeaturizingEstimator(mlContext, "Title", "Title"))
                  .Append(new TextFeaturizingEstimator(mlContext, "Description", "Description"))
                  .Append(new ColumnConcatenatingEstimator(mlContext, "Features", "Title", "Description"))
                  .Append(new SdcaMultiClassTrainer(mlContext, "Features", "Label"))
                  .Append(new KeyToValueEstimator(mlContext, "PredictedLabel"));

            var context = new MulticlassClassificationContext(mlContext);

            Console.WriteLine("=============== Cross Validating and getting Evaluation Metrics ===============");
            var cvResults = context.CrossValidate(trainData, pipeline, labelColumn: "Label", numFolds: 5);

            var microAccuracies = cvResults.Select(r => r.metrics.AccuracyMicro);
            var macroAccuracies = cvResults.Select(r => r.metrics.AccuracyMacro);
            var logLoss = cvResults.Select(r => r.metrics.LogLoss);
            var logLossReduction = cvResults.Select(r => r.metrics.LogLossReduction);
                                                                                      
            Console.WriteLine("=============== Training model ===============");

            var model = pipeline.Fit(trainData);

            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                model.SaveTo(mlContext, fs);

            Console.WriteLine("Average MicroAccuracy: " + microAccuracies.Average());
            Console.WriteLine("Average MacroAccuracy: " + macroAccuracies.Average());
            Console.WriteLine("Average LogLoss: " + logLoss.Average());
            Console.WriteLine("Average LogLossReduction: " + logLossReduction.Average());

            Console.WriteLine("=============== End training ===============");
            Console.WriteLine("The model is saved to {0}", ModelPath);

        }

    }
}
