using Microsoft.ML.Data;

namespace MNIST.DataStructures
{
    class OutputData
    {
        [ColumnName("Score")]
        public float[] Score;
    }
}