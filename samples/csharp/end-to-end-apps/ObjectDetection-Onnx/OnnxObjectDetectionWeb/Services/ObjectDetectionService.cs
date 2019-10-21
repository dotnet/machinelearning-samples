using Microsoft.Extensions.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using OnnxObjectDetection;

namespace OnnxObjectDetectionWeb.Services
{
    public interface IObjectDetectionService
    {
        void DetectObjectsUsingModel(ImageInputData imageInputData);
        Image DrawBoundingBox(string imageFilePath);
    }

    public class ObjectDetectionService : IObjectDetectionService
    {
        List<BoundingBox> filteredBoxes;
        private readonly OnnxOutputParser outputParser = new OnnxOutputParser(new TinyYoloModel(null));
        private readonly PredictionEnginePool<ImageInputData, TinyYoloPrediction> predictionEngine;

        public ObjectDetectionService(PredictionEnginePool<ImageInputData, TinyYoloPrediction> predictionEngine)
        {
            this.predictionEngine = predictionEngine;
        }

        public void DetectObjectsUsingModel(ImageInputData imageInputData)
        {
            var probs = predictionEngine.Predict(imageInputData).PredictedLabels;
            List<BoundingBox> boundingBoxes = outputParser.ParseOutputs(probs);
            filteredBoxes = outputParser.FilterBoundingBoxes(boundingBoxes, 5, .5F);
        }

        public Image DrawBoundingBox(string imageFilePath)
        {
            Image image = Image.FromFile(imageFilePath);
            var originalHeight = image.Height;
            var originalWidth = image.Width;
            foreach (var box in filteredBoxes)
            {
                //// process output boxes
                var x = (uint)Math.Max(box.Dimensions.X, 0);
                var y = (uint)Math.Max(box.Dimensions.Y, 0);
                var width = (uint)Math.Min(originalWidth - x, box.Dimensions.Width);
                var height = (uint)Math.Min(originalHeight - y, box.Dimensions.Height);

                // fit to current image size
                x = (uint)originalWidth * x / ImageSettings.imageWidth;
                y = (uint)originalHeight * y / ImageSettings.imageHeight;
                width = (uint)originalWidth * width / ImageSettings.imageWidth;
                height = (uint)originalHeight * height / ImageSettings.imageHeight;

                using (Graphics thumbnailGraphic = Graphics.FromImage(image))
                {
                    thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Define Text Options
                    Font drawFont = new Font("Arial", 12, FontStyle.Bold);
                    SizeF size = thumbnailGraphic.MeasureString(box.Description, drawFont);
                    SolidBrush fontBrush = new SolidBrush(Color.Black);
                    Point atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

                    // Define BoundingBox options
                    Pen pen = new Pen(box.BoxColor, 3.2f);
                    SolidBrush colorBrush = new SolidBrush(box.BoxColor);

                    // Draw text on image 
                    thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
                    thumbnailGraphic.DrawString(box.Description, drawFont, fontBrush, atPoint);

                    // Draw bounding box on image
                    thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
                }
            }
            return image;
        }
    }
}
