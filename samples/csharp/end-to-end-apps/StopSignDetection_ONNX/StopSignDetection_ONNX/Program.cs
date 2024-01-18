using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using StopSignDetection_ONNX;
using System.Drawing;

var context = new MLContext();

var data = context.Data.LoadFromEnumerable(new List<StopSignInput>());
var root = new FileInfo(typeof(Program).Assembly.Location);
var assemblyFolderPath = root.Directory.FullName;

// Create pipeline
var pipeline = context.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image_tensor", imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(StopSignInput.Image))
                .Append(context.Transforms.ExtractPixels(outputColumnName: "image_tensor"))
                .Append(context.Transforms.ApplyOnnxModel(outputColumnNames: new string[] { "detected_boxes", "detected_scores", "detected_classes" }, 
                    inputColumnNames: new string[] { "image_tensor" }, modelFile: "./Model/model.onnx"));

// Fit and create prediction engine
var model = pipeline.Fit(data);

var predictionEngine = context.Model.CreatePredictionEngine<StopSignInput, StopSignPrediction>(model);

var labels = File.ReadAllLines("./Model/labels.txt");

var testFiles = Directory.GetFiles("./test");

Bitmap testImage;

foreach (var image in testFiles)
{
    // Load test image into memory
    var predictedImage = $"{Path.GetFileName(image)}-predicted.jpg";

    using (var stream = new FileStream(image, FileMode.Open))
    {
        testImage = (Bitmap)Image.FromStream(stream);
    }

    // Predict on test image
    var prediction = predictionEngine.Predict(new StopSignInput { Image = testImage });

    // Calculate how many sets of bounding boxes we get from the prediction
    var boundingBoxes = prediction.BoundingBoxes.Chunk(prediction.BoundingBoxes.Count() / prediction.PredictedLabels.Count());

    var originalWidth = testImage.Width;
    var originalHeight = testImage.Height;

    // Draw boxes and predicted label
    for (int i = 0; i < boundingBoxes.Count(); i++)
    {
        var boundingBox = boundingBoxes.ElementAt(i);

        var left = boundingBox[0] * originalWidth;
        var top = boundingBox[1] * originalHeight;
        var right = boundingBox[2] * originalWidth;
        var bottom = boundingBox[3] * originalHeight;

        var x = left;
        var y = top;
        var width = Math.Abs(right - left);
        var height = Math.Abs(top - bottom);

        // Get predicted label from labels file
        var label = labels[prediction.PredictedLabels[i]];

        // Draw bounding box and add label to image
        using var graphics = Graphics.FromImage(testImage);

        graphics.DrawRectangle(new Pen(Color.NavajoWhite, 8), x, y, width, height);
        graphics.DrawString(label, new Font(FontFamily.Families[0], 18f), Brushes.NavajoWhite, x + 5, y + 5);
    }

    // Save the prediction image, but delete it if it already exists before saving
    if (File.Exists(predictedImage))
    {
        File.Delete(predictedImage);
    }

    testImage.Save(Path.Combine(assemblyFolderPath, predictedImage));
}