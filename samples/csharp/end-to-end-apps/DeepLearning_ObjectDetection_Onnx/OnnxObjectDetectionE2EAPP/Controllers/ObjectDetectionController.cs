using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnnxObjectDetectionE2EAPP.Infrastructure;
using OnnxObjectDetectionE2EAPP.Services;
using OnnxObjectDetectionE2EAPP.Utilities;

namespace OnnxObjectDetectionE2EAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDetectionController : ControllerBase
    {
        private readonly IImageFileWriter _imageWriter; 
        private readonly string _imagesTmpFolder;        

        private readonly ILogger<ObjectDetectionController> _logger;
        private readonly IObjectDetectionService _objectDetectionService;

        private string base64String = string.Empty;

        public ObjectDetectionController(IObjectDetectionService ObjectDetectionService, ILogger<ObjectDetectionController> logger, IImageFileWriter imageWriter) //When using DI/IoC (IImageFileWriter imageWriter)
        {
            //Get injected dependencies
            _objectDetectionService = ObjectDetectionService;
            _logger = logger;
            _imageWriter = imageWriter;
            _imagesTmpFolder = CommonHelpers.GetAbsolutePath(@"../../../ImagesTemp");
        }

        public class Result
        {
            public string imageString { get; set; }
        }

        [HttpGet()]
        public IActionResult Get([FromQuery]string url)
        {
            string imageFileRelativePath = @"../../.." + url;
            string imageFilePath = CommonHelpers.GetAbsolutePath(imageFileRelativePath);
            try
            {
                //Detect the objects in the image                
                var result = DetectAndPaintImage(imageFilePath);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                return BadRequest();
            }
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
                
                //Detect the objects in the image                
                var result = DetectAndPaintImage(imageFilePath);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                return BadRequest();
            }
        }

        private Result DetectAndPaintImage(string imageFilePath)
        {
            //Predict the objects in the image
            _objectDetectionService.DetectObjectsUsingModel(imageFilePath);
            var img = _objectDetectionService.PaintImages(imageFilePath);

            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                base64String = Convert.ToBase64String(imageBytes);
                var result = new Result { imageString = base64String };
                return result;
            }
        }
    }
}
