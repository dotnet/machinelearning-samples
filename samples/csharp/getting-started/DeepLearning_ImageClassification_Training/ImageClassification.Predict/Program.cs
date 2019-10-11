using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageClassification.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;

using System.Linq;

namespace ImageClassification.Predict
{
    class Program
    {
        static void Main(string[] args)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);
           
            string imagesFolderPathForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions");

            var imageClassifierModelZipFilePath = Path.Combine(assetsPath, "inputs", "MLNETModel", "imageClassifier.zip");

            try
            {
                MLContext mlContext = new MLContext(seed: 1);

                Console.WriteLine($"Loading model from: {imageClassifierModelZipFilePath}");

                // Load the model
                ITransformer loadedModel = mlContext.Model.Load(imageClassifierModelZipFilePath, out var modelInputSchema);

                // Create prediction engine to try a single prediction (input = ImageData, output = ImagePrediction)
                var predictionEngine = mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(loadedModel);

                //Predict the first image in the folder
                IEnumerable<InMemoryImageData> imagesToPredict = LoadInMemoryImagesFromDirectory(
                                                                        imagesFolderPathForPredictions, false);



                InMemoryImageData imageToPredict = new InMemoryImageData
                {
                    Image = imagesToPredict.First().Image,
                    ImageFileName = imagesToPredict.First().ImageFileName
                };

                var prediction = predictionEngine.Predict(imageToPredict);

                // Get the highest score and its index
                float maxScore = prediction.Score.Max();

                ////////
                // Double-check using the index
                int maxIndex = prediction.Score.ToList().IndexOf(maxScore);
                VBuffer<ReadOnlyMemory<char>> keys = default;
                predictionEngine.OutputSchema[4].GetKeyValues(ref keys);
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

                foreach (InMemoryImageData currentImageToPredict in imagesToPredict)
                {
                    var currentPrediction = predictionEngine.Predict(currentImageToPredict);

                    Console.WriteLine($"Image Filename : [{currentImageToPredict.ImageFileName}], " +
                                      $"Predicted Label : [{currentPrediction.PredictedLabel}], " +
                                      $"Probability : [{currentPrediction.Score.Max()}] "
                                     );
                }

                //Console.WriteLine("*** Showing all the predictions ***");
                //// Find the original label names.
                //VBuffer<ReadOnlyMemory<char>> keys = default;
                //predictionsDataView.Schema["LabelAsKey"].GetKeyValues(ref keys);
                //var originalLabels = keys.DenseValues().ToArray();

                //List<ImagePredictionEx> predictions = mlContext.Data.CreateEnumerable<ImagePredictionEx>(predictionsDataView, false, true).ToList();
                //predictions.ForEach(pred => ConsoleWriteImagePrediction(pred.ImagePath, pred.Label, (originalLabels[pred.PredictedLabel]).ToString(), pred.Score.Max()));

                // OTHER CASE:
                // Find the original label names.
                //VBuffer<ReadOnlyMemory<char>> keys = default;
                //predictionEngine.OutputSchema["LabelAsKey"].GetKeyValues(ref keys);

                //var originalLabels = keys.DenseValues().ToArray();
                ////var index = prediction.PredictedLabel;

                //Console.WriteLine($"In-Memory Image provided, " +
                //                  $"Scores : [{string.Join(",", prediction.Score)}], " +
                //                  $"Predicted Label : {prediction.PredictedLabel}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to end the app..");
            Console.ReadKey();
        }

        //private int GetTopScoreIndex(float[] scores, int n)
        //{
        //    int i;
        //    float first;
        //    int index0 = 0;
        //    if (n < 3)
        //    {
        //        Console.WriteLine("Invalid Input");
        //        return 0;
        //    }
        //    first = 000;
        //    for (i = 0; i < n; i++)
        //    {
        //        // If current element is  
        //        // smaller than first 
        //        if (scores[i] > first)
        //        {
        //            first = scores[i];
        //        }
        //    }
        //    var scoresList = scores.ToList();
        //    scoresList.
        //    index0 = scoresList.IndexOf(first);

        //    return index0;
        //}

        public static IEnumerable<InMemoryImageData> LoadInMemoryImagesFromDirectory(string folder,
                                                                                     bool useFolderNameAsLabel = true)
        {
            var files = Directory.GetFiles(folder, "*",
                searchOption: SearchOption.AllDirectories);
            foreach (var file in files)
            {
                //if (Path.GetExtension(file) != ".jpg")
                //    continue;

                var label = Path.GetFileName(file);
                if (useFolderNameAsLabel)
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

                yield return new InMemoryImageData()
                {
                    Image = File.ReadAllBytes(file),
                    Label = label,
                    ImageFileName = Path.GetFileName(file)
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
