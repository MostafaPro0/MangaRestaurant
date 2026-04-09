using MangaRestaurant.APIs.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Controllers
{
    public class UploadController : BaseApiController
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UploadController(IWebHostEnvironment environment, IStringLocalizer<SharedResource> localizer)
        {
            _environment = environment;
            _localizer = localizer;
        }

        [HttpPost("image")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse(400, _localizer["NO_FILE_UPLOADED"]));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new ApiResponse(400, _localizer["INVALID_FILE_TYPE"]));

            var folderPath = Path.Combine(_environment.WebRootPath, "Images", "Products");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative path starting with Images/Products/
            return Ok($"Images/Products/{fileName}");
        }
    }
}
