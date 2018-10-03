using System;
using System.IO;
using System.Threading.Tasks;
using ImageClassification.Model;
using static ImageClassification.Model.ConsoleHelpers;

namespace ImageClassification.Train
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var assetsPath = ModelHelpers.GetAssetsPath(@"..\..\..\assets");

            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "data");
            var inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");
            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            try
            {
                //var modelBuilderPipeline = new ModelTrainerPipeline(tagsTsv, imagesFolder, inceptionPb, imageClassifierZip);
                //await modelBuilderPipeline.BuildAndTrain();

                var modelBuilder = new ModelBuilder(tagsTsv, imagesFolder, inceptionPb, imageClassifierZip);
                modelBuilder.BuildAndTrain();

                //var modelEvaluator = new ModelEvaluator(
                //    ModelHelpers.GetAssetsPath("data", "tags.tsv"),
                //    ModelHelpers.GetAssetsPath("images"),
                //    ModelHelpers.GetAssetsPath("model", "imageClassifier.zip"));
                //await modelEvaluator.Evaluate();
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
