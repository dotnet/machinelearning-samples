using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Drawing;
using Microsoft.ML;
using LandUseML.Model;
using System.Reflection;

namespace LandUseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassificationController : ControllerBase
    {
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;
        private readonly object _predictionEngineLock = new object();

        public ClassificationController(PredictionEngine<ModelInput, ModelOutput> predictionEngine)
        {
            _predictionEngine = predictionEngine;
        }

        [HttpPost]
        public async Task<string> ClassifyImage([FromBody] Dictionary<string, string> input)
        {
            string prediction;
            string imagePath = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "inputimage.jpeg");

            // Get raw image bytes
            var imageBytes = Convert.FromBase64String(input["data"]);

            using (var ms = new MemoryStream(imageBytes))
            {
                // Save the image to a file
                using (var img = await Task.Run(() => Image.FromStream(ms)))
                    await Task.Run(() => img.Save(imagePath));
            }

            lock (_predictionEngineLock)
            {
                // Use Prediction to classify image
                ModelOutput output = _predictionEngine.Predict(new ModelInput { ImageSource = imagePath });
                prediction = output.Prediction;
            }

            return prediction;
        }
    }
}