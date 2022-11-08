using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace AutoMLTrialRunner
{
    public class SSARunner : ITrialRunner
    {
        private readonly MLContext _context;
        private readonly TrainTestData _data;
        private readonly IDataView _trainDataset;
        private readonly IDataView _evaluateDataset;
        private readonly SweepablePipeline _pipeline;
        private readonly string _labelColumnName;

        public SSARunner(MLContext context, TrainTestData data, string labelColumnName, SweepablePipeline pipeline)
        {
            _context = context;
            _data = data;
            _trainDataset = data.TrainSet;
            _evaluateDataset = data.TestSet;
            _labelColumnName = labelColumnName;
            _pipeline = pipeline;
        }

        public void Dispose()
        {
            return;
        }

        // Run trial asynchronously
        public Task<TrialResult> RunAsync(TrialSettings settings, CancellationToken ct)
        {
            try
            {
                return Task.Run(() => Run(settings));
            }
            catch (Exception ex) when (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException(ex.Message, ex.InnerException);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Helper function to define trial run logic
        private TrialResult Run(TrialSettings settings)
        {
            try
            {
                // Initialize stop watch to measure time
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                // Get pipeline parameters
                var parameter = settings.Parameter["_pipeline_"];

                // Build pipeline from parameters
                var pipeline = _pipeline.BuildFromOption(_context, parameter);

                // Train model
                var model = pipeline.Fit(_trainDataset);

                // Create prediction engine for single predictions
                var predictEngine = model.CreateTimeSeriesEngine<ForecastInput, ForecastOutput>(_context);

                // Create a checkpoint for time series engine prediction
                predictEngine.CheckPoint(_context, "origin");

                var predictedLoad1H = new List<float>();
                var N = _evaluateDataset.GetRowCount();

                // Evaluate performance on a rolling basis
                foreach (var load in _evaluateDataset.GetColumn<Single>(_labelColumnName))
                {
                    // First, get next n predictions where n is horizon, in this case, it's always 1.
                    var predict = predictEngine.Predict();

                    // Add prediction to list of predictions
                    predictedLoad1H.Add(predict.Prediction[0]);

                    // Update model with true value
                    predictEngine.Predict(new ForecastInput()
                    {
                        Load = load,
                    });
                }

                // Calculate (Root Mean Squared Error) evaluation metric 
                var rmse = Enumerable.Zip(_evaluateDataset.GetColumn<float>(_labelColumnName), predictedLoad1H)
                                       .Select(x => Math.Pow(x.First - x.Second, 2))
                                       .Average();
                rmse = Math.Sqrt(rmse);

                return new TrialResult()
                {
                    Metric = rmse,
                    Model = model,
                    TrialSettings = settings,
                    DurationInMilliseconds = stopWatch.ElapsedMilliseconds,
                };
            }
            catch (Exception)
            {
                return new TrialResult()
                {
                    Metric = double.MinValue,
                    Model = null,
                    TrialSettings = settings,
                    DurationInMilliseconds = 0,
                };
            }
        }

        // Define input schema
        private class ForecastInput
        {
            [ColumnName("load")]
            public float Load { get; set; }
        }

        // Define output schema
        private class ForecastOutput
        {
            [ColumnName("prediction")]
            public float[] Prediction { get; set; }
        }
    }
}
