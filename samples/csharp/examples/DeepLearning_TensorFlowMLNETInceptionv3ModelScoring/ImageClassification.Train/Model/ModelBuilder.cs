using Microsoft.ML.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageClassification.ImageData;
using static ImageClassification.Model.ConsoleHelpers;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.ImageAnalytics;
using Microsoft.ML.Transforms;
using Microsoft.ML.Runtime;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;

namespace ImageClassification.Model
{
    public class ModelBuilder
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;
        private readonly IHostEnvironment env;

        public ModelBuilder(string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.inputModelLocation = inputModelLocation;
            this.outputModelLocation = outputModelLocation;
            env = new LocalEnvironment();
        }

        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        public void BuildAndTrain()
        {
            var featurizerModelLocation = inputModelLocation;

            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {featurizerModelLocation}");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {dataLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), image mean: {ImageNetSettings.mean}");

            var sdcaContext = new MulticlassClassificationContext(env);
            MulticlassLogisticRegressionPredictor pred = null;
            //var loss = new HingeLoss(new HingeLoss.Arguments() { Margin = 1 });

            var reader = TextLoader.CreateReader(env,
                ctx => (ImagePath: ctx.LoadText(0), Label: ctx.LoadText(1)));

            var estimator = reader.MakeNewEstimator()
                .Append(row => (
                    row.Label,
                    input: row.ImagePath
                                .LoadAsImage(imagesFolder)
                                .Resize(ImageNetSettings.imageHeight, ImageNetSettings.imageWidth)
                                .ExtractPixels(interleaveArgb: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean)))
                .Append(row => (row.Label, LabelToKey: row.Label.ToKey(), softmax2_pre_activation: row.input.ApplyTensorFlowGraph(featurizerModelLocation)))
                .Append(row => (row.Label, preds: sdcaContext.Trainers.Sdca(row.LabelToKey, row.softmax2_pre_activation, maxIterations: 4, onFit: p => pred = p/*, loss: loss*/)))
                .Append(row => (row.Label, 
                                Score: row.preds.score, 
                                PredictedLabel: row.preds.predictedLabel.ToValue(),
                                Probability: row.preds.predictedLabel.ToVector()));

            var dataSource = new MultiFileSource(dataLocation);

            ConsoleWriteHeader("Training classification model");
            var model = estimator.Fit(reader.Read(dataSource));

            ConsoleWriteHeader("Results from metrics");
            var trainData = model.Transform(reader.Read(dataSource)).AsDynamic;
            var loadedModelOutputColumnNames = trainData.Schema.GetColumnNames();
            var trainData2 = trainData.AsEnumerable<ImageNetPipeline>(env, false, true).ToList();
            trainData2.ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath, pr.PredictedLabel, pr.Score.Max()));
            var metrics = sdcaContext.Evaluate(trainData, label: "LabelToKey", predictedLabel: "PredictedLabel");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(",", metrics.PerClassLogLoss.Select(c => c.ToString()))}");


            ConsoleWriteHeader("Results from predictor");
            var predictor = model.AsDynamic.MakePredictionFunction<ImageNetData, ImageNetStaticPrediction>(env);
            var testData = ImageNetData.ReadFromCsv(dataLocation, imagesFolder).ToList();

            Console.WriteLine("Using same predictor object");
            var temp = testData
                .Select(td => (td, pred: predictor.Predict(td)))
                .ToList();
            var temp1 = temp[1].pred;
            var temp2 = temp[2].pred;
            var ntemp = new ImageNetStaticPrediction();
            temp.ForEach(pr => ConsoleWriteImagePrediction(pr.td.ImagePath, pr.pred.PredictedLabel, pr.pred.Probability.Max()));

            Console.WriteLine(" ");
            Console.WriteLine("Copy predictor objector");
            testData
                .Select(td => new { td, pred = predictor.Predict(td) })
                .Select(pr => (pr.td.ImagePath, pr.pred.PredictedLabel, pr.pred.Probability))
                .ToList()
                .ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath, pr.PredictedLabel, pr.Probability.Max()));

            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputModelLocation);
            using (var f = new FileStream(outputModelLocation, FileMode.Create))
                model.AsDynamic.SaveTo(env, f);
            Console.WriteLine($"Model saved: {outputModelLocation}");
        }

    }
}
