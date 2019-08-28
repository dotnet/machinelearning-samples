using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageClassification.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ImageClassification.Predict
{
    class Program
    {
        static void Main(string[] args)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);
           
            string imagesForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions");

            var imageClassifierModelZipFilePath = Path.Combine(assetsPath, "inputs", "MLNETModel", "imageClassifier.zip");

            try
            {
                MLContext mlContext = new MLContext(seed: 1);

                Console.WriteLine($"Loading model from: {imageClassifierModelZipFilePath}");

                // Load the model
                ITransformer loadedModel = mlContext.Model.Load(imageClassifierModelZipFilePath, out var modelInputSchema);

                // Measuring Create Prediction Engine time
                var watchForCreatePredictionEngine = System.Diagnostics.Stopwatch.StartNew();

                // Create prediction engine to try a single prediction (input = ImageData, output = ImagePrediction)
                var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(loadedModel);

                watchForCreatePredictionEngine.Stop();
                long elapsedMsForCreatingPredictionEngine = watchForCreatePredictionEngine.ElapsedMilliseconds;
                Console.WriteLine("Creating PredEngine took: " + (elapsedMsForCreatingPredictionEngine).ToString() + " miliseconds");

                IEnumerable<ImageData> imagesToPredict = LoadImagesFromDirectory(imagesForPredictions, true);

                // Measuring PREDICTION execution time
                var watchForE2EPrediction = System.Diagnostics.Stopwatch.StartNew();

                // Obtain the original label names to map through the predicted label-index
                VBuffer<ReadOnlyMemory<char>> keys = default;
                predictionEngine.OutputSchema["LabelAsKey"].GetKeyValues(ref keys);
                var originalLabels = keys.DenseValues().ToArray();

                //Predict the first image in the folder
                //
                ImageData imageToPredict = new ImageData
                {
                    ImagePath = imagesToPredict.First().ImagePath
                };

                var prediction0 = predictionEngine.Predict(imageToPredict);

                // Measuring Predict() time
                var watchForPredictFunction = System.Diagnostics.Stopwatch.StartNew();

                var prediction = predictionEngine.Predict(imageToPredict);

                watchForPredictFunction.Stop();
                long elapsedMsForPredictFunction = watchForPredictFunction.ElapsedMilliseconds;
                Console.WriteLine("Only .Predict() took: " + (elapsedMsForPredictFunction).ToString() + " miliseconds");

                var index = prediction.PredictedLabel;

                Console.WriteLine($"ImageFile : [{Path.GetFileName(imageToPredict.ImagePath)}], " +
                                  $"Scores : [{string.Join(",", prediction.Score)}], " +
                                  $"Predicted Label : {originalLabels[index]}");

                watchForE2EPrediction.Stop();
                long elapsedMsForE2EPrediction = watchForE2EPrediction.ElapsedMilliseconds;
                Console.WriteLine("Prediction execution took: " + (elapsedMsForE2EPrediction).ToString() + " miliseconds");

                //////

                //Predict all images in the folder
                //
                Console.WriteLine("");
                Console.WriteLine("Predicting several images...");

                foreach (ImageData currentImageToPredict in imagesToPredict)
                {
                    var currentPrediction = predictionEngine.Predict(currentImageToPredict);
                    var currentIndex = currentPrediction.PredictedLabel;
                    Console.WriteLine($"ImageFile : [{Path.GetFileName(currentImageToPredict.ImagePath)}], " +
                                      $"Scores : [{string.Join(",", currentPrediction.Score)}], " +
                                      $"Predicted Label : {originalLabels[currentIndex]}");
                }
                //////
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to end the app..");
            Console.ReadKey();
        }

        public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameasLabel = true)
        {
            var files = Directory.GetFiles(folder, "*",
                searchOption: SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

                var label = Path.GetFileName(file);
                if (useFolderNameasLabel)
                    label = Directory.GetParent(file).Name;
                else
                {
                    for (int index = 0; index < label.Length; index++)
                    {
                        if (!char.IsLetter(label[index]))
                        {
                            label = label.Substring(0, index);
                            break;
                        }
                    }
                }

                yield return new ImageData()
                {
                    ImagePath = file,
                    Label = label
                };

            }
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
