using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Drawing;
using WeatherRecognition;

var context = new MLContext();

var emptyData = new List<WeatherRecognitionInput>();

var data = context.Data.LoadFromEnumerable(emptyData);

var pipeline = context.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "data", imageWidth: 300, imageHeight: 300, inputColumnName: nameof(WeatherRecognitionInput.Image))
                .Append(context.Transforms.ExtractPixels(outputColumnName: "data"))
                .Append(context.Transforms.ApplyOnnxModel(modelFile: "./model/model.onnx", outputColumnName: "model_output", inputColumnName: "data"));

var model = pipeline.Fit(data);

var predictionEngine = context.Model.CreatePredictionEngine<WeatherRecognitionInput, WeatherRecognitionPrediction>(model);

var labels = File.ReadAllLines("./model/labels.txt");

var testFiles = Directory.GetFiles("./test");

Bitmap testImage;

foreach (var image in testFiles)
{
    using (var stream = new FileStream(image, FileMode.Open))
    {
        testImage = (Bitmap)Image.FromStream(stream);
    }

    var prediction = predictionEngine.Predict(new WeatherRecognitionInput { Image = testImage });

    var maxValue = prediction.PredictedLabels.Max();
    var maxIndex = prediction.PredictedLabels.ToList().IndexOf(maxValue);

    var predictedLabel = labels[maxIndex];

    Console.WriteLine($"Prediction for file {image}: {predictedLabel}");
}