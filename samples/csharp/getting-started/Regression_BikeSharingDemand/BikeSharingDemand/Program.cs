using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BikeSharingDemand.BikeSharingDemandData;
using BikeSharingDemand.Helpers;
using BikeSharingDemand.Model;

using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;

namespace BikeSharingDemand
{
    internal static class Program
    {
        private static string TrainingDataLocation = @"Data/hour_train.csv";
        private static string TestDataLocation = @"Data/hour_test.csv";

        static void Main(string[] args)
        {
            var pipelineTransforms = new PipelineTransforms(TrainingDataLocation);
            var dataView = pipelineTransforms.CreateDataView();
            var pipeline = pipelineTransforms.TransformDataInPipeline(dataView);

            var sdcaModelBuilder = new ModelBuilder<RegressionPredictionTransformer<LinearRegressionPredictor>>(TrainingDataLocation);
            var sdcaModel = sdcaModelBuilder.BuildAndTrainWithSdcaRegressionTrainer(pipeline, dataView);
            sdcaModelBuilder.TestSinglePrediction(sdcaModel);
            var sdcaModelEvaluator = new ModelEvaluator<RegressionPredictionTransformer<LinearRegressionPredictor>>();
            var sdcaModelMetrics = sdcaModelEvaluator.Evaluate(TestDataLocation, sdcaModel);
            sdcaModelEvaluator.PrintRegressionMetrics("SDCA regression model", sdcaModelMetrics);

            var poissonModelBuilder = new ModelBuilder<RegressionPredictionTransformer<PoissonRegressionPredictor>>(TrainingDataLocation);
            var poissonModel = poissonModelBuilder.BuildAndTrainWithPoissonRegressionTrainer(pipeline, dataView);
            poissonModelBuilder.TestSinglePrediction(poissonModel);
            var poissonModelEvaluator = new ModelEvaluator<RegressionPredictionTransformer<PoissonRegressionPredictor>>();
            var poissonModelMetrics = poissonModelEvaluator.Evaluate(TestDataLocation, poissonModel);
            poissonModelEvaluator.PrintRegressionMetrics("Poisson regression model", poissonModelMetrics);

            //Other possible Learners to implement and compare
            //...FastTreeRegressor...
            //...FastForestRegressor...
            //...OnlineGradientDescentRegressor...
            //...FastTreeTweedieRegressor...
            //...GeneralizedAdditiveModelRegressor...

            //Visualize some predictions compared to observations from the test dataset
            var sdcaTester = new ModelTester<RegressionPredictionTransformer<LinearRegressionPredictor>>();
            sdcaTester.VisualizeSomePredictions("SDCA regression model", TestDataLocation, sdcaModel, 10);

            var poissonTester = new ModelTester<RegressionPredictionTransformer<PoissonRegressionPredictor>>();
            poissonTester.VisualizeSomePredictions("Poisson regression model", TestDataLocation, poissonModel, 10);

            //Save models as .ZIP files
            sdcaModelBuilder.SaveModelAsFile(sdcaModel, @".\sdcaModel.zip");
            poissonModelBuilder.SaveModelAsFile(poissonModel, @".\poissonModel.zip");

            Console.ReadLine();
        }
    }
}
