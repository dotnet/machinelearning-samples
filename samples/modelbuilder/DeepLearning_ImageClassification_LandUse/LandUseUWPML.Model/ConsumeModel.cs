// This file was auto-generated by ML.NET Model Builder. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ML;
using LandUseUWPML.Model;

namespace LandUseUWPML.Model
{
    public class ConsumeModel
    {
        // For more info on consuming ML.NET models, visit https://aka.ms/model-builder-consume
        // Method for consuming model in your app
        public static ModelOutput Predict(ModelInput input)
        {

            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Register NormalizeMapping
            mlContext.ComponentCatalog.RegisterAssembly(typeof(NormalizeMapping).Assembly);

            // Register LabelMapping
            mlContext.ComponentCatalog.RegisterAssembly(typeof(LabelMapping).Assembly);

            // Load model & create prediction engine
            string modelPath = @"C:\Users\luquinta.REDMOND\AppData\Local\Temp\MLVSTools\LandUseUWPML\LandUseUWPML.Model\MLModel.zip";
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            // Use model to make prediction on input data
            ModelOutput result = predEngine.Predict(input);
            return result;
        }
    }
}
