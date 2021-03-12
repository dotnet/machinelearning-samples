using System;
using System.Linq;
using StopSignDetectionML.Model;

namespace StopSignDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new ModelInput()
            {
                ImageSource = "test-image1.jpeg"
            };

            ModelOutput output = ConsumeModel.Predict(input);

            foreach (BoundingBox box in output.BoundingBoxes)
            {
                Console.WriteLine($"Found {box.Label} at coordinates Left: {box.Left}, Right: {box.Right}, Top: {box.Top}, Bottom: {box.Bottom} with Confidence Score {box.Score}");
            }

        }
    }
}
