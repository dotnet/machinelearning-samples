using BikeSharingDemand.BikeSharingDemandData;

using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Transforms;

namespace BikeSharingDemand.Model
{
    public sealed class ModelBuilder
    {
        private readonly string _trainingDataLocation;
        private readonly ILearningPipelineItem _algorithm;

        public ModelBuilder(string trainingDataLocation, ILearningPipelineItem algorithm)
        {
            _trainingDataLocation = trainingDataLocation;
            _algorithm = algorithm;
        }

        /// <summary>
        /// Using training data location that is passed trough constructor this method is building
        /// and training machine learning model.
        /// </summary>
        /// <returns>Trained machine learning model.</returns>
        public PredictionModel<BikeSharingDemandSample, BikeSharingDemandPrediction> BuildAndTrain()
        {
            var pipeline = new LearningPipeline();
            pipeline.Add(new TextLoader(_trainingDataLocation).CreateFrom<BikeSharingDemandSample>(useHeader: true, separator: ','));
            pipeline.Add(new ColumnCopier(("Count", "Label")));
            pipeline.Add(new ColumnConcatenator("Features", 
                                                "Season", 
                                                "Year", 
                                                "Month", 
                                                "Hour", 
                                                "Weekday", 
                                                "Weather", 
                                                "Temperature", 
                                                "NormalizedTemperature",
                                                "Humidity",
                                                "Windspeed"));
            pipeline.Add(_algorithm);

            return pipeline.Train<BikeSharingDemandSample, BikeSharingDemandPrediction>();
        }
    }
}
