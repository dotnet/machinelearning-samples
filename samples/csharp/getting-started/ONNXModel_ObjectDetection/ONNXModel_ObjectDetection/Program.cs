using Microsoft.ML;
using Microsoft.ML.Data;
using ObjectDetection.NewFolder;
using System.IO;
using System.Linq;

namespace ObjectDetection
{
    public class Program
    {
        private const int inputSize = 150528;

        public class TestData
        {
            [VectorType(inputSize)]
            public float[] data_0;
        }

        static void IU(string[] args)
        {
            var modelFile = Path.Combine(Directory.GetCurrentDirectory(), "Model", "model.onnx");
            var mlContext = new MLContext();
            var imageHeight = 224;
            var imageWidth = 224;
            var imagesFolder = GetAbsolutePath("../../../images");
            var DataLocation = GetAbsolutePath("../../../images/images.tsv");
            var modelLocation = GetAbsolutePath("../../../Model/model.onnx");
            //var imageFolder = Path.GetDirectoryName(dataFile);
            
             var data = mlContext.Data.LoadFromTextFile<ImageNetData>(DataLocation, hasHeader: true);
            //var data = TextLoaderStatic.CreateLoader(mlContext, ctx => (
            //    imagePath: ctx.LoadText(0),
            //    name: ctx.LoadText(1)))
            //    .Load(dataFile);

            //var pipe = d
            //   .Append(row => (
            //       row.name,
            //       data_0: row.imagePath.LoadAsImage(imageFolder).Resize(imageHeight, imageWidth).ExtractPixels(interleave: true)))
            //   .Append(row => (row.name, softmaxout_1: row.data_0.ApplyOnnxModel(modelFile)));

            var pipeline = mlContext.Transforms.LoadImages(imageFolder: imagesFolder, columnPairs: (outputColumnName: "ImageReal", inputColumnName: "data_0"))
                          .Append(mlContext.Transforms.ResizeImages(outputColumnName: "ImageReal", imageWidth: imageWidth, imageHeight: imageHeight, inputColumnName: "ImageReal"))
                          .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "ImageReal", interleave: true))
                          .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { "softmaxout_1" }, inputColumnNames: new[] { "data_0" }));

            var result = pipeline.Fit(data).Transform(data);

           
            // Retrieve model scores into Prediction class
            //var predictions = mlContext.CreateEnumerable<ImageNetPrediction>(result, reuseRowObject: false);

            // Iterate rows
            //foreach (var prediction in predictions)
            //{
            //    int numClasses = 0;
            //    foreach (var classScore in prediction.PredictedLabels.Take(3))
            //    {
            //        Console.WriteLine($"Class #{numClasses++} score = {classScore}");
            //    }
            //    Console.WriteLine(new string('-', 10));
            //}

            var softmaxOutCol = result.Schema["softmaxout_1"];

            //using (var cursor = result.GetRowCursor(softmaxOutCol))
            //{
            //    var buffer = default(VBuffer<float>);
            //    var getter = cursor.GetGetter<VBuffer<float>>(softmaxOutCol);
            //    var numRows = 0;
            //    while (cursor.MoveNext())
            //    {
            //        getter(ref buffer);
            //        //Assert.Equal(1000, buffer.Length);
            //        Console.WriteLine(buffer.Length);
            //        numRows += 1;
            //    }
            //    Console.WriteLine(numRows);
            //    //Assert.Equal(4, numRows);
            //}

        }


            public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
       }


    //public class TestData
    //{
    //    [VectorType(inputSize)]
    //    public float[] data_0;
    //}
    //public struct ImageNetSettings
    //{
    //    public const int imageHeight = 416;
    //    public const int imageWidth = 416;
    //}



}
