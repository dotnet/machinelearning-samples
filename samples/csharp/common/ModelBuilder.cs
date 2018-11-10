using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

namespace Common
{
    public class ModelBuilder<TObservation, TPrediction>
                                    where TObservation : class
                                    where TPrediction : class, new()
    {
        private MLContext _mlcontext;
        public IEstimator<ITransformer> TrainingPipeline { get; private set; }
        public ITransformer TrainedModel { get; private set; }

        public ModelBuilder(
            MLContext mlContext,
            IEstimator<ITransformer> dataProcessPipeline //, IEstimator<ITransformer> trainer
                           )
        {
            _mlcontext = mlContext;
            TrainingPipeline = dataProcessPipeline;
        }

        public void AddTrainer(IEstimator<ITransformer> trainer)
        {
            this.AddEstimator(trainer);
        }

        public void AddEstimator(IEstimator<ITransformer> estimator)
        {
            TrainingPipeline = TrainingPipeline.Append(estimator);
        }

        public ITransformer Train(IDataView trainingData)
        {
            TrainedModel = TrainingPipeline.Fit(trainingData);
            return TrainedModel;
        }

        public RegressionEvaluator.Result EvaluateRegressionModel(IDataView testData, string label, string score)
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(testData);
            var metrics = _mlcontext.Regression.Evaluate(predictions, label: label, score: score);
            return metrics;
        }

        public BinaryClassifierEvaluator.Result EvaluateBinaryClassificationModel(IDataView testData, string label, string score)
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(testData);
            var metrics = _mlcontext.BinaryClassification.Evaluate(predictions, label:label, score:score);
            return metrics;
        }

        public MultiClassClassifierEvaluator.Result EvaluateMultiClassClassificationModel(IDataView testData, string label="Label", string score="Score")
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(testData);
            var metrics = _mlcontext.MulticlassClassification.Evaluate(predictions, label: label, score: score);
            return metrics;
        }

        public (MultiClassClassifierEvaluator.Result metrics, 
                ITransformer model, 
                IDataView scoredTestData)[]
            CrossValidateAndEvaluateMulticlassClassificationModel(IDataView data, int numFolds = 5, string labelColumn = "Label", string stratificationColumn = null)
        {
            //CrossValidation happens actually before training, so no check here.

            //Cross validate
            var crossValidationResults = _mlcontext.MulticlassClassification.CrossValidate(data, TrainingPipeline, numFolds, labelColumn, stratificationColumn);

            //Another way to do it:
            //var context = new MulticlassClassificationContext(_mlcontext);
            //var crossValidationResults = context.CrossValidate(data, TrainingPipeline, numFolds, labelColumn, stratificationColumn);

            return crossValidationResults;
        }

        public ClusteringEvaluator.Result EvaluateClusteringModel(IDataView dataView)
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(dataView);

            var metrics = _mlcontext.Clustering.Evaluate(predictions, score:"Score", features: "Features");
            return metrics;
        }

        public void SaveModelAsFile(string persistedModelPath)
        {
            CheckTrained();

            using (var fs = new FileStream(persistedModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                _mlcontext.Model.Save(TrainedModel, fs);
            Console.WriteLine("The model is saved to {0}", persistedModelPath);
        }

        private void CheckTrained()
        {
            if (TrainedModel == null)
                throw new InvalidOperationException("Cannot test before training. Call Train() first.");
        }

    }
}
