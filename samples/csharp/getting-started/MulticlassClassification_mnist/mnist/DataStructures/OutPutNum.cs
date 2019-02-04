using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace mnist.DataStructures
{
    class OutPutNum
    {

        [ColumnName("Score")]
        public float[] Score;
    }
}
