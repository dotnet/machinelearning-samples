using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Drawing;
using Microsoft.ML;
using LandUseML.Model;

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
            string imagePath = "inputimage.jpeg";

            var imageBytes = Convert.FromBase64String(input["data"]);

            using (var ms = new MemoryStream(imageBytes))
            {
                using (var img = await Task.Run(() => Image.FromStream(ms)))
                    await Task.Run(() => img.Save(imagePath));
            }

            lock (_predictionEngineLock)
            {
                ModelOutput output = _predictionEngine.Predict(new ModelInput { ImageSource = imagePath });
                prediction = output.Prediction;
            }

            return prediction;
        }
    }
}