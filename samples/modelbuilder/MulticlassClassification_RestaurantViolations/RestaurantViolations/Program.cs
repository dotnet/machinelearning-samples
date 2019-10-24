using System;
using RestaurantViolationsML.Model;

namespace RestaurantViolations
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create sample data
            ModelInput input = new ModelInput
            {
                Inspection_type = "Routine - Unscheduled",
                Violation_description = "Wiping cloths not clean or properly stored or inadequate sanitizer"
            };

            // Make prediction
            ModelOutput result = ConsumeModel.Predict(input);

            // Print Prediction
            Console.WriteLine($"Predicted risk category: {result.Prediction}");
            Console.ReadKey();
        }
    }
}
