using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnnxObjectDetectionWebAPI.Infrastructure;
using OnnxObjectDetectionWebAPI.OnnxModelScorers;

namespace OnnxObjectDetectionWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDetectionController : ControllerBase
    {
        private readonly IImageFileWriter _imageWriter; 
        private readonly string _imagesTmpFolder;        

        private readonly ILogger<ObjectDetectionController> _logger;
        private readonly IOnnxModelScorer _modelScorer;

        public ObjectDetectionController(IOnnxModelScorer modelScorer, ILogger<ObjectDetectionController> logger, IImageFileWriter imageWriter) //When using DI/IoC (IImageFileWriter imageWriter)
        {
            //Get injected dependencies
            _modelScorer = modelScorer;
            _logger = logger;
            _imageWriter = imageWriter;
            _imagesTmpFolder = ModelHelpers.GetAbsolutePath(@"../../../ImagesTemp");
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("IdentifyObjects")]
        public async Task<IActionResult> IdentifyObjects(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                return BadRequest();

            string imageFilePath = "", fileName = "";
            try
            {
                //Save the temp image into the temp-folder 
                fileName = await _imageWriter.UploadImageAsync(imageFile, _imagesTmpFolder);
                imageFilePath = Path.Combine(_imagesTmpFolder, fileName);

                _logger.LogInformation($"Start processing image file { imageFilePath }");

                //Measure execution time
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Predict the objects in the image
                var objectsNames = _modelScorer.DetectObjectsUsingModel(imageFilePath);
                _modelScorer.PaintImages(imageFilePath);

                //Stop measuring time
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");
                
                return Ok(objectsNames);
            }
            catch(Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                return BadRequest();
            }
        }

        // GET api/ImageClassification
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "ACK Heart beat 1", "ACK Heart beat 2" };
        }       

    }
}
