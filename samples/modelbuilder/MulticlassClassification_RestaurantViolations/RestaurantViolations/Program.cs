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
                Violation_id = "80137_20161005_103149",
                Inspection_type = "Routine - Unscheduled",
                Violation_description = "Wiping cloths not clean or properly stored or inadequate sanitizer"
            };

            // Make prediction
            ModelOutput result = ConsumeModel.Predict(input);

            // Print Prediction
            Console.WriteLine($"Risk Category: {result.Prediction}");
            Console.ReadKey();
        }
    }
}
