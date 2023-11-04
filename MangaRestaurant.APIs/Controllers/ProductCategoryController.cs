using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications.EmployeeSpecs;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductCategoryController : BaseApiController
    {
        private readonly IGenericRepository<ProductCategory> _productCategoryRepo;

        public ProductCategoryController(IGenericRepository<ProductCategory> productCategoryRepo)
        {
            _productCategoryRepo = productCategoryRepo;
        }

        [ProducesResponseType(typeof(ProductCategory), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : /api/ProductCategories
        [HttpGet()]
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetAllProductCategories(string? Sort)
        {
            var spec = new ProductCategorySpecs(Sort);
            var productCategories = await _productCategoryRepo.GetAllAsyncWithSpecAsync(spec);


            return Ok(productCategories);
        }

        [ProducesResponseType(typeof(ProductCategory), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : /api/ProductCategories/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCategory>> GetProductCategory(int id)
        {
            var spec = new ProductCategorySpecs(id);
            var productCategory = await _productCategoryRepo.GetAsyncWithSpecAsync(spec);

            if (productCategory == null)
                return NotFound(new ApiResponse(404, "Product Category Not Found"));

            return Ok(productCategory);
        }
    }
}
