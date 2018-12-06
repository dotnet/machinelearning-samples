using eShopDashboard.Queries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace eShopDashboard.Controllers
{
    public class PicController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ICatalogQueries _queries;

        public PicController(
            IHostingEnvironment env,
            ICatalogQueries queries)
        {
            _env = env;
            _queries = queries;
        }

        [HttpGet("api/catalog/items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // GET: /<controller>/
        public async Task<IActionResult> GetImage(int catalogItemId)
        {
            if (catalogItemId <= 0)
            {
                return BadRequest();
            }

            var item = await _queries.GetCatalogItemById(catalogItemId);

            if (item != null)
            {
                if (string.IsNullOrEmpty(item.PictureFileName))
                    return BlankImage();

                var contentRootPath = _env.ContentRootPath;

                var path = Path.Combine(contentRootPath, "ProductImages", item.PictureFileName);

                if (!System.IO.File.Exists(path))
                    return BlankImage();

                string imageFileExtension = Path.GetExtension(item.PictureFileName);
                string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = System.IO.File.ReadAllBytes(path);

                return File(buffer, mimetype);
            }

            return BlankImage();
        }

        private IActionResult BlankImage()
        {
            const string blankImage = "coming_soon.png";
            var pathBlankImage = Path.Combine(_env.WebRootPath, "images", blankImage);

            string imageFileExtension = Path.GetExtension(pathBlankImage);
            string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

            var buffer = System.IO.File.ReadAllBytes(pathBlankImage);

            return File(buffer, mimetype);
        }

        private string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            string mimetype;

            switch (extension)
            {
                case ".png":
                    mimetype = "image/png";
                    break;

                case ".gif":
                    mimetype = "image/gif";
                    break;

                case ".jpg":
                case ".jpeg":
                    mimetype = "image/jpeg";
                    break;

                case ".bmp":
                    mimetype = "image/bmp";
                    break;

                case ".tiff":
                    mimetype = "image/tiff";
                    break;

                case ".wmf":
                    mimetype = "image/wmf";
                    break;

                case ".jp2":
                    mimetype = "image/jp2";
                    break;

                case ".svg":
                    mimetype = "image/svg+xml";
                    break;

                default:
                    mimetype = "application/octet-stream";
                    break;
            }

            return mimetype;
        }
    }
}