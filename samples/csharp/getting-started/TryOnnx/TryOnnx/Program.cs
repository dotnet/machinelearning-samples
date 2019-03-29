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
            var modelFilePath = Path.Combine("Model","model.onnx");
            var dataFile = @"images/images.tsv";
            var imagesFolder = Path.GetDirectoryName(dataFile);
            var tagsTsv = Path.Combine("images", "tags.tsv");


            var labelsTxt = Path.Combine("inception", "imagenet_comp_graph_label_strings.txt");
            var customInceptionPb = Path.Combine("inception_custom", "model_tf.pb");
            var customLabelsTxt = Path.Combine("inception_custom", "labels.txt");

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
    }
}



