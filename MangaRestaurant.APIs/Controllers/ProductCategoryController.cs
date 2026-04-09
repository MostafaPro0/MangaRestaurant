using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications.ProductCategorySpecs;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs.Resources;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductCategoryController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProductCategoryController(IUnitOfWork unitOfWork, IStringLocalizer<SharedResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        [ProducesResponseType(typeof(ProductCategory), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : /api/ProductCategories
        [HttpGet()]
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetAllProductCategories(string? Sort)
        {
            var spec = new ProductCategorySpecs(Sort);
            var productCategories = await _unitOfWork.Repository<ProductCategory>().GetAllAsyncWithSpecAsync(spec);


            return Ok(productCategories);
        }

        [ProducesResponseType(typeof(ProductCategory), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : /api/ProductCategories/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCategory>> GetProductCategory(int id)
        {
            var spec = new ProductCategorySpecs(id);
            var productCategory = await _unitOfWork.Repository<ProductCategory>().GetEntityWithSpecAsync(spec);

            if (productCategory == null)
                return NotFound(new ApiResponse(404, _localizer["CATEGORY_NOT_FOUND"]));

            return Ok(productCategory);
        }
    }
}
