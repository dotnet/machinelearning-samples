using Microsoft.ML.Data;

namespace SpamDetectionConsoleApp.MLDataStructures
{
    class SpamInput
    {
        [LoadColumn(0)]
        public string Label { get; set; }
        [LoadColumn(1)]
        public string Message { get; set; }
    }
}
