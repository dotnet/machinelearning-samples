using System;
using Microsoft.ML.Auto;
using Microsoft.ML.Data;

namespace Common
{
    public class BinaryExperimentProgressHandler : IProgress<RunDetail<BinaryClassificationMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<BinaryClassificationMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelper.PrintBinaryClassificationMetricsHeader();
            }
            ConsoleHelper.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
        }
    }

    public class MulticlassExperimentProgressHandler : IProgress<RunDetail<MulticlassClassificationMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<MulticlassClassificationMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelper.PrintMulticlassClassificationMetricsHeader();
            }
            ConsoleHelper.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
        }
    }

    public class RegressionExperimentProgressHandler : IProgress<RunDetail<RegressionMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<RegressionMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelper.PrintRegressionMetricsHeader();
            }
            ConsoleHelper.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName, 
                iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
        }
    }
}
