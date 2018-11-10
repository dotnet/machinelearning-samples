using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

namespace Common
{
    public class ModelScorer<TObservation, TPrediction>
                                    where TObservation : class
                                    where TPrediction : class, new()
    {
        private MLContext _mlContext;
        public ITransformer TrainedModel { get; private set; }
        public PredictionFunction<TObservation, TPrediction> PredictionFunction;

        public ModelScorer(MLContext mlContext, ITransformer trainedModel = null)
        {
            _mlContext = mlContext;

            if(trainedModel != null)
            {
                //Keep the trainedModel passed through the constructor
                TrainedModel = trainedModel;

                // Create prediction engine related to the passed trained model
                PredictionFunction = TrainedModel.MakePredictionFunction<TObservation, TPrediction>(_mlContext);
            }          
        }

        public TPrediction PredictSingle(TObservation input)
        {
            CheckTrainedModelIsLoaded();
            return PredictionFunction.Predict(input);
        }

        public IEnumerable<TPrediction> PredictBatch(IDataView inputDataView)
        {
            CheckTrainedModelIsLoaded();
            var predictions = TrainedModel.Transform(inputDataView);
            return predictions.AsEnumerable<TPrediction>(_mlContext, reuseRowObject: false);
        }

        public ITransformer LoadModelFromZipFile(string modelPath)
        {
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                TrainedModel = TransformerChain.LoadFrom(_mlContext, stream);
            }

            // Create prediction engine related to the loaded trained model
            PredictionFunction = TrainedModel.MakePredictionFunction<TObservation, TPrediction>(_mlContext);

            return TrainedModel;
        }

        private void CheckTrainedModelIsLoaded()
        {
            if (TrainedModel == null)
                throw new InvalidOperationException("Need to have a model before scoring. Call LoadModelFromZipFile(modelPath) first or provided a model through the constructor.");
        }
    }

}
