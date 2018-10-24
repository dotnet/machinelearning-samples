using BikeSharingDemand.BikeSharingDemandData;
using Microsoft.ML.Runtime.Data;

using BikeSharingDemand.Helpers;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Learners;
using System;
using System.IO;

namespace BikeSharingDemand.Model
{
    public class ModelBuilder<TPredictionTransformer> where TPredictionTransformer : class, ITransformer
    {   
        private LocalEnvironment _mlcontext; 

        public ModelBuilder(string trainingDataLocation)
        {
            _mlcontext = new LocalEnvironment();
        }

        //public TransformerChain<TPredictionTransformer>
        public TransformerChain<RegressionPredictionTransformer<LinearRegressionPredictor>>
                    BuildAndTrainWithSdcaRegressionTrainer(EstimatorChain<ITransformer> pipeline, IDataView dataView)
        {
            var pipelineWithTrainer = pipeline.Append(new SdcaRegressionTrainer(_mlcontext,
                                                                                new SdcaRegressionTrainer.Arguments(),
                                                                                "Features",
                                                                                "Label"
                                                                                ));

            Console.WriteLine("=============== Training model ===============");
            var model = pipelineWithTrainer.Fit(dataView);

            return model;
        }

        //public TransformerChain<TPredictionTransformer>
        public TransformerChain<RegressionPredictionTransformer<PoissonRegressionPredictor>>
                    BuildAndTrainWithPoissonRegressionTrainer(EstimatorChain<ITransformer> pipeline, IDataView dataView)
        {
            var pipelineWithTrainer = pipeline.Append(new PoissonRegression(_mlcontext,
                                                                            "Features",
                                                                            "Label"
                                                                            ));

            Console.WriteLine("=============== Training model ===============");
            var model = pipelineWithTrainer.Fit(dataView);

            return model;
        }

        public void TestSinglePrediction(TransformerChain<TPredictionTransformer> model)      
        {
            //Prediction test
            // Create prediction engine and make prediction.
            var engine = model.MakePredictionFunction<BikeSharingDemandSample, BikeSharingDemandPrediction>(_mlcontext);

            //Sample: 
            // instant,dteday,season,yr,mnth,hr,holiday,weekday,workingday,weathersit,temp,atemp,hum,windspeed,casual,registered,cnt
            // 13950,2012-08-09,3,1,8,10,0,4,1,1,0.8,0.7576,0.55,0.2239,72,133,205
            var demandSample = new BikeSharingDemandSample()
            {
                Season = 3,
                Year = 1,
                Month = 8,
                Hour = 10,
                Holiday = 0,
                Weekday = 4,
                WorkingDay = 1,
                Weather = 1,
                Temperature = (float)0.8,
                NormalizedTemperature = (float)0.7576,
                Humidity = (float)0.55,
                Windspeed = (float)0.2239
            };

            var prediction = engine.Predict(demandSample);
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"Predicted : {prediction.PredictedCount}");
            Console.WriteLine($"*************************************************");
        }

        public void SaveModelAsFile(TransformerChain<TPredictionTransformer> model, string persistedModelPath)
        {
            using (var fs = new FileStream(persistedModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                model.SaveTo(_mlcontext, fs);

            Console.WriteLine("The model is saved to {0}", persistedModelPath);
        }
    }
}
