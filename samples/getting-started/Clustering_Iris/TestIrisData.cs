namespace Clustering_Iris
{
    internal class TestIrisData
    {
        internal static readonly IrisData Setosa1 = new IrisData()
        {
            SepalLength = 3.3f,
            SepalWidth = 1.6f,
            PetalLength = 0.2f,
            PetalWidth = 5.1f,
        };

        internal static readonly IrisData Setosa2 = new IrisData()
        {
            SepalLength = 5.1f,			
            SepalWidth = 3.5f,
            PetalLength = 1.4f,
            PetalWidth = 0.2f,
        };

        internal static readonly IrisData Virginica1 = new IrisData()
        {
            SepalLength = 3.1f,
            SepalWidth = 5.5f,
            PetalLength = 2.2f,
            PetalWidth = 6.4f,
        };

        internal static readonly IrisData Virginica2 = new IrisData()
        {
            SepalLength = 6.3f, 			
            SepalWidth = 3.3f,
            PetalLength = 6f,
            PetalWidth = 2.5f,
        };

        internal static readonly IrisData Versicolor1 = new IrisData()
        {
            SepalLength = 3.1f,
            SepalWidth = 4.5f,
            PetalLength = 1.5f,
            PetalWidth = 6.4f,
        };

        internal static readonly IrisData Versicolor2 = new IrisData()
        {
            SepalLength = 3.2f, 			
            SepalWidth = 4.7f,
            PetalLength = 1.4f,
            PetalWidth = 7.0f,
        };
    }
}