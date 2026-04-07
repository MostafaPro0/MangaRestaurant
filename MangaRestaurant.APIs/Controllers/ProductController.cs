using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Helpers;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //GET : /api/Products
        [HttpGet]
        //    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        [CashedAttribute(300)]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetAllProducts([FromQuery]ProductSpecParams specParams)
        {
            var spec = new ProductWithBrandAndCategorySpecs(specParams);

            var products = await _unitOfWork.Repository<Product>().GetAllAsyncWithSpecAsync(spec);
            var MappedProducts = _mapper.Map<IReadOnlyList<Product>,IReadOnlyList<ProductToReturnDto>>(products);

            var countSpec  = new ProductWithFiltrationForCountAsync(specParams);
            var count = await _unitOfWork.Repository<Product>().GetCountAsyncWithSpecAsync(countSpec);

            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex,specParams.PageSize,specParams,MappedProducts, count));
        }

        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET /api/Products/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductWithBrandAndCategorySpecs(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);

            if (product == null)
                return NotFound(new ApiResponse(404, "Product Not Found"));

            // Increment Views Count
            product.Views++;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        // POST /api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> CreateProduct(ProductCreateDto productDto)
        {
            var product = _mapper.Map<ProductCreateDto, Product>(productDto);
            await _unitOfWork.Repository<Product>().AddAsync(product);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem Creating Product"));

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        // PUT /api/Products/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int id, ProductCreateDto productDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetAsync(id);
            if (product == null) return NotFound(new ApiResponse(404));

            _mapper.Map(productDto, product);
            _unitOfWork.Repository<Product>().Update(product);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem Updating Product"));

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        // DELETE /api/Products/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetAsync(id);
            if (product == null) return NotFound(new ApiResponse(404));

            _unitOfWork.Repository<Product>().Delete(product);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem Deleting Product"));

            return Ok(new ApiResponse(200, "Product deleted successfully"));
        }
    }
}
