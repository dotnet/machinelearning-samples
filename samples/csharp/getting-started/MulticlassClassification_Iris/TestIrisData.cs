namespace MulticlassClassification_Iris
{
    public static partial class Program
    {
        internal class TestIrisData
        {
            internal static readonly IrisData Iris0 = new IrisData()
            {
                SepalLength = 5.0f,
                SepalWidth = 2.0f,
                PetalLength = 3.5f,
                PetalWidth = 1.0f,
            };

            internal static readonly IrisData Iris1 = new IrisData()
            {
                SepalLength = 5.1f,
                SepalWidth = 3.3f,
                PetalLength = 1.6f,
                PetalWidth= 0.2f,
            };

            internal static readonly IrisData Iris2 = new IrisData()
            {
                SepalLength = 6.4f,
                SepalWidth = 3.1f,
                PetalLength = 5.5f,
                PetalWidth = 2.2f,
            };

            internal static readonly IrisData Iris3 = new IrisData()
            {
                SepalLength = 4.4f,
                SepalWidth = 3.1f,
                PetalLength = 2.5f,
                PetalWidth = 1.2f,
            };
        }
    }
}