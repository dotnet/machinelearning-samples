using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TensorFlowImageClassificationWebAPI.Infrastructure;
using TensorFlowImageClassificationWebAPI.OnnxModelScorers;

namespace TensorFlowImageClassificationWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDetectionController : ControllerBase
    {
        //Dependencies
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

            _imagesTmpFolder = ModelHelpers.GetFolderFullPath(@"ImagesTemp");
        }

        //[HttpPost]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //[Route("IdentifyObjects")]
        //public async Task<IActionResult> IdentifyObjects(IFormFile imageFile)
        //{
        //    if (imageFile.Length == 0)
        //        return BadRequest();

        //    string imageFilePath = "", fileName = "";
        //    try
        //    {
        //        //Save the temp image  into the temp-folder 
        //        fileName = await _imageWriter.UploadImageAsync(imageFile, _imagesTmpFolder);
        //        imageFilePath = Path.Combine(_imagesTmpFolder, fileName);

        //        //Convert image stream to byte[] 
        //        byte[] imageData = null;
        //        //
        //        //Image stream still not used in ML.NET 0.7 but only possible through a file
        //        //
        //        //MemoryStream image = new MemoryStream();           
        //        //await imageFile.CopyToAsync(image);
        //        //imageData = image.ToArray();
        //        //if (!imageData.IsValidImage())
        //        //    return StatusCode(StatusCodes.Status415UnsupportedMediaType);

        //       // ImagePredictedLabelWithProbability imageLabelPrediction = null;
        //        _logger.LogInformation($"Start processing image file { imageFilePath }");

        //        //Measure execution time
        //        var watch = System.Diagnostics.Stopwatch.StartNew();

        //        //Predict the image's label (The one with highest probability)
        //        List<string> objectsNames = _modelScorer.DetectObjectsUsingModel(imageFilePath);

        //        //Stop measuring time
        //        watch.Stop();
        //        var elapsedMs = watch.ElapsedMilliseconds;

        //        _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");

        //        //TODO: Commented as the file is still locked by TensorFlow or ML.NET?
        //        //_imageWriter.DeleteImageTempFile(imageFilePath);

        //        //return new ObjectResult(result);
        //        return Ok(objectsNames);
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            if(imageFilePath != string.Empty)
        //            {
        //                _logger.LogInformation($"Deleting Image {imageFilePath}");
        //                //TODO: Commented as the file is still locked by TensorFlow or ML.NET?
        //                //_imageWriter.DeleteImageTempFile(imageFilePath);
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            _logger.LogInformation("Error deleting image: " + imageFilePath);
        //        }
        //    }
        //}


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
                //Save the temp image  into the temp-folder 
                fileName = await _imageWriter.UploadImageAsync(imageFile, _imagesTmpFolder);
                imageFilePath = Path.Combine(_imagesTmpFolder, fileName);


                //Convert image stream to byte[] 
                byte[] imageData = null;
                //
                //Image stream still not used in ML.NET 0.7 but only possible through a file
                //
                //MemoryStream image = new MemoryStream();           
                //await imageFile.CopyToAsync(image);
                //imageData = image.ToArray();
                //if (!imageData.IsValidImage())
                //    return StatusCode(StatusCodes.Status415UnsupportedMediaType);

                // ImagePredictedLabelWithProbability imageLabelPrediction = null;
                _logger.LogInformation($"Start processing image file { imageFilePath }");

                //Measure execution time
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Predict the image's label (The one with highest probability)
               var objectsNames = _modelScorer.DetectObjectsUsingModel(imageFilePath);
                _modelScorer.PaintImages(imageFilePath);

                //Stop measuring time
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");

                //TODO: Commented as the file is still locked by TensorFlow or ML.NET?
                //_imageWriter.DeleteImageTempFile(imageFilePath);

                //return new ObjectResult(result);
                return Ok(objectsNames);
            }
            catch(Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                return BadRequest();
            }
            //finally
            //{
            //    try
            //    {
            //        if (imageFilePath != string.Empty)
            //        {
            //            _logger.LogInformation($"Deleting Image {imageFilePath}");
            //            //TODO: Commented as the file is still locked by TensorFlow or ML.NET?
            //            //_imageWriter.DeleteImageTempFile(imageFilePath);
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        _logger.LogInformation("Error deleting image: " + imageFilePath);
            //    }
            //}
        }

        // GET api/ImageClassification
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "ACK Heart beat 1", "ACK Heart beat 2" };
        }       

    }
}
