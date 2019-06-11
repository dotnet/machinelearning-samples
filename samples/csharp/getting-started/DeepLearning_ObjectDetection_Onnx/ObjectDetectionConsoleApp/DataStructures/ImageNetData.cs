// <ImageNetDataUsings>
using Microsoft.ML.Data;
// </ImageNetDataUsings>

namespace ObjectDetection
{

    #region ImageNetDataClass
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
    #endregion

    #region ImageNetDataProbabilityClass
    public class ImageNetDataProbability : ImageNetData
    {
        public string PredictedLabel;
        public float Probability { get; set; }
    }
    #endregion

}
