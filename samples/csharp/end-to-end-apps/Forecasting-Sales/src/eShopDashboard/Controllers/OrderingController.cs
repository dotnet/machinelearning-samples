using eShopDashboard.Infrastructure.Extensions;
using eShopDashboard.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/ordering")]
    public class OrderingController : Controller
    {
        private readonly IOrderingQueries _queries;

        public OrderingController(IOrderingQueries queries)
        {
            _queries = queries;
        }

        [HttpGet("product/{productId}/history")]
        public async Task<IActionResult> ProductHistory(string productId)
        {
            if (productId.IsBlank() || productId.IsNotAnInt()) return BadRequest();

            IEnumerable<dynamic> items = await _queries.GetProductHistoryAsync(productId);

            return Ok(items);
        }

        [HttpGet("product/{productId}/stats")]
        public async Task<IActionResult> ProductStats(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return BadRequest();

            IEnumerable<dynamic> stats = await _queries.GetProductStatsAsync(productId);

            return Ok(stats);
        }

        [HttpGet("product/stats")]
        public async Task<IActionResult> ProductStats()
        {
            IEnumerable<dynamic> items = await _queries.GetProductStatsAsync();

            var typedOrderItems = items
                .Select(c => new { c.next, c.productId, c.year, c.month, c.units, c.avg, c.count, c.max, c.min, c.prev })
                .ToList();

            var csvFile = File(Encoding.UTF8.GetBytes(typedOrderItems.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "products.stats.csv";
            return csvFile;
        }
    }
}