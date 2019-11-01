using System;
using System.IO;
using System.Linq;
using ImageClassification.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ImageClassification.Predict
{
    internal class Program
    {
        private static void Main()
        {
            const string assetsRelativePath = @"../../../assets";
            var assetsPath = GetAbsolutePath(assetsRelativePath);

            var imagesFolderPathForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions");

            var imageClassifierModelZipFilePath = Path.Combine(assetsPath, "inputs", "MLNETModel", "imageClassifier.zip");

            try
            {
                var mlContext = new MLContext(seed: 1);

                Console.WriteLine($"Loading model from: {imageClassifierModelZipFilePath}");

                // Load the model
                var loadedModel = mlContext.Model.Load(imageClassifierModelZipFilePath, out var modelInputSchema);

                // Create prediction engine to try a single prediction (input = ImageData, output = ImagePrediction)
                var predictionEngine = mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(loadedModel);

                //Predict the first image in the folder
                var imagesToPredict = FileUtils.LoadInMemoryImagesFromDirectory(imagesFolderPathForPredictions, false);

                var imageToPredict = imagesToPredict.First();

                // Measure #1 prediction execution time.
                var watch = System.Diagnostics.Stopwatch.StartNew();

                var prediction = predictionEngine.Predict(imageToPredict);

                // Stop measuring time.
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("First Prediction took: " + elapsedMs + "mlSecs");

                // Measure #2 prediction execution time.
                var watch2 = System.Diagnostics.Stopwatch.StartNew();

                var prediction2 = predictionEngine.Predict(imageToPredict);

                // Stop measuring time.
                watch2.Stop();
                var elapsedMs2 = watch2.ElapsedMilliseconds;
                Console.WriteLine("Second Prediction took: " + elapsedMs2 + "mlSecs");

                // Get the highest score and its index
                var maxScore = prediction.Score.Max();

                ////////
                // Double-check using the index
                var maxIndex = prediction.Score.ToList().IndexOf(maxScore);
                VBuffer<ReadOnlyMemory<char>> keys = default;
                predictionEngine.OutputSchema[3].GetKeyValues(ref keys);
                var keysArray = keys.DenseValues().ToArray();
                var predictedLabelString = keysArray[maxIndex];
                ////////

                Console.WriteLine($"Image Filename : [{imageToPredict.ImageFileName}], " +
                                  $"Predicted Label : [{prediction.PredictedLabel}], " +
                                  $"Probability : [{maxScore}] "
                                  );

                //Predict all images in the folder
                //
                Console.WriteLine("");
                Console.WriteLine("Predicting several images...");

                foreach (var currentImageToPredict in imagesToPredict)
                {
                    var currentPrediction = predictionEngine.Predict(currentImageToPredict);

                    Console.WriteLine(
                        $"Image Filename : [{currentImageToPredict.ImageFileName}], " +
                        $"Predicted Label : [{currentPrediction.PredictedLabel}], " +
                        $"Probability : [{currentPrediction.Score.Max()}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to end the app..");
            Console.ReadKey();
        }

        public static string GetAbsolutePath(string relativePath)
            => FileUtils.GetAbsolutePath(typeof(Program).Assembly, relativePath);
    }
}
