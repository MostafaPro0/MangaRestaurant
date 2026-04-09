using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications.ProductCategorySpecs;
using MangaRestaurant.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetAllProductCategories(string? Sort, [FromQuery] bool showHidden = false)
        {
            if (showHidden && !User.IsInRole("Admin")) showHidden = false;

            var spec = new ProductCategorySpecs(Sort, showHidden);
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductCategory>> CreateCategory(CategoryDto categoryDto)
        {
            var category = new ProductCategory
            {
                Name = categoryDto.Name,
                NameAr = categoryDto.NameAr,
                IsHidden = categoryDto.IsHidden
            };

            await _unitOfWork.Repository<ProductCategory>().AddAsync(category);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["CATEGORY_CREATE_FAILED"]));

            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductCategory>> UpdateCategory(int id, CategoryDto categoryDto)
        {
            var category = await _unitOfWork.Repository<ProductCategory>().GetAsync(id);
            if (category == null) return NotFound(new ApiResponse(404, _localizer["CATEGORY_NOT_FOUND"]));

            category.Name = categoryDto.Name;
            category.NameAr = categoryDto.NameAr;
            category.IsHidden = categoryDto.IsHidden;

            _unitOfWork.Repository<ProductCategory>().Update(category);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["CATEGORY_UPDATE_FAILED"]));

            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var category = await _unitOfWork.Repository<ProductCategory>().GetAsync(id);
            if (category == null) return NotFound(new ApiResponse(404, _localizer["CATEGORY_NOT_FOUND"]));

            _unitOfWork.Repository<ProductCategory>().Delete(category);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["CATEGORY_DELETE_FAILED"]));

            return Ok(new ApiResponse(200, _localizer["CATEGORY_DELETE_SUCCESS"]));
        }

        [HttpPut("{id}/hide")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> HideCategory(int id, [FromQuery] bool hide = true)
        {
            var category = await _unitOfWork.Repository<ProductCategory>().GetAsync(id);
            if (category == null) return NotFound(new ApiResponse(404, _localizer["CATEGORY_NOT_FOUND"]));

            category.IsHidden = hide;
            _unitOfWork.Repository<ProductCategory>().Update(category);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, hide ? _localizer["CATEGORY_HIDE_SUCCESS"] : _localizer["CATEGORY_SHOW_SUCCESS"]));
        }
    }
}
