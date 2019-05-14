using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace mnist.DataStructures
{
    class OutPutData
    {
        [ColumnName("Score")]
        public float[] Score;
    }
}
