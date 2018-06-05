namespace MulticlassClassification_Iris
{
    internal class TestIrisData
    {
        internal static readonly IrisData Setosa = new IrisData()
        {
            SepalLength = 3.3f,
            SepalWidth = 1.6f,
            PetalLength = 0.2f,
            PetalWidth = 5.1f,
        };

        internal static readonly IrisData Virginica = new IrisData()
        {
            SepalLength = 3.1f,
            SepalWidth = 5.5f,
            PetalLength = 2.2f,
            PetalWidth = 6.4f,
        };

        internal static readonly IrisData Versicolor = new IrisData()
        {
            SepalLength = 3.1f,
            SepalWidth = 2.5f,
            PetalLength = 1.2f,
            PetalWidth = 4.4f,
        };
    }
}