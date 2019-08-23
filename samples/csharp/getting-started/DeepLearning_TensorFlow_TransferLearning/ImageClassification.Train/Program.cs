using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageClassification.DataModels;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using static Microsoft.ML.DataOperationsCatalog;
using System.Linq;
using Microsoft.ML.Data;
using Common;

namespace ImageClassification.Train
{
    public class Program
    {
        static void Main(string[] args)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            var outputMlNetModelFilePath = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            string imagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "images");

            //Download the image set and unzip
            string finalImagesFolderName = DownloadImageSet(imagesDownloadFolderPath);
            string fullImagesetFolderPath = Path.Combine(imagesDownloadFolderPath, finalImagesFolderName);
            string trainImagesetFolderPath = Path.Combine(fullImagesetFolderPath, "train-dataset");
            string testImagesetFolderPath = Path.Combine(fullImagesetFolderPath, "test-dataset");

            string imagesForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions", "FlowersForPredictions");

            try
            {
                MLContext mlContext = new MLContext(seed: 1);

                //Load single full image-set
                //
                //IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: fullImagesetFolderPath, useFolderNameasLabel: true);
                //IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
                //
                //IDataView shuffledFullImagesDataset = mlContext.Data.ShuffleRows(fullImagesDataset);
                //
                // Split the data 80:20 into train and test sets, train and evaluate.
                //TrainTestData trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
                //IDataView trainDataView = trainTestData.TrainSet;
                //IDataView testDataView = trainTestData.TestSet;
                //

                //Load seggregated train-image-set 
                //
                IEnumerable<ImageData> trainImages = LoadImagesFromDirectory(folder: trainImagesetFolderPath, useFolderNameasLabel: true);
                IDataView trainDataView = mlContext.Data.LoadFromEnumerable(trainImages);

                //Load seggregated test-image-set 
                IEnumerable<ImageData> testImages = LoadImagesFromDirectory(folder: testImagesetFolderPath, useFolderNameasLabel: true);
                IDataView testDataView = mlContext.Data.LoadFromEnumerable(testImages);


                var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName:"LabelAsKey", inputColumnName:"Label")
                    .Append(mlContext.Transforms.LoadImages("ImageObject", null, "ImagePath"))
                    .Append(mlContext.Transforms.ResizeImages("Image",
                        inputColumnName: "ImageObject", imageWidth: 299,
                        imageHeight: 299))
                    .Append(mlContext.Transforms.ExtractPixels("Image",
                        interleavePixelColors: true))
                    .Append(mlContext.Model.ImageClassification("Image", "LabelAsKey",
                            arch: DnnEstimator.Architecture.InceptionV3,
                            epoch: 1200,               //An epoch is one learning cycle where the learner sees the whole training data set.
                            batchSize: 100,          // batchSize sets then number of images to feed the model at a time
                            learningRate: 0.000001f, //Good for hundreds of images: 0.000001f
                            statisticsCallback: (epoch, accuracy, crossEntropy) => Console.WriteLine(
                                                                                        $"Training-cycle: {epoch}, " +
                                                                                        $"Accuracy: {accuracy * 100}%, " +
                                                                                        $"Cross-Entropy: {crossEntropy}")));

                Console.WriteLine("*** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

                // Measuring training time
                var watch = System.Diagnostics.Stopwatch.StartNew();

                ITransformer trainedModel = pipeline.Fit(trainDataView);

                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Training with transfer learning took: " + (elapsedMs / 1000).ToString() + " seconds");

                // Get the metrics
                EvaluateModel(mlContext, testDataView, trainedModel);

                TrySinglePrediction(imagesForPredictions, mlContext, trainedModel);

                // Save the model to assets/outputs
                mlContext.Model.Save(trainedModel, trainDataView.Schema, outputMlNetModelFilePath);
                Console.WriteLine($"Model saved to: {outputMlNetModelFilePath}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to finish");
            Console.ReadKey();
        }

        private static void TrySinglePrediction(string imagesForPredictions, MLContext mlContext, ITransformer trainedModel)
        {
            // Create prediction function to try one prediction
            var predictionEngine = mlContext.Model
                .CreatePredictionEngine<ImageData, ImagePrediction>(trainedModel);

            IEnumerable<ImageData> testImages = LoadImagesFromDirectory(imagesForPredictions, true);
            ImageData imageToPredict = new ImageData
            {
                ImagePath = testImages.First().ImagePath
            };

            var prediction = predictionEngine.Predict(imageToPredict);

            // Find the original label names.
            VBuffer<ReadOnlyMemory<char>> keys = default;
            predictionEngine.OutputSchema["LabelAsKey"].GetKeyValues(ref keys);

            var originalLabels = keys.DenseValues().ToArray();
            var index = prediction.PredictedLabel;

            Console.WriteLine($"ImageFile : [{Path.GetFileName(imageToPredict.ImagePath)}], " +
                              $"Scores : [{string.Join(",", prediction.Score)}], " +
                              $"Predicted Label : {originalLabels[index]}");
        }

        
        private static void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            Console.WriteLine("Making bulk predictions and evaluating model's quality...");

