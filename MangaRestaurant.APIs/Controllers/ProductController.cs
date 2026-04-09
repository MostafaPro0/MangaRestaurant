using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Helpers;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Service;
using MangaRestaurant.Core.Specifications;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs;

namespace MangaRestaurant.APIs.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IStringLocalizer<SharedResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _localizer = localizer;
        }

        [ProducesResponseType(typeof(Pagination<ProductToReturnDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //GET : /api/Products
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            // Only Admins can see hidden products/categories/brands
            if (specParams.ShowHidden && !User.IsInRole("Admin"))
            {
                specParams.ShowHidden = false;
            }

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
                return NotFound(new ApiResponse(404, _localizer["PRODUCT_NOT_FOUND"]));

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

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["PRODUCT_CREATE_FAILED"]));

            await _notificationService.SendNewProductNotification(product.Name);

            // Re-fetch with relations for proper DTO return
            var spec = new ProductWithBrandAndCategorySpecs(product.Id);
            var reloadedProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductToReturnDto>(reloadedProduct));
        }

        // PUT /api/Products/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int id, ProductCreateDto productDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetAsync(id);
            if (product == null) return NotFound(new ApiResponse(404, _localizer["PRODUCT_NOT_FOUND"]));

            // Capture old price before mapping
            var oldPrice = product.Price;

            // Preserve the existing picture URL because updates are handled by a separate upload endpoint
            var existingPictureUrl = product.PictureUrl;

            _mapper.Map(productDto, product);

            // Restore the original URL
            product.PictureUrl = existingPictureUrl;

            _unitOfWork.Repository<Product>().Update(product);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["PRODUCT_UPDATE_FAILED"]));

            // Notify all clients if the price has changed so they can update their baskets in real-time
            if (product.Price != oldPrice)
            {
                await _notificationService.SendPriceUpdatedNotification(product.Id, product.Name, product.NameAr, product.Price);
            }

            // Re-fetch with relations for proper DTO return
            var spec = new ProductWithBrandAndCategorySpecs(id);
            var reloadedProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductToReturnDto>(reloadedProduct));
        }

        // DELETE /api/Products/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetAsync(id);
            if (product == null) return NotFound(new ApiResponse(404, _localizer["PRODUCT_NOT_FOUND"]));

            _unitOfWork.Repository<Product>().Delete(product);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["PRODUCT_DELETE_FAILED"]));

            return Ok(new ApiResponse(200, _localizer["PRODUCT_DELETE_SUCCESS"]));
        }

        [HttpGet("deals")]
        [CashedAttribute(30)]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetDeals()
        {
            var spec = new ProductWithBrandAndCategorySpecs();
            var products = await _unitOfWork.Repository<Product>().GetAllAsyncWithSpecAsync(spec);
            
            var deals = products.Where(p => p.OldPrice > p.Price).Take(8).ToList();
            
            return Ok(_mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(deals));
        }

        [HttpGet("latest")]
        [CashedAttribute(30)]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetLatestProducts()
        {
            var spec = new ProductWithBrandAndCategorySpecs(); // In a real app, you'd add ordering to the spec
            var products = await _unitOfWork.Repository<Product>().GetAllAsyncWithSpecAsync(spec);
            
            var latest = products.OrderByDescending(p => p.Id).Take(8).ToList();
            
            return Ok(_mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(latest));
        }
    }
}
