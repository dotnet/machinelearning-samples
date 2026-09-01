using System;
using System.Drawing;
using System.Linq;
using Microsoft.ML.Data;
using Microsoft.Extensions.ML;
using StyleTransfer.Web.ML.DataModels;
using StyleTransfer.Web.Utils;

namespace StyleTransfer.Web.Services
{
    public class Predictor
    {
        //Declare PredictionEnginePool

        public string RunPrediction(string base64Image)
        {

            //Prepare the input data

            ////Get PredictionEngine object from the Object Pool


        }

        /// <summary>Method that builds a new bitmap image based on the prediction output result</summary>
        private string ProcessResult(TensorOutput result)
        {
            var ints = result.Output.Select(x => (int)Math.Min(Math.Max(x, 0), 255)).ToArray();
            var bitmap = new Bitmap(ImageConstants.ImageWidth, ImageConstants.ImageHeight);
            var index = 0;
            for (var j = 0; j < ImageConstants.ImageHeight; j++)
            {
                for (var i = 0; i < ImageConstants.ImageWidth; i++)
                {
                    var r = ints[index++];
                    var g = ints[index++];
                    var b = ints[index++];

                    var color = Color.FromArgb(255, r, g, b);
                    bitmap.SetPixel(i, j, color);
                }
            }

            return ImageUtils.BitmapToBase64(bitmap);
        }
    }
}