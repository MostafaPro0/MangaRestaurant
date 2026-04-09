using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications.ProductBrandSpecs;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using MangaRestaurant.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetAllProductBrands(string? Sort)
        {
            var spec = new ProductBrandSpecs(Sort);
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
    }
}
