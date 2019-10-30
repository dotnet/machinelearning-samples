using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common;
using ImageClassification.DataModels;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using static Microsoft.ML.Transforms.ValueToKeyMappingEstimator;

namespace ImageClassification.Train
{
    internal class Program
    {
        static void Main()
        {
            const string assetsRelativePath = @"../../../assets";
            var assetsPath = GetAbsolutePath(assetsRelativePath);

            var outputMlNetModelFilePath = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");
            var imagesFolderPathForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions", "FlowersForPredictions");

            var imagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "images");

            // 1. Download the image set and unzip
            var finalImagesFolderName = DownloadImageSet(imagesDownloadFolderPath);
            var fullImagesetFolderPath = Path.Combine(imagesDownloadFolderPath, finalImagesFolderName);

            var mlContext = new MLContext(seed: 1);

            // 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
            var images = LoadImagesFromDirectory(folder: fullImagesetFolderPath, useFolderNameAsLabel: true);
            var fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
            var shuffledFullImageFilePathsDataset = mlContext.Data.ShuffleRows(fullImagesDataset);

            // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
            var shuffledFullImagesDataset = mlContext.Transforms.Conversion
                .MapValueToKey(
                    outputColumnName: "LabelAsKey",
                    inputColumnName: "Label",
                    keyOrdinality: KeyOrdinality.ByValue)
                .Append(
                    mlContext.Transforms.LoadImages(
                        outputColumnName: "Image",
                        imageFolder: fullImagesetFolderPath, 
                        useImageType: false,
                        inputColumnName: "ImagePath"))
                .Fit(shuffledFullImageFilePathsDataset)
                .Transform(shuffledFullImageFilePathsDataset);

            // 4. Split the data 80:20 into train and test sets, train and evaluate.
            var trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
            var trainDataView = trainTestData.TrainSet;
            var testDataView = trainTestData.TestSet;

            // 5. Define the model's training pipeline 
            var pipeline = mlContext.Model.ImageClassification(
                featuresColumnName:"Image",
                labelColumnName:"LabelAsKey",
                arch: ImageClassificationEstimator.Architecture.InceptionV3, // Just by changing/selecting InceptionV3 here instead of ResnetV2101 you can try a different architecture/pre-trained model. 
                epoch: 100,      //An epoch is one learning cycle where the learner sees the whole training data set.
                batchSize: 10,   // batchSize sets the number of images to feed the model at a time. It needs to divide the training set evenly or the remaining part won't be used for training.                              
                learningRate: 0.01f,
                metricsCallback: (metrics) => Console.WriteLine(metrics),
                validationSet: testDataView
                //disableEarlyStopping: true, //If true, it will run all the specified epochs. If false, when converging it'll stop training.
                //reuseTrainSetBottleneckCachedValues: false //Use cache. Use it for fastest training if there are no changes in the dataset
                )
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));

            // 6. Train/create the ML model
            Console.WriteLine("*** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

            // Measuring training time
            var watch = Stopwatch.StartNew();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Training with transfer learning took: {elapsedMs / 1000} seconds");

            // 7. Get the quality metrics (accuracy, etc.)
            ITransformer trainedModel = pipeline.Fit(trainDataView);
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
            var watch = Stopwatch.StartNew();

            var predictionsDataView = trainedModel.Transform(testDataset);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName:"LabelAsKey", predictedLabelColumnName: "PredictedLabel");
            ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

            watch.Stop();
            var elapsed2Ms = watch.ElapsedMilliseconds;

            Console.WriteLine($"Predicting and Evaluation took: {elapsed2Ms / 1000} seconds");
        }

        private static void TrySinglePrediction(string imagesFolderPathForPredictions, MLContext mlContext, ITransformer trainedModel)
        {
            // Create prediction function to try one prediction
            var predictionEngine = mlContext.Model
                .CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

            var testImages = FileUtils.LoadInMemoryImagesFromDirectory(
                imagesFolderPathForPredictions, false);

            var imageToPredict = testImages.First();

            var prediction = predictionEngine.Predict(imageToPredict);

            Console.WriteLine(
                $"Image Filename : [{imageToPredict.ImageFileName}], " +
                $"Scores : [{string.Join(",", prediction.Score)}], " +
                $"Predicted Label : {prediction.PredictedLabel}");
        }


        public static IEnumerable<ImageData> LoadImagesFromDirectory(
            string folder,
            bool useFolderNameAsLabel = true)
            => FileUtils.LoadImagesFromDirectory(folder, useFolderNameAsLabel)
                .Select(x => new ImageData(x.imagePath, x.label));

        public static string DownloadImageSet(string imagesDownloadFolder)
        {
            // get a set of images to teach the network about the new classes

            //SINGLE SMALL FLOWERS IMAGESET (200 files)
            const string fileName = "flower_photos_small_set.zip";
            var url = $"https://mlnetfilestorage.file.core.windows.net/imagesets/flower_images/flower_photos_small_set.zip?st=2019-08-07T21%3A27%3A44Z&se=2030-08-08T21%3A27%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=SZ0UBX47pXD0F1rmrOM%2BfcwbPVob8hlgFtIlN89micM%3D";
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
            => FileUtils.GetAbsolutePath(typeof(Program).Assembly, relativePath);

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

