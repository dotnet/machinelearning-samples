using eShopDashboard.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/catalog")]
    public class CatalogController : Controller
    {
        private readonly ICatalogQueries _catalogQueries;
        private readonly IOrderingQueries _orderingQueries;

        public CatalogController(ICatalogQueries queries, IOrderingQueries orderingQueries)
        {
            _catalogQueries = queries;
            _orderingQueries = orderingQueries;
        }

        // GET: api/Catalog
        [HttpGet("productSetDetailsByDescription")]
        public async Task<IActionResult> SimilarProducts([FromQuery]string description)
        {
            const int minDepthOrderingThreshold = 9;

            if (string.IsNullOrEmpty(description))
                return BadRequest();

            var items = await _catalogQueries.GetProductsByDescriptionAsync(description);

            if (!items.Any()) return Ok();

            var products = items.Select(c => c.Id).Cast<int>();
            var depth = await _orderingQueries.GetProductsHistoryDepthAsync(products);

            items = items.Join(depth, l => l.Id.ToString(), r => r.ProductId.ToString(), (l,r) => new {l,r})
                .Where(j => j.r.count > minDepthOrderingThreshold)
                .Select(j => j.l);

            return Ok(items);
        }
    }
}