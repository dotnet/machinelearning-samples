using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnnxObjectDetectionE2EAPP.Infrastructure;
using OnnxObjectDetectionE2EAPP.OnnxModelScorers;

namespace OnnxObjectDetectionE2EAPP.Controllers
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
        }

        public class Result
        {
            public string imageString { get; set; }
        }

        [HttpGet()]
        public Result Get([FromQuery]string url)
        {
            string imageFileRelativePath = @"../../.." + url;
            string imageFilePath = ModelHelpers.GetAbsolutePath(imageFileRelativePath);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Predict the objects in the image
            var objectsNames = _modelScorer.DetectObjectsUsingModel(imageFilePath);
            var img = _modelScorer.PaintImages(imageFilePath);            
            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return new Result { imageString = base64String };
            }            
        }
    }
}
