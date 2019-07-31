using Microsoft.AspNetCore.Mvc;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/seeding")]
    public class SeedingProgressController : Controller
    {
        // GET: api/SeedingProgress
        [HttpGet("progress")]
        public IActionResult GetCurrent()
        {
            return Ok(Program.GetSeedingProgress());
        }
    }
}