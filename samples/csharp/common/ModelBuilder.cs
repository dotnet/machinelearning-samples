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
        private IEstimator<ITransformer> _trainingPipeline;
        public ITransformer TrainedModel { get; private set; }

        public ModelBuilder(
            MLContext mlContext,
            IEstimator<ITransformer> dataProcessPipeline,
            IEstimator<ITransformer> trainer)
        {
            _mlcontext = mlContext;
            _trainingPipeline = dataProcessPipeline.Append(trainer);
        }
        
        public ITransformer Train(IDataView trainingData)
        {
            TrainedModel = _trainingPipeline.Fit(trainingData);            
            return TrainedModel;
        }

        public RegressionEvaluator.Result EvaluateRegressionModel(IDataView testData)
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(testData);
            var metrics = _mlcontext.Regression.Evaluate(predictions, "Count", "Score");
            return metrics;
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
