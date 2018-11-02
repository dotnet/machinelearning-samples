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

        public ModelScorer(MLContext mlContext)
        {
            _mlContext = mlContext;
        }

        public TPrediction PredictSingle(TObservation input)
        {
            return PredictionFunction.Predict(input);
        }

        public IEnumerable<TPrediction> PredictBatch(IDataView inputDataView)
        {
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
    }

}
