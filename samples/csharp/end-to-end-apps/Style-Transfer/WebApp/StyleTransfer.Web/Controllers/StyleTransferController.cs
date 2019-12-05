using System;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using StyleTransfer.Web.Services;

namespace StyleTransfer.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly Predictor _predictor = new Predictor();

        [HttpPost]
        public IActionResult Post(TransferRequest request)
        {
            if (request.Data == "")
            {
                return BadRequest();
            }

            var prediction = _predictor.RunPrediction(request.Data);
            return Ok(new TransferResponse
            {
                Base64Image = "data:image/png;base64," + prediction
            });
        }

        public class TransferRequest
        {
            public string Filter { get; set; }
            public string Data { get; set; }
        }

        public class TransferResponse
        {
            public string Base64Image { get; set; }
        }
    }
}