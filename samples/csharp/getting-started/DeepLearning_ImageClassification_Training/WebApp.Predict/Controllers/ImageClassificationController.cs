using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ImageClassification.WebApp;
using ImageClassification.WebApp.ImageHelpers;
using ImageClassification.WebApp.ML.DataModels;

namespace TensorFlowImageClassification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageClassificationController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;
        private readonly ILogger<ImageClassificationController> _logger;

        public ImageClassificationController(PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool, IConfiguration configuration, ILogger<ImageClassificationController> logger) //When using DI/IoC
        {
            // Get the ML Model Engine injected, for scoring.
            _predictionEnginePool = predictionEnginePool;

            Configuration = configuration;

            // Get other injected dependencies.
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("classifyimage")]
        public async Task<IActionResult> ClassifyImage(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                return BadRequest();

            var imageMemoryStream = new MemoryStream();
            await imageFile.CopyToAsync(imageMemoryStream);

            // Check that the image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage())
                return StatusCode(StatusCodes.Status415UnsupportedMediaType);

            _logger.LogInformation("Start processing image...");

            // Measure execution time.
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Set the specific image data into the ImageInputData type used in the DataView.
            var imageInputData = new InMemoryImageData { Image = imageData };

            // Predict code for provided image.
            var prediction = _predictionEnginePool.Predict(imageInputData);

            // Stop measuring time.
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");

            // Predict the image's label (The one with highest probability).
            ImagePredictedLabelWithProbability imageBestLabelPrediction =
                        new ImagePredictedLabelWithProbability()
                        {  
                            PredictedLabel = prediction.PredictedLabel,
                            Probability = prediction.Score.Max(),
                            PredictionExecutionTime = elapsedMs,
                            ImageId = imageFile.FileName
                        };

            return Ok(imageBestLabelPrediction);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }

        // GET api/ImageClassification
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "ACK Heart beat 1", "ACK Heart beat 2" };
        }
    }
}