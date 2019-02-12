using MulticlassClassification_Iris.DataStructures;

namespace MulticlassClassification_Iris.DataStructures
{
    public class SampleIrisData
    {
        internal static readonly IrisData Iris1 = new IrisData()
        {
            SepalLength = 5.1f,
            SepalWidth = 3.3f,
            PetalLength = 1.6f,
            PetalWidth= 0.2f,
        };
        
        internal static readonly IrisData Iris2 = new IrisData()
        {
            SepalLength = 6.0f,
            SepalWidth = 3.4f,
            PetalLength = 6.1f,
            PetalWidth = 2.0f,
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