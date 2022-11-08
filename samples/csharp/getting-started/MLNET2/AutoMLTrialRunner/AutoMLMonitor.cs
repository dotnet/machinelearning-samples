using Microsoft.ML.AutoML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMLTrialRunner
{
    public class AutoMLMonitor : IMonitor
    {
        private readonly SweepablePipeline _pipeline;

        public AutoMLMonitor(SweepablePipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public void ReportBestTrial(TrialResult result)
        {
            return;
        }

        public void ReportCompletedTrial(TrialResult result)
        {
            var trialId = result.TrialSettings.TrialId;
            var timeToTrain = result.DurationInMilliseconds;
            var pipeline = _pipeline.ToString(result.TrialSettings.Parameter);
            Console.WriteLine($"Trial {trialId} finished training in {timeToTrain}ms with pipeline {pipeline}");
        }

        public void ReportFailTrial(TrialSettings settings, Exception exception = null)
        {
            if (exception.Message.Contains("Operation was canceled."))
            {
                Console.WriteLine($"{settings.TrialId} cancelled. Time budget exceeded.");
            }
            Console.WriteLine($"{settings.TrialId} failed with exception {exception.Message}");
        }

        public void ReportRunningTrial(TrialSettings setting)
        {
            return;
        }
    }
}
