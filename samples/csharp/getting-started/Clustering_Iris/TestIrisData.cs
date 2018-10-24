namespace Clustering_Iris
{
    internal class TestIrisData
    {
        internal static readonly IrisData Setosa1 = new IrisData()
        {
            SepalLength = 5.1f,
            SepalWidth = 3.3f,
            PetalLength = 1.6f,
            PetalWidth = 0.2f,
        };

        internal static readonly IrisData Setosa2 = new IrisData()
        {
            SepalLength = 0.2f,			
            SepalWidth = 5.1f,
            PetalLength = 3.5f,
            PetalWidth = 1.4f,
        };

        internal static readonly IrisData Virginica1 = new IrisData()
        {
            SepalLength = 6.4f,
            SepalWidth = 3.1f,
            PetalLength = 5.5f,
            PetalWidth = 2.2f,
        };

        internal static readonly IrisData Virginica2 = new IrisData()
        {
            SepalLength = 2.5f, 			
            SepalWidth = 6.3f,
            PetalLength = 3.3f,
            PetalWidth = 6f,
        };

        internal static readonly IrisData Versicolor1 = new IrisData()
        {
            SepalLength = 6.4f,
            SepalWidth = 3.1f,
            PetalLength = 4.5f,
            PetalWidth = 1.5f,
        };

        internal static readonly IrisData Versicolor2 = new IrisData()
        {
            SepalLength = 7.0f, 			
            SepalWidth = 3.2f,
            PetalLength = 4.7f,
            PetalWidth = 1.4f,
        };
    }
}