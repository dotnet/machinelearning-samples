using System;
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

            Console.WriteLine("\n\nPredicted Boxes:\n");
            Console.WriteLine(output);
        }
    }
}
