using BikeSharingDemand.BikeSharingDemandData;
using Microsoft.ML.Runtime.Data;

using BikeSharingDemand.Helpers;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Learners;
using System;
using System.IO;

namespace BikeSharingDemand.Model
{
    public sealed class PipelineTransforms
    {
        private readonly string _trainingDataLocation;
        private LocalEnvironment _mlcontext; 

        public PipelineTransforms(string trainingDataLocation)
        {
            _trainingDataLocation = trainingDataLocation;
            _mlcontext = new LocalEnvironment();
        }

        public IDataView CreateDataView()
        {
            //Create TextLoader with schema related to columns in the training data file
            TextLoader textLoader = new BikeSharingTextLoaderFactory().CreateTextLoader(_mlcontext);

            // Now read the file (remember though, readers are lazy, so the actual reading will happen when 'fitting').
            IDataView dataView = textLoader.Read(new MultiFileSource(_trainingDataLocation));
            return dataView;
        }

        public EstimatorChain<ITransformer> TransformDataInPipeline(IDataView dataView)
        {
            //Copy the Count column to the Label column 
            var estimator = new CopyColumnsEstimator(_mlcontext, "Count", "Label");

            //Concatenate all the numeric columns into a single features column
            var pipeline = estimator.Append(new ConcatEstimator(_mlcontext, "Features",
                                                                         new[] { "Season", "Year", "Month",
                                                                                 "Hour", "Holiday", "Weekday",
                                                                                 "Weather", "Temperature", "NormalizedTemperature",
                                                                                 "Humidity", "Windspeed" }));
            //Peek some data in the DataView
            ConsoleHelper.PeekDataViewInConsole(_mlcontext, dataView, pipeline, 10);
            ConsoleHelper.PeekFeaturesColumnDataInConsole("Features", _mlcontext, dataView, pipeline, 10);

            return pipeline;
        }
    }
}
