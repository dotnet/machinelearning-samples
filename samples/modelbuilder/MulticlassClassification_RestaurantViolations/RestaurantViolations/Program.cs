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
                InspectionType = "Complaint",
                ViolationDescription = "Inadequate sewage or wastewater disposal"
            };

            // Make prediction
            ModelOutput result = ConsumeModel.Predict(input);

            // Print Prediction
            Console.WriteLine($"Inspection type: {input.InspectionType}");
            Console.WriteLine($"Violation description: {input.ViolationDescription}");
            Console.WriteLine($"Predicted risk category: {result.Prediction}");
            Console.ReadKey();
        }
    }
}
