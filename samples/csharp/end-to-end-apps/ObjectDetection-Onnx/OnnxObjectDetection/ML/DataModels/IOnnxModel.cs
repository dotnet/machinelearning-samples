namespace OnnxObjectDetection
{
    public interface IOnnxModel
    {
        public string ModelPath { get; }

        // To check Model input and output parameter names, you can
        // use tools like Netron: https://github.com/lutzroeder/netron
        public string ModelInput { get; }
        public string ModelOutput { get; }

        public string[] Labels { get; }
        public float[] Anchors { get; }
    }

    public interface IOnnxObjectPrediction
    {
        public float[] PredictedLabels { get; set; }
    }
}