            // Measuring time
            var watch2 = System.Diagnostics.Stopwatch.StartNew();

            IDataView predictionsDataView = trainedModel.Transform(testDataset);
            //var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

            // This is an optional step, but it's useful for debugging issues
            var loadedModelOutputColumnNames = predictionsDataView.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName:"LabelAsKey", predictedLabelColumnName: "PredictedLabel");
            ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

            Console.WriteLine($"Micro-accuracy: {metrics.MicroAccuracy}," +
                              $"macro-accuracy = {metrics.MacroAccuracy}");

            watch2.Stop();
            long elapsed2Ms = watch2.ElapsedMilliseconds;

            Console.WriteLine("Predicting and Evaluation took: " + (elapsed2Ms / 1000).ToString() + " seconds");

            Console.WriteLine("*** Showing all the predictions ***");

            // Find the original label names.
            VBuffer<ReadOnlyMemory<char>> keys = default;
            predictionsDataView.Schema["LabelAsKey"].GetKeyValues(ref keys);
            var originalLabels = keys.DenseValues().ToArray();

            List<ImageWithPipelineFeatures> predictions = mlContext.Data.CreateEnumerable<ImageWithPipelineFeatures>(predictionsDataView, false, true).ToList();
            predictions.ForEach(pred => ConsoleWriteImagePrediction(pred.ImagePath, pred.Label, (originalLabels[pred.PredictedLabel]).ToString(), pred.Score.Max()));

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

        public static string DownloadImageSet(string imagesDownloadFolder)
        {
            // get a set of images to teach the network about the new classes

            //SINGLE FULL FLOWERS IMAGESET (3,600 files)
            //string fileName = "flower_photos.tgz";
            //string url = $"http://download.tensorflow.org/example_images/{fileName}";
            //Web.Download(url, imagesDownloadFolder, fileName);
            //Compress.ExtractTGZ(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

            //SINGLE SMALL FLOWERS IMAGESET (200 files)
            //string fileName = "flower_photos_small_set.zip";
            //string url = $"https://mlnetfilestorage.file.core.windows.net/imagesets/flower_images/flower_photos_small_set.zip?st=2019-08-07T21%3A27%3A44Z&se=2030-08-08T21%3A27%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=SZ0UBX47pXD0F1rmrOM%2BfcwbPVob8hlgFtIlN89micM%3D";
            //Web.Download(url, imagesDownloadFolder, fileName);
            //Compress.UnZip(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

            //SPLIT TRAIN/TEST DATASETS (FROM SMALL IMAGESET - 200 files)
            string fileName = "flower_photos_small_set_split.zip";
            string url = $"https://mlnetfilestorage.file.core.windows.net/imagesets/flower_images/flower_photos_small_set_split.zip?st=2019-08-23T00%3A03%3A25Z&se=2030-08-24T00%3A03%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=qROCaSGod0mCDP87xDmGCli3o8XyKUlUUimRGGVG9RE%3D";
            Web.Download(url, imagesDownloadFolder, fileName);
            Compress.UnZip(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

            return Path.GetFileNameWithoutExtension(fileName);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public static void ConsoleWriteImagePrediction(string ImagePath, string Label, string PredictedLabel, float Probability)
        {
            var defaultForeground = Console.ForegroundColor;
            var labelColor = ConsoleColor.Magenta;
            var probColor = ConsoleColor.Blue;

            Console.Write("Image File: ");
            Console.ForegroundColor = labelColor;
            Console.Write($"{Path.GetFileName(ImagePath)}");
            Console.ForegroundColor = defaultForeground;
            Console.Write(" original labeled as ");
            Console.ForegroundColor = labelColor;
            Console.Write(Label);
            Console.ForegroundColor = defaultForeground;
            Console.Write(" predicted as ");
            Console.ForegroundColor = labelColor;
            Console.Write(PredictedLabel);
            Console.ForegroundColor = defaultForeground;
            Console.Write(" with score ");
            Console.ForegroundColor = probColor;
            Console.Write(Probability);
            Console.ForegroundColor = defaultForeground;
            Console.WriteLine("");
        }

    }
}


// IDataView shuffledFullImagesDataset = mlContext.Data.ShuffleRows(fullImagesDataset);

//IDataView shuffledFullImagesDataset = mlContext.Data.ShuffleRows(fullImagesDataset);
//shuffledFullImagesDataset = mlContext.Transforms.Conversion.MapValueToKey("Label")
//    .Fit(shuffledFullImagesDataset)
//    .Transform(shuffledFullImagesDataset);

//fullImagesDataset = mlContext.Transforms.Conversion.MapValueToKey("Label")
//    .Fit(fullImagesDataset)
//    .Transform(fullImagesDataset);


//.Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName:"PredictedLabelValue", 
//                                                      inputColumnName:"PredictedLabel"));