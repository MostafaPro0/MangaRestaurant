using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications.ProductBrandSpecs;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using MangaRestaurant.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs.Resources;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductBrandController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProductBrandController(IUnitOfWork unitOfWork, IStringLocalizer<SharedResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        [ProducesResponseType(typeof(ProductBrand), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : /api/ProductBrands
        [HttpGet()]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetAllProductBrands(string? Sort, [FromQuery] bool showHidden = false)
        {
            if (showHidden && !User.IsInRole("Admin")) showHidden = false;

            var spec = new ProductBrandSpecs(Sort, showHidden);
            var productBrands = await _unitOfWork.Repository<ProductBrand>().GetAllAsyncWithSpecAsync(spec);


            return Ok(productBrands);
        }

        [ProducesResponseType(typeof(ProductBrand), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : /api/ProductBrands/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductBrand>> GetProductBrand(int id)
        {
            var spec = new ProductBrandSpecs(id);
            var productBrand = await _unitOfWork.Repository<ProductBrand>().GetEntityWithSpecAsync(spec);

            if (productBrand == null)
                return NotFound(new ApiResponse(404, _localizer["BRAND_NOT_FOUND"]));

            return Ok(productBrand);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductBrand>> CreateBrand(BrandDto brandDto)
        {
            var brand = new ProductBrand
            {
                Name = brandDto.Name,
                NameAr = brandDto.NameAr,
                IsHidden = brandDto.IsHidden
            };

            await _unitOfWork.Repository<ProductBrand>().AddAsync(brand);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem creating brand"));

            return Ok(brand);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductBrand>> UpdateBrand(int id, BrandDto brandDto)
        {
            var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
            if (brand == null) return NotFound(new ApiResponse(404, _localizer["BRAND_NOT_FOUND"]));

            brand.Name = brandDto.Name;
            brand.NameAr = brandDto.NameAr;
            brand.IsHidden = brandDto.IsHidden;

            _unitOfWork.Repository<ProductBrand>().Update(brand);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem updating brand"));

            return Ok(brand);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteBrand(int id)
        {
            var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
            if (brand == null) return NotFound(new ApiResponse(404, _localizer["BRAND_NOT_FOUND"]));

            _unitOfWork.Repository<ProductBrand>().Delete(brand);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem deleting brand"));

            return Ok(new ApiResponse(200, "Brand deleted successfully"));
        }

        [HttpPut("{id}/hide")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> HideBrand(int id, [FromQuery] bool hide = true)
        {
            var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
            if (brand == null) return NotFound(new ApiResponse(404, _localizer["BRAND_NOT_FOUND"]));

            brand.IsHidden = hide;
            _unitOfWork.Repository<ProductBrand>().Update(brand);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, hide ? "Brand hidden successfully" : "Brand shown successfully"));
        }
    }
}
