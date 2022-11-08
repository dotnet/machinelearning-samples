//Load sample data
using TextClassification;

var sampleData = new ReviewSentiment.ModelInput()
{
    Col0 = @"Crust is not good.",
};

//Load model and predict output
var result = ReviewSentiment.Predict(sampleData);

// Print sentiment
Console.WriteLine($"Sentiment: {(result.PredictedLabel == 0 ? "Negative" : "Positive")}");
