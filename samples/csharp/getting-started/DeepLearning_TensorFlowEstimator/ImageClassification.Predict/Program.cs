using System;
using System.IO;
using System.Threading.Tasks;
using ImageClassification.Model;
using static ImageClassification.Model.ConsoleHelpers;

namespace ImageClassification.Predict
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var assetsPath = ModelHelpers.GetAssetsPath(@"..\..\..\assets");

            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "data");
            var imageClassifierZip = Path.Combine(assetsPath, "inputs", "imageClassifier.zip");

            try
            {
                var modelScorer = new ModelScorer(tagsTsv, imagesFolder, imageClassifierZip);
                modelScorer.ClassifyImages();
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            ConsolePressAnyKey();
        }
    }
}
