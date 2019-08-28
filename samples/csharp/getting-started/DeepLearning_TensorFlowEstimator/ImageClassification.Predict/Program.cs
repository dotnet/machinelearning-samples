using System;
using System.IO;
using System.Threading.Tasks;
using ImageClassification.Model;
using static ImageClassification.Model.ConsoleHelpers;

namespace ImageClassification.Predict
{
    class Program
    {
        static void Main(string[] args)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            
            var imagesFolder = Path.Combine(assetsPath, "inputs", "images-for-predictions");
            var imageClassifierZip = Path.Combine(assetsPath, "inputs", "MLNETModel", "imageClassifier.zip");

            try
            {
                var modelScorer = new ModelScorer(imagesFolder, imageClassifierZip);
                modelScorer.ClassifyImages();
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.ToString());
            }

            ConsolePressAnyKey();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
