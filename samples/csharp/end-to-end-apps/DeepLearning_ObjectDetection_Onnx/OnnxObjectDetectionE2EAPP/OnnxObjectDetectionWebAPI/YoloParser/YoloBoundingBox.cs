using System.Drawing;

namespace OnnxObjectDetectionWebAPI
{
    class YoloBoundingBox
    {    
        public string Label { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public float Confidence { get; set; }

        public RectangleF Rect
        {
            get { return new RectangleF(X, Y, Width, Height); }
        }
    }
}