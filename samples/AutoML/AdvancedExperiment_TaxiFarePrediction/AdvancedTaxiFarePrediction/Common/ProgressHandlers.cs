using System;
using Microsoft.ML.Auto;
using Microsoft.ML.Data;

namespace Common
{
    public class RegressionExperimentProgressHandler : IProgress<RunDetail<RegressionMetrics>>
    {
        private int _iterationIndex;
        private bool _initialized = false;

        public void Report(RunDetail<RegressionMetrics> iterationResult)
        {
            if (!_initialized)
            {
                ConsoleHelper.PrintObserveProgressRegressionHeader();
                _initialized = true;
            }
            _iterationIndex++;
            ConsoleHelper.PrintRegressionIterationMetrics(_iterationIndex, iterationResult.TrainerName, iterationResult.ValidationMetrics);
        }
    }
}
