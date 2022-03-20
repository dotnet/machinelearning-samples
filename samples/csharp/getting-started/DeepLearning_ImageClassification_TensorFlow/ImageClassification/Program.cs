using Common;
using ImageClassification.ModelScorer;
using System;
using System.Collections.Generic;
using System.IO;


namespace ImageClassification
{
    public class Program
    {
        static void Main(string[] args)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            // https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip = inception-v1.zip
            const string inceptionFile = "inception5h"; //"inception-v1";
            const string inceptionGraph = "tensorflow_inception_graph.pb";

            const string inceptionGraphZip = inceptionFile + ".zip";
            const string inceptionGraphUrl =
                "https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip"; // v1

            var tagsTsv = Path.Combine(assetsPath, "inputs", "images", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "images");
            var inceptionPb = Path.Combine(assetsPath, "inputs", "inception", inceptionGraph);
            var labelsTxt = Path.Combine(assetsPath, "inputs", "inception", 
                "imagenet_comp_graph_label_strings.txt");

            var inceptionFolder = Path.Combine(assetsPath, "inputs", "tensorflow-pretrained-models");
            var commonGraphsRelativePath = @"../../../../../../../../graphs";
            var commonGraphsPath = GetAbsolutePath(commonGraphsRelativePath);

            var fullIncepionFolderPath = Path.Combine(
                inceptionFolder, Path.GetFileNameWithoutExtension(inceptionGraphZip));
            List<string> destGraphFile = new List<string>() { inceptionPb };
            Web.DownloadBigFile(fullIncepionFolderPath, inceptionGraphUrl, inceptionGraphZip, commonGraphsPath, destGraphFile);

            try
            {
                var modelScorer = new TFModelScorer(tagsTsv, imagesFolder, inceptionPb, labelsTxt);
                modelScorer.Score();

            }
            catch (Exception ex)
            {
                ConsoleHelpers.ConsoleWriteException(ex.ToString());
            }

            ConsoleHelpers.ConsolePressAnyKey();
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
