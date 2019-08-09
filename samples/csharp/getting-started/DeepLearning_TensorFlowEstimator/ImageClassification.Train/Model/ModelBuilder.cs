using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using ImageClassification.DataModels;
using static ImageClassification.Model.ConsoleHelpers;
using Common;
using System.Collections;
using System.Collections.Generic;

namespace ImageClassification.Model
{
    public class ModelBuilder
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputTensorFlowModelFilePath;
        private readonly string outputMlNetModelFilePath;
        private readonly MLContext mlContext;
        private static string LabelAsKey = nameof(LabelAsKey);
        private static string ImageReal = nameof(ImageReal);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);

        public ModelBuilder(string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.inputTensorFlowModelFilePath = inputModelLocation;
            this.outputMlNetModelFilePath = outputModelLocation;
            mlContext = new MLContext(seed: 1);
        }

        private struct ImageSettingsForTFModel
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        public void BuildAndTrain()
        {
            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {inputTensorFlowModelFilePath}");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {dataLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageSettingsForTFModel.imageWidth},{ImageSettingsForTFModel.imageHeight}), image mean: {ImageSettingsForTFModel.mean}");

            // 1. Load images information (filenames and labels) in IDataView
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<ImageData>(path:dataLocation, hasHeader: false);

            // 2. Load images in-memory while applying image transformations 
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelAsKey, inputColumnName: "Label")
                            .Append(mlContext.Transforms.LoadImages(outputColumnName: "image_object", imageFolder: imagesFolder, inputColumnName: nameof(DataModels.ImageData.ImageFileName)))
                            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "image_object_resized", imageWidth: ImageSettingsForTFModel.imageWidth, imageHeight: ImageSettingsForTFModel.imageHeight, inputColumnName: "image_object"))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", inputColumnName: "image_object_resized", interleavePixelColors: ImageSettingsForTFModel.channelsLast, offsetImage: ImageSettingsForTFModel.mean))
                            .Append(mlContext.Model.LoadTensorFlowModel(inputTensorFlowModelFilePath).
                                 ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true));
                                // Input and output column names have to coincide with the input and output tensor names of the TensorFlow model
                                // You can check out those tensor names by opening the Tensorflow .pb model with a visual tool like Netron: https://github.com/lutzroeder/netron
                                // TF .pb model --> Softmax node --> INPUTS --> input --> id: "input" 
                                // TF .pb model --> input node --> INPUTS --> logits --> id: "softmax2_pre_activation" 

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 2);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "softmax2_pre_activation", trainingDataView, dataProcessPipeline, 2);

            // 3. Set the training algorithm and convert back the key to the categorical values                            
            var trainer = mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelAsKey, featureColumnName: "softmax2_pre_activation");
            var trainingPipeline = dataProcessPipeline.Append(trainer)
                                                      .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"));                                                      

            // 4. Train the model
            ConsoleWriteHeader("Training the classification model");
            ITransformer model = trainingPipeline.Fit(trainingDataView);

            // 5. Make bulk predictions and calculate quality metrics
            ConsoleWriteHeader("Create Predictions and Evaluate the model quality");
            //TO DO: 
            // Accuracy is currently 1 because we're testing/evaluating with the same image-set used for training...
            // Use a larger image-set and seggregate a training set and a test set so test images have not been used for training
            IDataView predictionsDataView = model.Transform(trainingDataView);

            // This is an optional step, but it's useful for debugging issues
            var loadedModelOutputColumnNames = predictionsDataView.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);

            // 5.1 Show the predictions
            List<ImageWithPipelineFeatures> predictions = mlContext.Data.CreateEnumerable<ImageWithPipelineFeatures>(predictionsDataView, false, true).ToList();
            predictions.ForEach(pred => ConsoleWriteImagePrediction(pred.ImageFileName, pred.PredictedLabelValue, pred.Score.Max()));

            // 5.2 Show the performance metrics for the multi-class classification            
            var classificationContext = mlContext.MulticlassClassification;
            ConsoleWriteHeader("Classification metrics");
            var metrics = classificationContext.Evaluate(predictionsDataView, labelColumnName: LabelAsKey, predictedLabelColumnName: "PredictedLabel");
            ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

            // 6. Save the model to assets/outputs
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputMlNetModelFilePath);

            mlContext.Model.Save(model, predictionsDataView.Schema, outputMlNetModelFilePath);
            Console.WriteLine($"Model saved: {outputMlNetModelFilePath}");
        }

    }
}
