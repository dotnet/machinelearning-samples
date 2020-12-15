using Microsoft.ML.Data;

namespace SrCnnEntireDetection.DataStructures
{
    public class PhoneCallsData
    {
        [LoadColumn(0)]
        public string timestamp;

        [LoadColumn(1)]
        public double value;
    }
}
