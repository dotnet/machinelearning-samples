using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.ML;
using SentimentRazorML.Model;

namespace SentimentAnalysisRazorPages.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;

        public IndexModel(PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool)
        {
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
            var sentiment = Convert.ToBoolean(prediction.Prediction) ? "Toxic" : "Not Toxic";
            return Content(sentiment);
        }
    }
}
