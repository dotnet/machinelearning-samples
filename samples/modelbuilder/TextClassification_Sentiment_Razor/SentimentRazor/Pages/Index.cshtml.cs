using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.ML;
using static SentimentRazor.SentimentAnalysis;

namespace SentimentRazor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;

        public IndexModel(ILogger<IndexModel> logger, PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool)
        {
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
        }

        public void OnGet()
        {

        }

        public IActionResult OnGetAnalyzeSentiment([FromQuery] string text)
        {
            if (String.IsNullOrEmpty(text)) return Content("Neutral");

            var input = new ModelInput { SentimentText = text };

            var prediction = _predictionEnginePool.Predict(input);

            var sentiment = Convert.ToBoolean(prediction.PredictedLabel) ? "Toxic" : "Not Toxic";

            return Content(sentiment);
        }
    }
}