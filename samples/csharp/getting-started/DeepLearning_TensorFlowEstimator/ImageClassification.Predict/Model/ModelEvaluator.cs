using System;
using System.Linq;
using ImageClassification.ImageData;
using System.IO;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime;
using static ImageClassification.Model.ConsoleHelpers;

namespace ImageClassification.Model
{
    public class ModelScorer
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly IHostEnvironment env;

        public ModelScorer(string dataLocation, string imagesFolder, string modelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            env = new ConsoleEnvironment( seed: 1);
        }

        public void ClassifyImages()
        {
            ConsoleWriteHeader("Loading model");
            Console.WriteLine($"Model loaded: {modelLocation}");

            // Load the model
            ITransformer loadedModel;
            using (var f = new FileStream(modelLocation, FileMode.Open))
                loadedModel = TransformerChain.LoadFrom(env, f);

            // Make prediction function (input = ImageNetData, output = ImageNetPrediction)
            var predictor = loadedModel.MakePredictionFunction<ImageNetData, ImageNetPrediction>(env);
            // Read csv file into List<ImageNetData>
            var testData = ImageNetData.ReadFromCsv(dataLocation, imagesFolder).ToList();

            ConsoleWriteHeader("Making classifications");
            // There is a bug (https://github.com/dotnet/machinelearning/issues/1138), 
            // that always buffers the response from the predictor
            // so we have to make a copy-by-value op everytime we get a response
            // from the predictor
            testData
                .Select(td => new { td, pred = predictor.Predict(td) })
                .Select(pr => (pr.td.ImagePath, pr.pred.PredictedLabelValue, pr.pred.Score))
                .ToList()
                .ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath, pr.PredictedLabelValue, pr.Score.Max()));
        }
    }
}
