using BikeSharingDemand.BikeSharingDemandData;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;

using BikeSharingDemand.Helpers;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Learners;
using System;
using static Microsoft.ML.Runtime.Data.DataCommand;
using Microsoft.ML.Runtime.Api;

namespace BikeSharingDemand.Model
{
    public sealed class ModelBuilder
    {
        private readonly string _trainingDataLocation;
        private EstimatorChain<ITransformer> _pipeline;
        private IDataView _dataView;
        private LocalEnvironment _mlcontext; 

        public ModelBuilder(string trainingDataLocation)
        {
            _trainingDataLocation = trainingDataLocation;
            _mlcontext = new LocalEnvironment();
        }

        public void TransformDataInPipeline()
        {
            // Create the TextLoader by defining the data columns and where to find (column position) them in the text file.

            //TODO: Use FeatureVector to load all the 11 columns to compose a single feature column.

            //var textLoader = new TextLoader(_context,
            //                                new TextLoader.Arguments()
            //                                {
            //                                    Separator = ",",
            //                                    HasHeader = true,
            //                                    Column = new[]
            //                                    {
            //                                        // We read the first 10 values as a single float vector.
            //                                        new TextLoader.Column("FeatureVector", DataKind.R4, new[] {new TextLoader.Range(2, 12)}),
            //                                        // Separately, read the target variable.
            //                                        new TextLoader.Column("Count", DataKind.R4, 16)                                                    
            //                                    }
            //                                });

            var textLoader = new TextLoader(_mlcontext,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = ",",
                                                HasHeader = true,
                                                Column = new[]
                                                {
                                                    new TextLoader.Column("Season", DataKind.R4, 2),
                                                    new TextLoader.Column("Year", DataKind.R4, 3),
                                                    new TextLoader.Column("Month", DataKind.R4, 4),
                                                    new TextLoader.Column("Hour", DataKind.R4, 5),
                                                    new TextLoader.Column("Holiday", DataKind.R4, 6),
                                                    new TextLoader.Column("Weekday", DataKind.R4, 7),
                                                    new TextLoader.Column("WorkingDay", DataKind.R4, 7),
                                                    new TextLoader.Column("Weather", DataKind.R4, 8),
                                                    new TextLoader.Column("Temperature", DataKind.R4, 9),
                                                    new TextLoader.Column("NormalizedTemperature", DataKind.R4, 10),
                                                    new TextLoader.Column("Humidity", DataKind.R4, 11),
                                                    new TextLoader.Column("Windspeed", DataKind.R4, 12),
                                                    new TextLoader.Column("Count", DataKind.R4, 16)
                                                }
                                            });

            // Now read the file (remember though, readers are lazy, so the actual reading will happen when 'fitting').
            _dataView = textLoader.Read(new MultiFileSource(_trainingDataLocation));

            //Copy the Count column to the Label column 
            var estimator = new CopyColumnsEstimator(_mlcontext, "Count", "Label");

            //Concatenate all the numeric columns into a single features column
            var _pipeline = estimator.Append(new ConcatEstimator(_mlcontext, "Features", 
                                                                         new[] { "Season", "Year", "Month",
                                                                                 "Hour", "Holiday", "Weekday",
                                                                                 "Weather", "Temperature", "NormalizedTemperature",
                                                                                 "Humidity", "Windspeed" }));
            //Peek some data in the DataView
            //ConsoleHelper.PeekDataViewInConsole(_mlcontext, _dataView, _pipeline, 10);
            //ConsoleHelper.PeekFeaturesColumnDataInConsole("Features", _mlcontext, _dataView, _pipeline, 10);

            //Train with SdcaRegressionTrainer
            //_pipeline.Append(new SdcaRegressionTrainer(_mlcontext,
            //                                           new SdcaRegressionTrainer.Arguments(),
            //                                           "Features",
            //                                           "Label"
            //                                           ));

            //Train with OnlineGradientDescentTrainer
            _pipeline.Append(new OnlineGradientDescentTrainer(_mlcontext, "Label", "Features"));

            Console.WriteLine("=============== Training model ===============");

            var model = _pipeline.Fit(_dataView);

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
        }

        public TransformerChain<ITransformer> BuildAndTrainWithSdcaRegressionTrainer()
        {
            ITrainer algorithm = new SdcaRegressionTrainer(_mlcontext,
                                      new SdcaRegressionTrainer.Arguments(),
                                      "Features",
                                      "Label"
                                     );

            return BuildAndTrain(algorithm);
        }

        //Microsoft.ML.Runtime.Learners.SdcaRegressionTrainer

        public TransformerChain<ITransformer> BuildAndTrain(ITrainer algorithm)
        {
            //TODO: Parametrize the Trainer/Learner
            _pipeline.Append(new SdcaRegressionTrainer(_mlcontext,
                                                       new SdcaRegressionTrainer.Arguments(),
                                                       "Features",
                                                       "Label"
                                                       ));
                     //.Append(new KeyToValueEstimator(_mlcontext, "PredictedLabel"));

            Console.WriteLine("=============== Training model ===============");

            var model = _pipeline.Fit(_dataView);

            return model;
        }


        //ITrainer algorithm;


        /// <summary>
        /// Using training data location that is passed trough constructor this method is building
        /// and training machine learning model.
        /// </summary>
        /// <returns>Trained machine learning model.</returns>
        //public PredictionModel<BikeSharingDemandSample, BikeSharingDemandPrediction> BuildAndTrain()
        //{
        //    var pipeline = new LearningPipeline();
        //    pipeline.Add(new TextLoader(_trainingDataLocation).CreateFrom<BikeSharingDemandSample>(useHeader: true, separator: ','));
        //    pipeline.Add(new ColumnCopier(("Count", "Label")));
        //    pipeline.Add(new ColumnConcatenator("Features", 
        //                                        "Season", 
        //                                        "Year", 
        //                                        "Month", 
        //                                        "Hour", 
        //                                        "Weekday", 
        //                                        "Weather", 
        //                                        "Temperature", 
        //                                        "NormalizedTemperature",
        //                                        "Humidity",
        //                                        "Windspeed"));
        //    pipeline.Add(_algorithm);

        //    return pipeline.Train<BikeSharingDemandSample, BikeSharingDemandPrediction>();
        //}
    }
}
