using Microsoft.ML;
using OnnxObjectDetectionE2EAPP;
using OnnxObjectDetectionModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace OnnxObjectDetectionModel
{
    public class Program
    {
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
         }

        public static void Main(string[] args)
        {
            string assetsRelativePath = @"Assets";
            string imagesFolderRelativePath = Path.Combine(assetsRelativePath, "inputs", "ImagesTemp");
             
            string inputModelRelativePath = Path.Combine(assetsRelativePath, "inputs",  "TinyYolo2_model.onnx");
            string outputModelRelativePath = Path.Combine(assetsRelativePath, "outputs", "TinyYoloModel.zip");

            string imagesFolderPath = GetAbsolutePath(imagesFolderRelativePath);
            string inputModelPath = GetAbsolutePath(inputModelRelativePath);
            string outputModelPath = GetAbsolutePath(outputModelRelativePath);

            MLContext mlContext = new MLContext();

            try
            {
                var modelScorer = new OnnxModelScorer(mlContext,imagesFolderPath, inputModelPath, outputModelPath);
                modelScorer.CreateSaveModel();
                TestModel(mlContext, outputModelRelativePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("========= End of Process..Hit any Key ========");
            Console.ReadLine();
        }

        public static void TestModel(MLContext mlContext, string modelPath)
        {
            IList<YoloBoundingBox> _boxes = new List<YoloBoundingBox>();
            YoloWinMlParser _parser = new YoloWinMlParser();
            string imageFileRelativePath = @"assets/inputs/image1.jpg";
            string imageFilePath = GetAbsolutePath(imageFileRelativePath);
            var model = mlContext.Model.Load(modelPath, out var modelInputSchema);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(model);
            var imageInputData = new ImageNetData { ImagePath = imageFilePath };
            var probs = predictionEngine.Predict(imageInputData).PredictedLabels;
            _boxes = _parser.ParseOutputs(probs);
            var filteredBoxes = _parser.NonMaxSuppress(_boxes, 5, .5F);

            Console.WriteLine(".....The objects in the image {0} are detected as below....", Path.GetFileNameWithoutExtension(imageInputData.ImagePath));
            foreach (var box in filteredBoxes)
            {
                Console.WriteLine(box.Label + " and its Confidence score: " + box.Confidence);
            }
            Console.WriteLine("");
        }
    }
}



