using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BikeSharingDemand.BikeSharingDemandData;
using BikeSharingDemand.Helpers;
using BikeSharingDemand.Model;

using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.FastTree;
using Microsoft.ML.Runtime.Learners;

namespace BikeSharingDemand
{
    internal static class Program
    {
        private static string TrainingDataLocation = @"Data/hour_train.csv";
        private static string TestDataLocation = @"Data/hour_test.csv";

        static void Main(string[] args)
        {
            // 1. Common data transformations in the pipeline
            var pipelineTransforms = new PipelineTransforms(TrainingDataLocation);
            var dataView = pipelineTransforms.CreateDataView();
            var pipeline = pipelineTransforms.TransformDataInPipeline(dataView);

            // 2. Build/train, evaluate and test with Fast Tree regressor algorithm
            var fastTreeModelBuilder = new ModelBuilder<RegressionPredictionTransformer<FastTreeRegressionPredictor>>(TrainingDataLocation);
            var fastTreeModel = fastTreeModelBuilder.BuildAndTrainWithFastTreeRegressionTrainer(pipeline, dataView);
            fastTreeModelBuilder.TestSinglePrediction(fastTreeModel);
            var fastTreeModelEvaluator = new ModelEvaluator<RegressionPredictionTransformer<FastTreeRegressionPredictor>>();
            var fastTreeModelMetrics = fastTreeModelEvaluator.Evaluate(TestDataLocation, fastTreeModel);
            fastTreeModelEvaluator.PrintRegressionMetrics("Fast Tree regression model", fastTreeModelMetrics);

            // 3. Build/train, evaluate and test with SDCA regressor algorithm
            var sdcaModelBuilder = new ModelBuilder<RegressionPredictionTransformer<LinearRegressionPredictor>>(TrainingDataLocation);
            var sdcaModel = sdcaModelBuilder.BuildAndTrainWithSdcaRegressionTrainer(pipeline, dataView);
            sdcaModelBuilder.TestSinglePrediction(sdcaModel);
            var sdcaModelEvaluator = new ModelEvaluator<RegressionPredictionTransformer<LinearRegressionPredictor>>();
            var sdcaModelMetrics = sdcaModelEvaluator.Evaluate(TestDataLocation, sdcaModel);
            sdcaModelEvaluator.PrintRegressionMetrics("SDCA regression model", sdcaModelMetrics);

            // 4. Build/train, evaluate and test with Poisson regressor algorithm
            var poissonModelBuilder = new ModelBuilder<RegressionPredictionTransformer<PoissonRegressionPredictor>>(TrainingDataLocation);
            var poissonModel = poissonModelBuilder.BuildAndTrainWithPoissonRegressionTrainer(pipeline, dataView);
            poissonModelBuilder.TestSinglePrediction(poissonModel);
            var poissonModelEvaluator = new ModelEvaluator<RegressionPredictionTransformer<PoissonRegressionPredictor>>();
            var poissonModelMetrics = poissonModelEvaluator.Evaluate(TestDataLocation, poissonModel);
            poissonModelEvaluator.PrintRegressionMetrics("Poisson regression model", poissonModelMetrics);

            //Other possible Learners to implement and compare
            //...FastForestRegressor...
            //...OnlineGradientDescentRegressor...
            //...FastTreeTweedieRegressor...
            //...GeneralizedAdditiveModelRegressor...

            // 4. Visualize some predictions compared to observations from the test dataset

            var fastTreeTester = new ModelTester<RegressionPredictionTransformer<FastTreeRegressionPredictor>>();
            fastTreeTester.VisualizeSomePredictions("Fast Tree regression model", TestDataLocation, fastTreeModel, 10);

            var sdcaTester = new ModelTester<RegressionPredictionTransformer<LinearRegressionPredictor>>();
            sdcaTester.VisualizeSomePredictions("SDCA regression model", TestDataLocation, sdcaModel, 10);

            var poissonTester = new ModelTester<RegressionPredictionTransformer<PoissonRegressionPredictor>>();
            poissonTester.VisualizeSomePredictions("Poisson regression model", TestDataLocation, poissonModel, 10);

            // 5. Just saving as .ZIP file the model based on Fast Tree which is the one with better accuracy and tests
            fastTreeModelBuilder.SaveModelAsFile(fastTreeModel, @".\FastTreeModel.zip");

            Console.ReadLine();
        }
    }
}
