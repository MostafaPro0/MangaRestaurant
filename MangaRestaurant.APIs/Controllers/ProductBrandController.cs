using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications.EmployeeSpecs;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using MangaRestaurant.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductBrandController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductBrandController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            var productBrand = await _unitOfWork.Repository<ProductBrand>().GetAsyncWithSpecAsync(spec);

            if (productBrand == null)
                return NotFound(new ApiResponse(404, "Product Brand Not Found"));

            return Ok(productBrand);
        }
    }
}
