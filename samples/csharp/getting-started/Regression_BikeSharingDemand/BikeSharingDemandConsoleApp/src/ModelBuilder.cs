using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

using BikeSharingDemand.DataStructures;

namespace BikeSharingDemand
{
    public class ModelBuilder
    {
        private MLContext _mlcontext;
        private IEstimator<ITransformer> _trainingPipeline;

        public ITransformer TrainedModel { get; private set; }
        public PredictionFunction<DemandObservation, DemandPrediction> PredictionFunction { get; private set;}

        public ModelBuilder(
            MLContext mlContext,
            IEstimator<ITransformer> dataPreprocessPipeline,
            IEstimator<ITransformer> regressionLearner)
        {
            _mlcontext = mlContext;
            _trainingPipeline = dataPreprocessPipeline.Append(regressionLearner);
        }
        
        public ITransformer Train(IDataView trainingData)
        {
            TrainedModel = _trainingPipeline.Fit(trainingData);
            PredictionFunction = TrainedModel.MakePredictionFunction<DemandObservation, DemandPrediction>(_mlcontext);
            return TrainedModel;
        }

        /// <summary>
        /// For single prediction it's easier to use the PredictionFunction
        /// beacuse we can directly use the BikeSharingData.DemandSample and
        /// BikeSharingData.Prediction classes instead of IDataView.
        /// </summary>
        /// <param name="input">Single data</param>
        /// <returns>Prediction for the input data</returns>
        public DemandPrediction PredictSingle(DemandObservation input)
        {
            CheckTrained();
            return PredictionFunction.Predict(input);
        }

        public IEnumerable<DemandPrediction> PredictBatch(IDataView inputDataView)
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(inputDataView);
            return predictions.AsEnumerable<DemandPrediction>(_mlcontext, reuseRowObject: false);
        }

        public RegressionEvaluator.Result Evaluate(IDataView testData)
        {
            CheckTrained();
            var predictions = TrainedModel.Transform(testData);
            var metrics = _mlcontext.Regression.Evaluate(predictions, "Count", "Score");
            return metrics;
        }

        public void SaveAsFile(string persistedModelPath)
        {
            CheckTrained();
            using (var fs = new FileStream(persistedModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                _mlcontext.Model.Save(TrainedModel, fs);
            Console.WriteLine("The model is saved to {0}", persistedModelPath);
        }

        private void CheckTrained()
        {
            if (TrainedModel == null || PredictionFunction == null)
                throw new InvalidOperationException("Cannot test before training. Call Train() first.");
        }

    }
}
