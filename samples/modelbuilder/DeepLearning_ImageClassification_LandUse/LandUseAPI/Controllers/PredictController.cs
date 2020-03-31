using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LandUseUWPML.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.Extensions.ML;

namespace LandUseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        //private static object _lock = new object();

        public PredictController(PredictionEngine<ModelInput,ModelOutput> predictionEngine)
        {
            _predictionEngine = predictionEngine;
        }

        [HttpPost]
        public async Task<IActionResult> Predict([FromBody]HttpContent imageString)
        {
            var imgContent = await imageString.ReadAsStringAsync();
            var imageBytes = Convert.FromBase64String(imgContent);

            using (var imgStream = new MemoryStream(imageBytes))
            {
                var img = Image.FromStream(imgStream);
                string fileName = $"{img.GetHashCode()}.jpeg";
                img.Save(fileName);

                ModelInput input = new ModelInput { ImageSource = fileName };
                var prediction = _predictionEngine.Predict(input);
                return new OkObjectResult(prediction.Prediction);
            }

            //using (var imgStream = new MemoryStream(imageBytes))
            //{
            //    var img = Image.FromStream(imgStream);

            //    string fileName = $"{img.GetHashCode()}.jpeg";
            //    img.Save(fileName);

            //    ModelInput input = new ModelInput { ImageSource = fileName };

            //    var prediction = _predictionEngine.Predict(input);

            //    return new OkObjectResult(prediction);
            //}
        }
    }
}