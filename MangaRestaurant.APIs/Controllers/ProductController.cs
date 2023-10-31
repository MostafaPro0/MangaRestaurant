using AutoMapper;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Helpers;
using MangaRestaurant.Core.Dtos;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IMapper _mapper;

        public ProductController(IGenericRepository<Product> productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }

        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //GET : /api/Products
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetAllProducts([FromQuery]ProductSpecParams specParams)
        {
            var spec = new ProductWithBrandAndCategorySpecs(specParams);

            var products = await _productRepo.GetAllAsyncWithSpecAsync(spec);
            var MappedProducts = _mapper.Map<IReadOnlyList<Product>,IReadOnlyList<ProductToReturnDto>>(products);

            var countSpec  = new ProductWithFiltrationForCountAsync(specParams);
            var count = await _productRepo.GetCountAsyncWithSpecAsync(countSpec);

            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex,specParams.PageSize,specParams,MappedProducts, count));
        }

        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET /api/Products/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductWithBrandAndCategorySpecs(id);
            var product = await _productRepo.GetAsyncWithSpecAsync(spec);

            if (product == null)
                return NotFound(new ApiResponse(404, "Product Not Found"));

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }
    }
}
