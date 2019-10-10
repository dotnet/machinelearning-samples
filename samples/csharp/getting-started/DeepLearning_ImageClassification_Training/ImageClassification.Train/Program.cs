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
            string imagesFolderPathForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions", "FlowersForPredictions");

            string imagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "images");

            // 1. Download the image set and unzip
            string finalImagesFolderName = DownloadImageSet(imagesDownloadFolderPath);
            string fullImagesetFolderPath = Path.Combine(imagesDownloadFolderPath, finalImagesFolderName);

            MLContext mlContext = new MLContext(seed: 1);

            // 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
            IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: fullImagesetFolderPath, useFolderNameasLabel: true);
            IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = mlContext.Data.ShuffleRows(fullImagesDataset);

            // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
            IDataView shuffledFullImagesDataset = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelAsKey",
                                                                                                inputColumnName: "Label",
                                                                                                keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
                                        .Append(mlContext.Transforms.LoadImages(outputColumnName: "Image",
                                                                                imageFolder: fullImagesetFolderPath, 
                                                                                useImageType: false,
                                                                                inputColumnName: "ImagePath"))
                                        .Fit(shuffledFullImageFilePathsDataset)
                                        .Transform(shuffledFullImageFilePathsDataset);

            // 4. Split the data 80:20 into train and test sets, train and evaluate.
            TrainTestData trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
            IDataView trainDataView = trainTestData.TrainSet;
            IDataView testDataView = trainTestData.TestSet;

            // 5. Define the model's training pipeline 
            var pipeline = mlContext.Model.ImageClassification(featuresColumnName:"Image", labelColumnName:"LabelAsKey",
                                        arch: ImageClassificationEstimator.Architecture.InceptionV3, // Just by changing/selecting InceptionV3 here instead of ResnetV2101 you can try a different architecture/pre-trained model. 
                                        epoch: 100,      //An epoch is one learning cycle where the learner sees the whole training data set.
                                        batchSize: 10,   // batchSize sets the number of images to feed the model at a time. It needs to divide the training set evenly or the remaining part won't be used for training.                              
                                        learningRate: 0.01f,
                                        metricsCallback: (metrics) => Console.WriteLine(metrics),
                                        validationSet: testDataView
                                        //disableEarlyStopping: true, //If true, it will run all the specified epochs. If false, when converging it'll stop training.
                                        //reuseTrainSetBottleneckCachedValues: false
                                        )
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));

            // 6. Train/create the ML model
            Console.WriteLine("*** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

            // Measuring training time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            ITransformer trainedModel = pipeline.Fit(trainDataView);

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine("Training with transfer learning took: " +
                (elapsedMs / 1000).ToString() + " seconds");

            // 7. Get the quality metrics (accuracy, etc.)
            EvaluateModel(mlContext, testDataView, trainedModel);

            // 8. Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
            mlContext.Model.Save(trainedModel, trainDataView.Schema, outputMlNetModelFilePath);
            Console.WriteLine($"Model saved to: {outputMlNetModelFilePath}");

            // 9. Try a single prediction simulating an end-user app
            TrySinglePrediction(imagesFolderPathForPredictions, mlContext, trainedModel); 

            Console.WriteLine("Press any key to finish");
            Console.ReadKey();
        }
       
        private static void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            Console.WriteLine("Making predictions in bulk for evaluating model's quality...");

            // Measuring time
            var watch2 = System.Diagnostics.Stopwatch.StartNew();

            IDataView predictionsDataView = trainedModel.Transform(testDataset);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName:"LabelAsKey", predictedLabelColumnName: "PredictedLabel");
            ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

            watch2.Stop();
            long elapsed2Ms = watch2.ElapsedMilliseconds;

            Console.WriteLine("Predicting and Evaluation took: " +
                (elapsed2Ms / 1000).ToString() + " seconds");
        }

        private static void TrySinglePrediction(string imagesFolderPathForPredictions, MLContext mlContext, ITransformer trainedModel)
        {
            // Create prediction function to try one prediction
            var predictionEngine = mlContext.Model
                .CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

            IEnumerable<InMemoryImageData> testImages = LoadInMemoryImagesFromDirectory(
                imagesFolderPathForPredictions, false);

            InMemoryImageData imageToPredict = new InMemoryImageData
            {
                Image = testImages.First().Image,
                ImageFileName = testImages.First().ImageFileName
            };

            var prediction = predictionEngine.Predict(imageToPredict);

            Console.WriteLine($"Image Filename : [{imageToPredict.ImageFileName}], " +
                              $"Scores : [{string.Join(",", prediction.Score)}], " +
                              $"Predicted Label : {prediction.PredictedLabel}");
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

        public static IEnumerable<InMemoryImageData> LoadInMemoryImagesFromDirectory(string folder,
                                                                                     bool useFolderNameAsLabel = true)
        {
            var files = Directory.GetFiles(folder, "*",
                searchOption: SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

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

        public static string DownloadImageSet(string imagesDownloadFolder)
        {
            // get a set of images to teach the network about the new classes

            //SINGLE SMALL FLOWERS IMAGESET (200 files)
            string fileName = "flower_photos_small_set.zip";
            string url = $"https://mlnetfilestorage.file.core.windows.net/imagesets/flower_images/flower_photos_small_set.zip?st=2019-08-07T21%3A27%3A44Z&se=2030-08-08T21%3A27%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=SZ0UBX47pXD0F1rmrOM%2BfcwbPVob8hlgFtIlN89micM%3D";
            Web.Download(url, imagesDownloadFolder, fileName);
            Compress.UnZip(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

            //SINGLE FULL FLOWERS IMAGESET (3,600 files)
            //string fileName = "flower_photos.tgz";
            //string url = $"http://download.tensorflow.org/example_images/{fileName}";
            //Web.Download(url, imagesDownloadFolder, fileName);
            //Compress.ExtractTGZ(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

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

