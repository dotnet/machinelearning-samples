using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMLTrialRunner
{
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

        public class TCRunner : ITrialRunner
        {
            private readonly MLContext _context;
            private readonly TrainTestData _data;
            private readonly IDataView _trainDataset;
            private readonly IDataView _evaluateDataset;
            private readonly SweepablePipeline _pipeline;
            private readonly string _labelColumnName;
            private readonly MulticlassClassificationMetric _metric;

            public TCRunner(
                MLContext context, 
                TrainTestData data, 
                SweepablePipeline pipeline,
                string labelColumnName = "Label", 
                MulticlassClassificationMetric metric = MulticlassClassificationMetric.MicroAccuracy)
            {
                _context = context;
                _data = data;
                _trainDataset = data.TrainSet;
                _evaluateDataset = data.TestSet;
                _labelColumnName = labelColumnName;
                _pipeline = pipeline;
                _metric = metric;
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

                    // Use parameters to build pipeline
                    var pipeline = _pipeline.BuildFromOption(_context, parameter);

                    // Train model
                    var model = pipeline.Fit(_trainDataset);

                    // Evaluate the model
                    var predictions = model.Transform(_evaluateDataset);

                    // Get metrics
                    var evaluationMetrics = _context.MulticlassClassification.Evaluate(predictions, labelColumnName: _labelColumnName);
                    var chosenMetric = GetMetric(evaluationMetrics);

                    return new TrialResult()
                    {
                        Metric = chosenMetric,
                        Model = model,
                        TrialSettings = settings,
                        DurationInMilliseconds = stopWatch.ElapsedMilliseconds
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

            // Helper function to choose metric used by experiment
            private double GetMetric(MulticlassClassificationMetrics metric)
            {
                return _metric switch
                {
                    MulticlassClassificationMetric.MacroAccuracy => metric.MacroAccuracy,
                    MulticlassClassificationMetric.MicroAccuracy => metric.MicroAccuracy,
                    MulticlassClassificationMetric.LogLoss => metric.LogLoss,
                    MulticlassClassificationMetric.LogLossReduction => metric.LogLossReduction,
                    MulticlassClassificationMetric.TopKAccuracy => metric.TopKAccuracy,
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}
