using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using StopSignDetection_ONNX;
using System.Drawing;

string[] testFiles = new[] { "./test/stop-sign-test.jpg" };

var context = new MLContext();

var data = context.Data.LoadFromEnumerable(new List<StopSignInput>());

// Create pipeline
var pipeline = context.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image_tensor", imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(StopSignInput.Image))
                .Append(context.Transforms.ExtractPixels(outputColumnName: "image_tensor"))
                .Append(context.Transforms.ApplyOnnxModel(outputColumnNames: new string[] { "detected_boxes", "detected_scores", "detected_classes" }, 
                    inputColumnNames: new string[] { "image_tensor" }, modelFile: "./Model/model.onnx"));

// Fit and create prediction engine
var model = pipeline.Fit(data);

var predictionEngine = context.Model.CreatePredictionEngine<StopSignInput, StopSignPrediction>(model);

var labels = File.ReadAllLines("./Model/labels.txt");

Bitmap testImage;

foreach (var image in testFiles)
{
    // Load test image into memory
    var predictedImage = $"{image}-predicted.jpg";

    using (var stream = new FileStream(image, FileMode.Open))
    {
        testImage = (Bitmap)Image.FromStream(stream);
    }

    // Predict on test image
    var prediction = predictionEngine.Predict(new StopSignInput { Image = testImage });

    // Calculate bounding boxes based on prediction
    var originalWidth = testImage.Width;
    var originalHeight = testImage.Height;

    var left = prediction.BoundingBoxes[0] * originalWidth;
    var top = prediction.BoundingBoxes[1] * originalHeight;
    var right = prediction.BoundingBoxes[2] * originalWidth;
    var bottom = prediction.BoundingBoxes[3] * originalHeight;

    var x = left;
    var y = top;
    var width = Math.Abs(right - left);
    var height = Math.Abs(top - bottom);

    // Get predicted label from labels file
    var label = labels[prediction.PredictedLabels[0]];

    // Draw bounding box and add label to image
    using (var graphics = Graphics.FromImage(testImage))
    {
        graphics.DrawRectangle(new Pen(Color.Red, 3), x, y, width, height);
        graphics.DrawString(label, new Font(FontFamily.Families[0], 55f), Brushes.Red, x + 5, y + 5);
    }

    // Save the prediction image, but delete it if it already exists before saving
    if (File.Exists(predictedImage))
    {
        File.Delete(predictedImage);
    }

    testImage.Save(predictedImage);
}