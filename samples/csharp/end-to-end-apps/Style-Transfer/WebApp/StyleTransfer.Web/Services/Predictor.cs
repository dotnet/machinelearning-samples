using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.ImageAnalytics;
using StyleTransfer.Web.Utils;

namespace StyleTransfer.Web.Services
{
    public class Predictor
    {
        private readonly MLContext _mlContext = new MLContext();

        public string RunPrediction(string base64Image)
        {
            // Prepare input data

            // Create pipeline to execute our model

            return null;
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

    public class ImageInput
    {
        [LoadColumn(0)]
        public string ImagePath;
    }

    public class TensorInput
    {
        [VectorType(ImageConstants.ImageHeight, ImageConstants.ImageWidth, 3)]
        public float[] Placeholder;
    }

    public class TensorOutput
    {
        [ColumnName("add_37")]
        public float[] Output;
    }
}