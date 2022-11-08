using Microsoft.ML.SearchSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMLTrialRunner
{
    // Define ForecastBySsa search space
    public class SSAOption
    {

        [Range(2, 24 * 7 * 30)]
        public int WindowSize { get; set; } = 2;

        [Range(2, 24 * 7 * 30)]
        public int SeriesLength { get; set; } = 2;

        [Range(2, 24 * 7 * 30)]
        public int TrainSize { get; set; } = 2;

        [Range(1, 24 * 7 * 30)]
        public int Horizon { get; set; } = 1;
    }
}
