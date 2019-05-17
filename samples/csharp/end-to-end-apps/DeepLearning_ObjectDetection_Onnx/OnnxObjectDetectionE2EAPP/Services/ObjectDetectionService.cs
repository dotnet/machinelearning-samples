using Microsoft.Extensions.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OnnxObjectDetectionE2EAPP.Services
{
    public interface IObjectDetectionService
    {
        void DetectObjectsUsingModel(string imagesFilePath);
        Image PaintImages(string imageFilePath);
    }
    public class ObjectDetectionService : IObjectDetectionService
    {
        private readonly YoloWinMlParser _parser = new YoloWinMlParser();
        IList<YoloBoundingBox> filteredBoxes;
        private readonly PredictionEnginePool<ImageNetData, ImageNetPrediction> model;

        public ObjectDetectionService(PredictionEnginePool<ImageNetData, ImageNetPrediction> model)
        {
            this.model = model;
        }

        public void DetectObjectsUsingModel(string imagesFilePath)
        {
            var imageInputData = new ImageNetData { ImagePath = imagesFilePath };
            var probs = model.Predict(imageInputData).PredictedLabels;
            IList<YoloBoundingBox> boundingBoxes = _parser.ParseOutputs(probs);
            filteredBoxes = _parser.NonMaxSuppress(boundingBoxes, 5, .5F);
        }

        public Image PaintImages(string imageFilePath)
        {
            Image image = Image.FromFile(imageFilePath);
            var originalHeight = image.Height;
            var originalWidth = image.Width;
            foreach (var box in filteredBoxes)
            {
                //// process output boxes
                var x = (uint)Math.Max(box.X, 0);
                var y = (uint)Math.Max(box.Y, 0);
                var w = (uint)Math.Min(originalWidth - x, box.Width);
                var h = (uint)Math.Min(originalHeight - y, box.Height);

                // fit to current image size
                x = (uint)originalWidth * x / 416;
                y = (uint)originalHeight * y / 416;
                w = (uint)originalWidth * w / 416;
                h = (uint)originalHeight * h / 416;

                string text = string.Format("{0} ({1})", box.Label, box.Confidence);

                using (Graphics graph = Graphics.FromImage(image))
                {
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.HighQuality;
                    graph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    Font drawFont = new Font("Arial", 16);
                    SolidBrush redBrush = new SolidBrush(Color.Red);
                    Point atPoint = new Point((int)x, (int)y);
                    Pen pen = new Pen(Color.Yellow, 4.0f);
                    SolidBrush yellowBrush = new SolidBrush(Color.Yellow);

                    // Fill rectangle on which the text is displayed.
                    RectangleF rect = new RectangleF(x, y, w, 20);
                    graph.FillRectangle(yellowBrush, rect);
                    //draw text in red color
                    graph.DrawString(text, drawFont, redBrush, atPoint);
                    //draw rectangle around object
                    graph.DrawRectangle(pen, x, y, w, h);
                }
            }
            return image;
        }



    }
}
