using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnnxObjectDetectionE2EAPP
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {

        //[HttpGet]
        //public string Detect(string url)
        //{
        //    return url;
        //}

        //// GET: api/<controller>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<controller>/5
        [HttpGet()]
        public Result Get([FromQuery]string url)
        {
            return new Result { ImageURL = url };
        }

        public class Result
        {
            public string ImageURL { get; set; }
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
