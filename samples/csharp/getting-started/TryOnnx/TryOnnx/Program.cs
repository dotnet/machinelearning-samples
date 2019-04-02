using System;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ML.Transforms;
using Microsoft.ML;
using System.IO;

namespace TryOnnx
{
    class Program
    {
        public static void Main()
        {
            var assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);
            //var squeezemodelFilePath = Path.Combine(assetsPath, "Model","model.onnx");
            var yoloModelPath = Path.Combine(assetsPath, "Model","yolo", "tiny-yolov2-1.2.onnx");
            var imagesFolder = Path.Combine(assetsPath,"images");
            var tagsTsv = Path.Combine(assetsPath,"images", "tags.tsv");

            var modelFilePath = yoloModelPath;

            var labelsTxt = Path.Combine(assetsPath, "inception", "imagenet_comp_graph_label_strings.txt");
          

            try
            {
                var modelScorer = new OnnxModelScorer(tagsTsv, imagesFolder, modelFilePath, labelsTxt);
                modelScorer.Score();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
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



