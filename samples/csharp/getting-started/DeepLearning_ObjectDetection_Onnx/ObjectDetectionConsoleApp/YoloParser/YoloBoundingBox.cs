using System.Drawing;

namespace ObjectDetection
{
    class YoloBoundingBox
    {
        public BoundingBoxDimensions Dimensions { get; set; }

        public string Label { get; set; }

        public float Confidence { get; set; }

        public RectangleF Rect
        {
            get { return new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
        }

        public Color BoxColor { get; set; }
    }

    class BoundingBoxDimensions
    {
        public float X {get;set;}
        public float Y {get;set;}
        public float Height {get;set;}
        public float Width {get;set;}
    }
}