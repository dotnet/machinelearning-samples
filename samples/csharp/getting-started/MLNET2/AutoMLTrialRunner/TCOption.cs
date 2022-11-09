using Microsoft.ML.SearchSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMLTrialRunner
{
    public class TCOption
    {
        [Range(64, 128, 32)]
        public int BatchSize { get; set; }
    }
}
