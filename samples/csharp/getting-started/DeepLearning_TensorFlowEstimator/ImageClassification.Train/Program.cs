using System;
using System.IO;
using System.Threading.Tasks;
using ImageClassification.Model;
using static ImageClassification.Model.ConsoleHelpers;

namespace ImageClassification.Train
{
    public class Program
    {
        static void Main(string[] args)
        {
            var assetsPath = ModelHelpers.GetAssetsPath(@"..\..\..\assets");

            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "data");
            var inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");
            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            try
            {
                var modelBuilder = new ModelBuilder(tagsTsv, imagesFolder, inceptionPb, imageClassifierZip);
                modelBuilder.BuildAndTrain();
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
