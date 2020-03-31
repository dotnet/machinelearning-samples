// This file was auto-generated by ML.NET Model Builder. 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using LandUseUWPML.Model;
namespace LandUseUWPML.ConsoleApp
{
    public static class ModelBuilder
    {
        private static string TRAIN_DATA_FILEPATH = @"C:\Users\luquinta.REDMOND\AppData\Local\Temp\f437b2ed-761a-4835-8303-879f7c75b403.tsv";
        private static string MLNET_MODEL = @"C:\Users\luquinta.REDMOND\AppData\Local\Temp\MLVSTools\LandUseUWPML\LandUseUWPML.Model\MLModel.zip";
        private static string ONNX_MODEL = @"C:\Users\luquinta.REDMOND\AppData\Local\Temp\MLVSTools\LandUseUWPML\LandUseUWPML.Model\bestModel.onnx";

        // Create MLContext to be shared across the model creation workflow objects 
        // Set a random seed for repeatable/deterministic results across multiple trainings.
        private static MLContext mlContext = new MLContext(seed: 1);

        // Training in Azure produces an ONNX model; this method demonstrates creating an ML.NET model (MLModel.zip) from the ONNX model (bestModel.onnx), which you can then use with the ConsumeModel() method to make predictions
        public static void CreateMLNetModelFromOnnx()
        {
            // Load data
            //IDataView inputDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
            //                                path: TRAIN_DATA_FILEPATH,
            //                                hasHeader: true,
            //                                separatorChar: '\t',
            //                                allowQuoting: true,
            //                                allowSparse: false);

            IDataView inputDataView = mlContext.Data.LoadFromEnumerable<ModelInput>(new ModelInput[] { });

            // Create an ML.NET pipeline to score using the ONNX model
            // Notice that this pipeline is not trainable because it only contains transformers
            IEstimator<ITransformer> pipeline = BuildPipeline(mlContext);

            // Create ML.NET model from pipeline
            ITransformer mlModel = pipeline.Fit(inputDataView);

            // Save model
            SaveModel(mlContext, mlModel, MLNET_MODEL, inputDataView.Schema);
        }

        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations to:
            // 1. Score using provided onnx model
            // 2. Map scores to labels to make model output easier to understand and use
            var pipeline = mlContext.Transforms.LoadImages("ImageSource_featurized", null, "ImageSource")
                                      .Append(mlContext.Transforms.ResizeImages("ImageSource_featurized", 224, 224, "ImageSource_featurized"))
                                      .Append(mlContext.Transforms.ExtractPixels("ImageSource_featurized", "ImageSource_featurized"))
                                      .Append(mlContext.Transforms.CustomMapping<NormalizeInput, NormalizeOutput>(
                                          (input, output) => NormalizeMapping.Mapping(input, output),
                                          contractName: nameof(NormalizeMapping)))
                                      .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: ONNX_MODEL))
                                      .Append(mlContext.Transforms.CustomMapping<LabelMappingInput, LabelMappingOutput>(
                                          (input, output) => LabelMapping.Mapping(input, output),
                                          contractName: nameof(LabelMapping)));
            return pipeline;
        }

        private static void SaveModel(MLContext mlContext, ITransformer mlModel, string modelRelativePath, DataViewSchema modelInputSchema)
        {
            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"=============== Saving the model  ===============");
            mlContext.Model.Save(mlModel, modelInputSchema, GetAbsolutePath(modelRelativePath));
            Console.WriteLine("The model is saved to {0}", GetAbsolutePath(modelRelativePath));
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
