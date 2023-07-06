using System;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace Common
{
    /// <summary>
    /// Progress handler that AutoML will invoke after each model it produces and evaluates.
    /// </summary>
    public class BinaryExperimentProgressHandler : IProgress<RunDetail<BinaryClassificationMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<BinaryClassificationMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelperAutoML.PrintBinaryClassificationMetricsHeader();
            }

            if (iterationResult.Exception != null)
            {
                ConsoleHelperAutoML.PrintIterationException(iterationResult.Exception);
            }
            else
            {
                ConsoleHelperAutoML.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                    iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
            }
        }
    }

    /// <summary>
    /// Progress handler that AutoML will invoke after each model it produces and evaluates.
    /// </summary>
    public class MulticlassExperimentProgressHandler : IProgress<RunDetail<MulticlassClassificationMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<MulticlassClassificationMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelperAutoML.PrintMulticlassClassificationMetricsHeader();
            }

            if (iterationResult.Exception != null)
            {
                ConsoleHelperAutoML.PrintIterationException(iterationResult.Exception);
            }
            else
            {
                ConsoleHelperAutoML.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                    iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
            }
        }
    }

    /// <summary>
    /// Progress handler that AutoML will invoke after each model it produces and evaluates.
    /// </summary>
    public class RegressionExperimentProgressHandler : IProgress<RunDetail<RegressionMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<RegressionMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelperAutoML.PrintRegressionMetricsHeader();
            }

            if (iterationResult.Exception != null)
            {
                ConsoleHelperAutoML.PrintIterationException(iterationResult.Exception);
            }
            else
            {
                ConsoleHelperAutoML.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                    iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
            }
        }
    }

    public class RankingExperimentProgressHandler : IProgress<RunDetail<RankingMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<RankingMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelperAutoML.PrintRankingMetricsHeader();
            }

            if (iterationResult.Exception != null)
            {
                ConsoleHelperAutoML.PrintIterationException(iterationResult.Exception);
            }
            else
            {
                ConsoleHelperAutoML.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                    iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
            }
        }
    }
}
